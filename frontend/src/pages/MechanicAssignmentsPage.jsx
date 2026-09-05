import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  clearAuth,
  jobCardApi,
  mechanicApi,
  mechanicAssignmentApi,
} from '../services/api'

function MechanicAssignmentsPage() {
  const navigate = useNavigate()
  const [jobs, setJobs] = useState([])
  const [mechanics, setMechanics] = useState([])
  const [selectedMechanics, setSelectedMechanics] = useState({})
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const handleAuthError = (err) => {
    if (err.status === 401 || err.status === 403) {
      clearAuth()
      navigate('/login')
      return true
    }
    return false
  }

  const load = async () => {
    try {
      setLoading(true)
      setError('')
      const [jobData, mechanicData] = await Promise.all([
        jobCardApi.getAll(),
        mechanicApi.getActiveMechanics(),
      ])
      setJobs(jobData)
      setMechanics(mechanicData)
    } catch (err) {
      if (!handleAuthError(err)) setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  const assign = async (jobId) => {
    const mechanicId = selectedMechanics[jobId]
    if (!mechanicId) {
      setError('Please select a mechanic.')
      return
    }

    try {
      setError('')
      setMessage('')
      await mechanicAssignmentApi.assign({
        jobCardId: jobId,
        mechanicId,
      })
      setMessage('Mechanic assigned successfully.')
      await load()
    } catch (err) {
      if (!handleAuthError(err)) setError(err.message)
    }
  }

  return (
    <main className="job-cards-page">
      <div className="check-in-shell">
        <div className="check-in-header">
          <div>
            <p className="eyebrow">JOB & MAINTENANCE</p>
            <h1>Mechanic Assignment</h1>
            <p>Assign an active mechanic to a job card.</p>
          </div>
          <button
            type="button"
            className="secondary-button"
            onClick={() => navigate('/service-advisor/job-cards')}
          >
            Job Cards
          </button>
        </div>

        {message && <div className="success-alert">{message}</div>}
        {error && <div className="error-alert">{error}</div>}

        <section className="check-in-card">
          <div className="section-title-row">
            <div>
              <h2>Active Job Cards</h2>
              <p className="form-hint">Only active mechanics from User Management are listed.</p>
            </div>
            <button type="button" className="secondary-button" onClick={load}>Refresh</button>
          </div>

          {loading ? (
            <p>Loading...</p>
          ) : jobs.length === 0 ? (
            <p>No job cards found.</p>
          ) : (
            <div className="job-card-list">
              {jobs.map((job) => (
                <div className="job-card-row" key={job.id}>
                  <span>
                    <strong>{job.jobCardNumber}</strong>
                    <small>{job.vehicleRegistrationNumber}</small>
                  </span>
                  <span>
                    <strong>
                      {job.assignedMechanicName
                        ? `Assigned: ${job.assignedMechanicName}`
                        : 'Not Assigned'}
                    </strong>
                    <small>Check-In #{job.checkInId}</small>
                  </span>
                  {!job.assignedMechanicName && (
                    <span className="assignment-actions">
                      <select
                        value={selectedMechanics[job.id] || ''}
                        onChange={(event) =>
                          setSelectedMechanics({
                            ...selectedMechanics,
                            [job.id]: event.target.value,
                          })
                        }
                      >
                        <option value="">Select mechanic</option>
                        {mechanics.map((mechanic) => (
                          <option key={mechanic.userId} value={mechanic.userId}>
                            {mechanic.email}
                          </option>
                        ))}
                      </select>
                      <button
                        type="button"
                        className="primary-button"
                        onClick={() => assign(job.id)}
                      >
                        Assign
                      </button>
                    </span>
                  )}
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  )
}

export default MechanicAssignmentsPage
