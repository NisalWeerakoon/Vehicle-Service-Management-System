import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import CustomerSidebar from '../components/CustomerSidebar'
import { clearAuth, inspectionApi, jobCardApi } from '../services/api'

function InspectionPage() {
  const { jobCardId } = useParams()
  const navigate = useNavigate()
  const [job, setJob] = useState(null)
  const [inspection, setInspection] = useState(null)
  const [results, setResults] = useState('')
  const [problems, setProblems] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const load = async () => {
    try {
      setLoading(true); setError('')
      const jobData = await jobCardApi.getById(jobCardId)
      setJob(jobData)
      try {
        const existing = await inspectionApi.getByJob(jobCardId)
        setInspection(existing); setResults(existing.inspectionResults || ''); setProblems(existing.identifiedProblems || '')
      } catch (err) {
        if (err.status !== 404) throw err
      }
    } catch (err) {
      if (err.status === 401 || err.status === 403) { clearAuth(); navigate('/login'); return }
      setError(err.message)
    } finally { setLoading(false) }
  }

  useEffect(() => { load() }, [jobCardId])

  const save = async (event) => {
    event.preventDefault(); setError(''); setMessage('')
    if (results.trim().length < 3 || problems.trim().length < 3) {
      setError('Inspection results and identified problems are required (minimum 3 characters each).'); return
    }
    try {
      setSaving(true)
      const data = await inspectionApi.save({ jobCardId: Number(jobCardId), inspectionResults: results, identifiedProblems: problems })
      setInspection(data); setMessage('Inspection saved successfully.')
    } catch (err) { setError(err.message) } finally { setSaving(false) }
  }

  const complete = async () => {
    if (!inspection) return
    if (!window.confirm('Complete this inspection? This will mark the job as Inspected.')) return
    try {
      setSaving(true); setError(''); setMessage('')
      const data = await inspectionApi.complete(inspection.id)
      setInspection(data); setMessage('Inspection completed and the InspectionCompleted event was published.')
    } catch (err) { setError(err.message) } finally { setSaving(false) }
  }

  if (loading) return <div className="portal-layout"><CustomerSidebar /><main className="portal-main"><div className="portal-content"><div className="portal-loading-card"><div className="loading-spinner" /><p>Loading inspection...</p></div></div></main></div>

  return <div className="portal-layout"><CustomerSidebar /><main className="portal-main"><header className="portal-topbar"><div><span className="portal-eyebrow">MECHANIC INTERFACE</span><h1>Vehicle Inspection</h1></div><button className="portal-secondary-button" type="button" onClick={() => navigate('/mechanic/my-jobs')}>← My Jobs</button></header><div className="portal-content">
    {error && <div className="portal-error"><span>!</span>{error}</div>}
    {message && <div className="portal-success">✓ {message}</div>}
    {job && <section className="checkin-card"><span className="profile-welcome-label">JOB CARD</span><h2>{job.jobCardNumber}</h2><p><strong>Vehicle:</strong> {job.vehicleRegistrationNumber}</p><p><strong>Reported problem:</strong> {job.reportedProblems}</p></section>}
    <form className="checkin-card" style={{ marginTop: '24px' }} onSubmit={save}>
      <h2>Inspection Findings</h2>
      <p>Record the inspection results and problems found on the assigned vehicle.</p>
      <div className="form-group"><label htmlFor="inspectionResults">Inspection Results *</label><textarea id="inspectionResults" rows="6" value={results} onChange={e => setResults(e.target.value)} disabled={inspection?.isCompleted || saving} placeholder="Describe checks performed and their results..." /></div>
      <div className="form-group"><label htmlFor="identifiedProblems">Identified Problems *</label><textarea id="identifiedProblems" rows="6" value={problems} onChange={e => setProblems(e.target.value)} disabled={inspection?.isCompleted || saving} placeholder="List problems found during inspection..." /></div>
      <div className="button-row"><button className="portal-secondary-button" type="button" onClick={() => navigate('/mechanic/my-jobs')}>Cancel</button>{!inspection?.isCompleted && <><button className="portal-primary-button" type="submit" disabled={saving}>{saving ? 'Saving...' : 'Save Inspection'}</button>{inspection?.id && <button className="portal-primary-button" type="button" onClick={complete} disabled={saving}>Complete Inspection</button>}</>}</div>
      {inspection?.isCompleted && <p style={{ marginTop: '18px' }}><strong>Completed:</strong> {new Date(inspection.completedAt).toLocaleString()}</p>}
    </form>
  </div></main></div>
}
export default InspectionPage
