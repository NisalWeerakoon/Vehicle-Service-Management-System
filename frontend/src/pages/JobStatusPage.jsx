import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import CustomerSidebar from '../components/CustomerSidebar'
import { clearAuth, jobCardApi, jobStatusApi } from '../services/api'

function JobStatusPage() {
  const { jobCardId } = useParams()
  const navigate = useNavigate()
  const [job, setJob] = useState(null)
  const [statusInfo, setStatusInfo] = useState(null)
  const [history, setHistory] = useState([])
  const [selectedStatus, setSelectedStatus] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const load = async () => {
    try {
      setLoading(true)
      setError('')

      const [jobData, statusData, historyData] = await Promise.all([
        jobCardApi.getById(jobCardId),
        jobStatusApi.getStatus(jobCardId),
        jobStatusApi.getHistory(jobCardId),
      ])

      setJob(jobData)
      setStatusInfo(statusData)
      setHistory(historyData)
      setSelectedStatus(statusData.allowedNextStatuses?.[0] || '')
    } catch (err) {
      if (err.status === 401 || err.status === 403) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [jobCardId])

  const updateStatus = async (event) => {
    event.preventDefault()
    if (!selectedStatus) return

    try {
      setSaving(true)
      setError('')
      setMessage('')
      await jobStatusApi.transition(jobCardId, selectedStatus)
      setMessage(`Job status updated to "${selectedStatus}".`)
      await load()
    } catch (err) {
      if (err.status === 401 || err.status === 403) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(err.message)
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="portal-layout">
        <CustomerSidebar />
        <main className="portal-main">
          <div className="portal-content">
            <div className="portal-loading-card">
              <div className="loading-spinner" />
              <p>Loading job status...</p>
            </div>
          </div>
        </main>
      </div>
    )
  }

  return (
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">JOB & MAINTENANCE</span>
            <h1>Track Job Progress</h1>
          </div>
          <button className="portal-secondary-button" type="button" onClick={() => navigate(-1)}>
            ← Back
          </button>
        </header>

        <div className="portal-content">
          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          {message && (
            <div className="checkin-alert success">
              <strong>{message}</strong>
            </div>
          )}

          {job && (
            <>
              <section className="profile-welcome">
                <div>
                  <span className="profile-welcome-label">JOB CARD</span>
                  <h2>{job.jobCardNumber}</h2>
                  <p>{job.vehicleRegistrationNumber} · {job.reportedProblems}</p>
                </div>
                <div className="booking-status status-inservice">{job.status}</div>
              </section>

              <section className="checkin-card" style={{ marginTop: '24px' }}>
                <h2>Controlled Status Update</h2>
                <p className="form-hint">
                  Only valid next statuses are available. Every accepted change is timestamped.
                </p>

                {statusInfo?.allowedNextStatuses?.length ? (
                  <form onSubmit={updateStatus} style={{ marginTop: '18px', display: 'grid', gap: '14px', maxWidth: '520px' }}>
                    <label>
                      Next Status
                      <select
                        value={selectedStatus}
                        onChange={(event) => setSelectedStatus(event.target.value)}
                        required
                      >
                        {statusInfo.allowedNextStatuses.map((status) => (
                          <option key={status} value={status}>{status}</option>
                        ))}
                      </select>
                    </label>

                    <button className="checkin-submit-button" type="submit" disabled={saving}>
                      {saving ? 'Updating...' : 'Update Job Status'}
                    </button>
                  </form>
                ) : (
                  <div className="alert success-alert" style={{ marginTop: '18px' }}>
                    This job has no further status transitions.
                  </div>
                )}
              </section>

              <section className="checkin-card" style={{ marginTop: '24px' }}>
                <div className="section-title-row">
                  <div>
                    <h2>Status History</h2>
                    <p className="form-hint">Audit trail for major job-status changes.</p>
                  </div>
                  <button className="portal-secondary-button" type="button" onClick={load}>
                    ↻ Refresh
                  </button>
                </div>

                {history.length === 0 ? (
                  <p style={{ marginTop: '20px', color: '#6b7280' }}>
                    No status changes have been recorded yet.
                  </p>
                ) : (
                  <div className="job-card-list" style={{ marginTop: '18px' }}>
                    {history.map((item) => (
                      <div className="job-card-row" key={item.id}>
                        <span>
                          <strong>{item.fromStatus}</strong>
                          <small>From status</small>
                        </span>
                        <span>
                          <strong>{item.toStatus}</strong>
                          <small>To status</small>
                        </span>
                        <span>
                          <strong>{item.changedByRole}</strong>
                          <small>{item.changedBy}</small>
                        </span>
                        <span>
                          <strong>{new Date(item.changedAt).toLocaleString()}</strong>
                          <small>Changed at</small>
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </section>
            </>
          )}
        </div>
      </main>
    </div>
  )
}

export default JobStatusPage
