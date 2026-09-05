import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
    <main className="job-cards-page">
      <div className="check-in-shell">
        <div className="check-in-header">
          <div>
            <p className="eyebrow">MECHANIC INTERFACE</p>
            <h1>My Assigned Jobs</h1>
            <p>View only the jobs assigned to your mechanic account.</p>
          </div>
          <button type="button" className="secondary-button" onClick={load}>Refresh</button>
        </div>

        {error && <div className="error-alert">{error}</div>}

        <section className="check-in-card">
          {loading ? (
            <p>Loading assigned jobs...</p>
          ) : assignments.length === 0 ? (
            <p>No jobs are currently assigned to you.</p>
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
  )
}

export default MyAssignedJobsPage
