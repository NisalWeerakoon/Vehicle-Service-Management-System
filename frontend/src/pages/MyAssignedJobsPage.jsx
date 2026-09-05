import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import CustomerSidebar from '../components/CustomerSidebar'
import { clearAuth, jobCardApi, mechanicAssignmentApi } from '../services/api'

function MyAssignedJobsPage() {
  const navigate = useNavigate()
  const [assignments, setAssignments] = useState([])
  const [jobs, setJobs] = useState({})
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = async () => {
    try {
      setLoading(true)
      const data = await mechanicAssignmentApi.getMyJobs()
      setAssignments(data)

      const details = await Promise.all(
        data.map(async (assignment) => {
          try {
            const job = await jobCardApi.getById(assignment.jobCardId)
            return [assignment.jobCardId, job]
          } catch {
            return [assignment.jobCardId, null]
          }
        }),
      )

      setJobs(Object.fromEntries(details))
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
  }, [])

  return (
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">MECHANIC INTERFACE</span>
            <h1>My Assigned Jobs</h1>
          </div>

          <button
            className="portal-primary-button"
            type="button"
            onClick={load}
          >
            ↻ Refresh Jobs
          </button>
        </header>

        <div className="portal-content">
          <section className="profile-welcome">
            <div>
              <span className="profile-welcome-label">MECHANIC WORKSPACE</span>
              <h2>Your Maintenance Queue</h2>
              <p>View and manage all active service jobs assigned directly to your account.</p>
            </div>
          </section>

          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          <section className="checkin-card" style={{ marginTop: '24px' }}>
            {loading ? (
              <div className="portal-loading-card" style={{ marginTop: '20px' }}>
                <div className="loading-spinner" />
                <p>Loading assigned jobs...</p>
              </div>
            ) : assignments.length === 0 ? (
              <p style={{ marginTop: '20px', color: '#6b7280' }}>No jobs are currently assigned to you.</p>
            ) : (
              <div className="job-card-list">
                {assignments.map((assignment) => {
                  const job = jobs[assignment.jobCardId]
                  return (
                    <div className="job-card-row" key={assignment.id}>
                      <span>
                        <strong>{job?.jobCardNumber || `Job #${assignment.jobCardId}`}</strong>
                        <small>{job?.vehicleRegistrationNumber || 'Vehicle details unavailable'}</small>
                      </span>
                      <span>
                        <strong>{job?.status || 'Assigned'}</strong>
                        <small>{job?.reportedProblems || 'No reported problems'}</small>
                      </span>
                      <span>
                        <strong>Assigned</strong>
                        <small>{new Date(assignment.assignedAt).toLocaleString()}</small>
                      </span>
                    </div>
                  )
                })}
              </div>
            )}
          </section>
        </div>
      </main>
    </div>
  )
}

export default MyAssignedJobsPage
