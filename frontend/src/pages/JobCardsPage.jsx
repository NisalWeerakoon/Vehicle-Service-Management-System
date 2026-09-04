import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
    <main className="job-cards-page">
      <div className="check-in-shell">
        <div className="check-in-header">
          <div>
            <p className="eyebrow">JOB & MAINTENANCE</p>
            <h1>Job Cards</h1>
            <p>Create a job card for a checked-in vehicle and view existing job details.</p>
          </div>
          <button type="button" className="secondary-button" onClick={() => navigate('/service-advisor/check-in')}>
            Vehicle Check-In
          </button>
        </div>

        {message && <div className="success-alert">{message}</div>}
        {error && <div className="error-alert">{error}</div>}

        <div className="job-card-grid">
          <section className="check-in-card">
            <h2>Create Job Card</h2>
            <p className="form-hint">Enter the identifiers from the vehicle check-in.</p>
            <form onSubmit={submit} className="check-in-form">
              <label>Check-In ID<input name="checkInId" type="number" min="1" value={form.checkInId} onChange={update} required /></label>
              <label>Customer ID<input name="customerId" type="number" min="1" value={form.customerId} onChange={update} required /></label>
              <label>Vehicle ID<input name="vehicleId" type="number" min="1" value={form.vehicleId} onChange={update} required /></label>
              <label>Registration Number<input name="vehicleRegistrationNumber" maxLength="30" value={form.vehicleRegistrationNumber} onChange={update} required /></label>
              <label>Reported Problems<textarea name="reportedProblems" maxLength="500" value={form.reportedProblems} onChange={update} required /></label>
              <button type="submit" className="primary-button" disabled={saving}>
                {saving ? 'Creating...' : 'Create Job Card'}
              </button>
            </form>
          </section>

          <section className="check-in-card">
            <div className="section-title-row">
              <div>
                <h2>Job Cards</h2>
                <p className="form-hint">Kafka VehicleCheckedIn events create cards automatically.</p>
              </div>
              <button type="button" className="secondary-button" onClick={loadJobs}>Refresh</button>
            </div>

            {loading ? (
              <p>Loading job cards...</p>
            ) : jobs.length === 0 ? (
              <p>No job cards found.</p>
            ) : (
              <div className="job-card-list">
                {jobs.map((job) => (
                  <button
                    type="button"
                    className={`job-card-row ${selected?.id === job.id ? 'selected' : ''}`}
                    key={job.id}
                    onClick={() => setSelected(job)}
                  >
                    <span><strong>{job.jobCardNumber}</strong><small>{job.vehicleRegistrationNumber}</small></span>
                    <span><strong>Check-In #{job.checkInId}</strong><small>{job.status}</small></span>
                  </button>
                ))}
              </div>
            )}
          </section>
        </div>

        {selected && (
          <section className="check-in-card job-detail-card">
            <h2>Job Details</h2>
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
  )
}

export default JobCardsPage
