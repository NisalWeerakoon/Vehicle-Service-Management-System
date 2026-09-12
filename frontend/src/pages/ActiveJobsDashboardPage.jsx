import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import AdminSidebar from '../components/AdminSidebar'
import ServiceAdvisorSidebar from '../components/ServiceAdvisorSidebar'
import ProtectedRoute from '../components/ProtectedRoute'
import { activeJobsReportApi, getRole } from '../services/api'

const STATUS_OPTIONS = [
  'Created',
  'Awaiting Inspection',
  'Inspected',
  'Awaiting Parts',
  'In Progress',
  'Completed',
  'Ready for Collection',
]

function statusClass(status) {
  return `active-job-status report-status-${status
    .toLowerCase()
    .replace(/\s+/g, '-')
    .replace(/[^a-z-]/g, '')}`
}

function formatDate(value) {
  if (!value) return '—'

  return new Date(value).toLocaleString()
}

function ActiveJobsDashboardContent() {
  const navigate = useNavigate()
  const role = getRole()

  const [report, setReport] = useState(null)
  const [statusFilter, setStatusFilter] = useState('')
  const [grouped, setGrouped] = useState(true)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const loadReport = useCallback(async (status = '') => {
    setLoading(true)
    setError('')

    try {
      const data = await activeJobsReportApi.getActiveJobs(status)
      setReport(data)
    } catch (err) {
      setError(err.message || 'Unable to load active jobs report.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadReport(statusFilter)
  }, [loadReport, statusFilter])

  const groupedJobs = useMemo(() => {
    if (!report) return []

    return STATUS_OPTIONS
      .map((status) => ({
        status,
        jobs: report.jobs.filter((job) => job.status === status),
      }))
      .filter((group) => group.jobs.length > 0)
  }, [report])

  function handleStatusChange(event) {
    setStatusFilter(event.target.value)
  }

  function renderJobRow(job) {
    return (
      <tr key={job.id}>
        <td>
          <strong>{job.jobCardNumber}</strong>
        </td>
        <td>{job.vehicleRegistrationNumber}</td>
        <td>#{job.customerId}</td>
        <td>{job.assignedMechanicName || 'Not assigned'}</td>
        <td>
          <span className={statusClass(job.status)}>
            {job.status}
          </span>
        </td>
        <td>{formatDate(job.updatedAt || job.createdAt)}</td>
        <td>
          <button
            className="active-job-action"
            onClick={() => navigate(`/jobs/${job.id}/status`)}
          >
            View Status
          </button>
        </td>
      </tr>
    )
  }

  return (
    <div className="portal-layout">
      {role === 'Administrator' ? (
        <AdminSidebar activeTab="reports" />
      ) : (
        <ServiceAdvisorSidebar />
      )}

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">OPERATIONS REPORT</span>
            <h1>Active Jobs Dashboard</h1>
            <p className="report-subtitle">
              Monitor current service-center workload by job status.
            </p>
          </div>

          <div className="report-header-actions">
            <button
              className="secondary-button small-button"
              onClick={() => loadReport(statusFilter)}
              disabled={loading}
            >
              {loading ? 'Refreshing...' : '↻ Refresh'}
            </button>
          </div>
        </header>

        <section className="active-jobs-report">
          {error && (
            <div className="alert error-alert">
              {error}
            </div>
          )}

          <div className="active-job-summary">
            <div className="active-job-total-card">
              <span>Total Active Jobs</span>
              <strong>{report?.totalActiveJobs ?? 0}</strong>
              <small>
                {report?.appliedStatusFilter
                  ? `Filtered: ${report.appliedStatusFilter}`
                  : 'All non-closed jobs'}
              </small>
            </div>

            {report?.statusCounts?.map((item) => (
              <button
                type="button"
                key={item.status}
                className={
                  statusFilter === item.status
                    ? 'active-job-count-card selected'
                    : 'active-job-count-card'
                }
                onClick={() =>
                  setStatusFilter((current) =>
                    current === item.status ? '' : item.status,
                  )
                }
              >
                <span>{item.status}</span>
                <strong>{item.count}</strong>
              </button>
            ))}
          </div>

          <section className="portal-card active-job-report-card">
            <div className="active-job-toolbar">
              <div>
                <h2>Current Service Jobs</h2>
                <p>
                  Filter the operational workload or view it grouped by status.
                </p>
              </div>

              <div className="active-job-filters">
                <label>
                  <span>Status</span>
                  <select
                    value={statusFilter}
                    onChange={handleStatusChange}
                  >
                    <option value="">All active statuses</option>
                    {STATUS_OPTIONS.map((status) => (
                      <option key={status} value={status}>
                        {status}
                      </option>
                    ))}
                  </select>
                </label>

                <button
                  type="button"
                  className="secondary-button small-button"
                  onClick={() => setGrouped((current) => !current)}
                >
                  {grouped ? 'Show All Jobs' : 'Group by Status'}
                </button>
              </div>
            </div>

            {loading ? (
              <div className="report-empty-state">
                <div className="spinner" style={{ margin: '0 auto 14px' }} />
                <p>Loading active jobs...</p>
              </div>
            ) : report?.jobs?.length ? (
              grouped ? (
                <div className="active-job-groups">
                  {groupedJobs.map((group) => (
                    <div className="active-job-group" key={group.status}>
                      <div className="active-job-group-heading">
                        <h3>{group.status}</h3>
                        <span>{group.jobs.length} job(s)</span>
                      </div>

                      <div className="active-job-table-wrapper">
                        <table className="active-job-table">
                          <thead>
                            <tr>
                              <th>Job Card</th>
                              <th>Vehicle</th>
                              <th>Customer</th>
                              <th>Mechanic</th>
                              <th>Status</th>
                              <th>Last Updated</th>
                              <th>Action</th>
                            </tr>
                          </thead>
                          <tbody>
                            {group.jobs.map(renderJobRow)}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="active-job-table-wrapper">
                  <table className="active-job-table">
                    <thead>
                      <tr>
                        <th>Job Card</th>
                        <th>Vehicle</th>
                        <th>Customer</th>
                        <th>Mechanic</th>
                        <th>Status</th>
                        <th>Last Updated</th>
                        <th>Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      {report.jobs.map(renderJobRow)}
                    </tbody>
                  </table>
                </div>
              )
            ) : (
              <div className="report-empty-state">
                <div className="empty-state-icon">📋</div>
                <h3>No active jobs found</h3>
                <p>
                  There are no jobs matching the selected status.
                </p>
              </div>
            )}

            {report && (
              <div className="report-footer">
                <span>
                  Showing {report.jobs.length} job(s)
                </span>
                <span>
                  Report generated: {formatDate(report.generatedAt)}
                </span>
              </div>
            )}
          </section>
        </section>
      </main>
    </div>
  )
}

function ActiveJobsDashboardPage() {
  return (
    <ProtectedRoute roles={['ServiceAdvisor', 'Administrator']}>
      <ActiveJobsDashboardContent />
    </ProtectedRoute>
  )
}

export default ActiveJobsDashboardPage
