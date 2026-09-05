import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import CustomerSidebar from '../components/CustomerSidebar'
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
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">SERVICE ADVISOR</span>
            <h1>Mechanic Assignments</h1>
          </div>

          <button
            className="portal-primary-button"
            onClick={() => navigate('/service-advisor/job-cards')}
          >
            Job Cards
          </button>
        </header>

        <div className="portal-content">
          <section className="profile-welcome">
            <div>
              <span className="profile-welcome-label">STAFF MANAGEMENT</span>
              <h2>Assign Mechanics to Jobs</h2>
              <p>Select active mechanics from User Management and assign them to job cards.</p>
            </div>

            <button
              className="portal-primary-button"
              type="button"
              onClick={load}
            >
              ↻ Refresh List
            </button>
          </section>

          {message && (
            <div className="checkin-alert success">
              <strong>{message}</strong>
            </div>
          )}
          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          <section className="checkin-card" style={{ marginTop: '24px' }}>
            <div className="section-title-row">
              <div>
                <h2>Active Job Cards</h2>
                <p className="form-hint">Only active mechanics are listed for assignment.</p>
              </div>
            </div>

            {loading ? (
              <div className="portal-loading-card" style={{ marginTop: '20px' }}>
                <div className="loading-spinner" />
                <p>Loading jobs and mechanics...</p>
              </div>
            ) : jobs.length === 0 ? (
              <p style={{ marginTop: '20px', color: '#6b7280' }}>No job cards found.</p>
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
                            <option key={mechanic.id || mechanic.userId} value={mechanic.id || mechanic.userId}>
                              {mechanic.email}
                            </option>
                          ))}
                        </select>
                        <button
                          type="button"
                          className="portal-primary-button"
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
    </div>
  )
}

export default MechanicAssignmentsPage
