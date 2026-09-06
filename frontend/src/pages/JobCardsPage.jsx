import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import CustomerSidebar from '../components/CustomerSidebar'
import { clearAuth, jobCardApi } from '../services/api'

function JobCardsPage() {
  const navigate = useNavigate()
  const [jobs, setJobs] = useState([])
  const [form, setForm] = useState({
    checkInId: '',
    customerId: '',
    vehicleId: '',
    vehicleRegistrationNumber: '',
    reportedProblems: '',
  })
  const [selected, setSelected] = useState(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  const loadJobs = async () => {
    try {
      setLoading(true)
      const data = await jobCardApi.getAll()
      setJobs(data)
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
    loadJobs()
  }, [])

  const update = (event) => {
    setForm({ ...form, [event.target.name]: event.target.value })
  }

  const submit = async (event) => {
    event.preventDefault()
    setError('')
    setMessage('')
    setSaving(true)

    try {
      const created = await jobCardApi.create({
        checkInId: Number(form.checkInId),
        customerId: Number(form.customerId),
        vehicleId: Number(form.vehicleId),
        vehicleRegistrationNumber: form.vehicleRegistrationNumber,
        reportedProblems: form.reportedProblems,
      })

      setMessage(`Job card ${created.jobCardNumber} is ready.`)
      setSelected(created)
      setForm({
        checkInId: '',
        customerId: '',
        vehicleId: '',
        vehicleRegistrationNumber: '',
        reportedProblems: '',
      })
      await loadJobs()
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

  return (
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">SERVICE ADVISOR</span>
            <h1>Job Cards</h1>
          </div>

          <button
            className="portal-primary-button"
            onClick={() => navigate('/service-advisor/check-in')}
          >
            Vehicle Check-In
          </button>
        </header>

        <div className="portal-content">
          <section className="profile-welcome">
            <div>
              <span className="profile-welcome-label">JOB & MAINTENANCE</span>
              <h2>Job Cards Management</h2>
              <p>Create a job card for a checked-in vehicle and view active maintenance jobs.</p>
            </div>

            <button
              className="portal-primary-button"
              type="button"
              onClick={loadJobs}
            >
              ↻ Refresh Jobs
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

          <div className="job-card-grid" style={{ marginTop: '24px' }}>
            <section className="checkin-card">
              <h2>Create Job Card</h2>
              <p className="form-hint">Enter the identifiers from the vehicle check-in.</p>

              <form onSubmit={submit} style={{ marginTop: '18px', display: 'grid', gap: '16px' }}>
                <label>
                  Check-In ID
                  <input
                    name="checkInId"
                    type="number"
                    min="1"
                    value={form.checkInId}
                    onChange={update}
                    placeholder="e.g. 1"
                    required
                  />
                </label>

                <label>
                  Customer ID
                  <input
                    name="customerId"
                    type="number"
                    min="1"
                    value={form.customerId}
                    onChange={update}
                    placeholder="e.g. 5"
                    required
                  />
                </label>

                <label>
                  Vehicle ID
                  <input
                    name="vehicleId"
                    type="number"
                    min="1"
                    value={form.vehicleId}
                    onChange={update}
                    placeholder="e.g. 12"
                    required
                  />
                </label>

                <label>
                  Registration Number
                  <input
                    name="vehicleRegistrationNumber"
                    maxLength="30"
                    value={form.vehicleRegistrationNumber}
                    onChange={update}
                    placeholder="e.g. CAB-1234"
                    required
                  />
                </label>

                <label>
                  Reported Problems
                  <textarea
                    name="reportedProblems"
                    rows="4"
                    maxLength="500"
                    value={form.reportedProblems}
                    onChange={update}
                    placeholder="Describe issues reported..."
                    required
                  />
                </label>

                <button type="submit" className="checkin-submit-button" disabled={saving}>
                  {saving ? 'Creating...' : 'Create Job Card'}
                </button>
              </form>
            </section>

            <section className="checkin-card">
              <div className="section-title-row">
                <div>
                  <h2>All Job Cards</h2>
                  <p className="form-hint">Kafka VehicleCheckedIn events create cards automatically.</p>
                </div>
              </div>

              {loading ? (
                <div className="portal-loading-card" style={{ marginTop: '20px' }}>
                  <div className="loading-spinner" />
                  <p>Loading job cards...</p>
                </div>
              ) : jobs.length === 0 ? (
                <p style={{ marginTop: '20px', color: '#6b7280' }}>No job cards found.</p>
              ) : (
                <div className="job-card-list">
                  {jobs.map((job) => (
                    <button
                      type="button"
                      className={`job-card-row ${selected?.id === job.id ? 'selected' : ''}`}
                      key={job.id}
                      onClick={() => setSelected(job)}
                    >
                      <span>
                        <strong>{job.jobCardNumber}</strong>
                        <small>{job.vehicleRegistrationNumber}</small>
                      </span>
                      <span>
                        <strong>Check-In #{job.checkInId}</strong>
                        <small>{job.status}</small>
                      </span>
                    </button>
                  ))}
                </div>
              )}
            </section>
          </div>

          {selected && (
            <section className="checkin-card job-detail-card" style={{ marginTop: '24px' }}>
              <h2>Job Details — {selected.jobCardNumber}</h2>
              <div className="job-detail-grid">
                <div><span>Job Card</span><strong>{selected.jobCardNumber}</strong></div>
                <div><span>Check-In ID</span><strong>{selected.checkInId}</strong></div>
                <div><span>Customer ID</span><strong>{selected.customerId}</strong></div>
                <div><span>Vehicle ID</span><strong>{selected.vehicleId}</strong></div>
                <div><span>Registration</span><strong>{selected.vehicleRegistrationNumber}</strong></div>
                <div><span>Status</span><strong>{selected.status}</strong></div>
                <div className="full-width"><span>Reported Problems</span><strong>{selected.reportedProblems}</strong></div>
              </div>
            </section>
          )}
        </div>
      </main>
    </div>
  )
}

export default JobCardsPage
