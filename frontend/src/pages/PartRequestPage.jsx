import { useEffect, useState } from 'react'
import MechanicSidebar from '../components/MechanicSidebar'
import { jobCardApi, jobPartRequestApi, mechanicAssignmentApi, sparePartApi } from '../services/api'

function PartRequestPage() {
  const [jobs, setJobs] = useState([])
  const [parts, setParts] = useState([])
  const [requests, setRequests] = useState([])
  const [form, setForm] = useState({ jobCardId: '', sparePartId: '', requestedQuantity: '' })
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)

  async function load() {
    try {
      setError('')
      const [assignments, catalog, mine] = await Promise.all([mechanicAssignmentApi.getMyJobs(), sparePartApi.getAll(), jobPartRequestApi.getMine()])
      const jobDetails = await Promise.all(assignments.map(x => jobCardApi.getById(x.jobCardId)))
      setJobs(jobDetails); setParts(catalog); setRequests(mine)
    } catch (err) { setError(err.message || 'Unable to load part-request information.') }
  }
  useEffect(() => { load() }, [])

  async function submit(event) {
    event.preventDefault(); setError(''); setSuccess('')
    const quantity = Number(form.requestedQuantity)
    if (!form.jobCardId || !form.sparePartId || !Number.isInteger(quantity) || quantity <= 0) { setError('Select a job and part, and enter a whole quantity greater than zero.'); return }
    try {
      setSaving(true)
      const request = await jobPartRequestApi.create({ jobCardId: Number(form.jobCardId), sparePartId: Number(form.sparePartId), requestedQuantity: quantity })
      const part = parts.find(x => x.id === request.sparePartId)
      setSuccess(`Request #${request.id} is ${request.status}: ${request.requestedQuantity} × ${part?.name || `part #${request.sparePartId}`} for ${request.jobCardNumber}.`)
      setForm({ jobCardId: '', sparePartId: '', requestedQuantity: '' }); await load()
    } catch (err) { setError(err.message || 'Unable to create the part request.') } finally { setSaving(false) }
  }

  const partName = id => parts.find(x => x.id === id)?.name || `Part #${id}`
  return <div className="portal-layout"><MechanicSidebar /><main className="portal-main"><header className="portal-topbar"><div><span className="portal-eyebrow">MECHANIC INTERFACE</span><h1>Request Spare Parts</h1></div></header><div className="portal-content">
    {error && <div className="portal-error">{error}</div>}{success && <div className="checkin-alert success">{success}</div>}
    <section className="portal-card inventory-form-card"><h2>New Part Request</h2><form className="inventory-form" onSubmit={submit}>
      <label>Job card<select value={form.jobCardId} onChange={e => setForm({ ...form, jobCardId: e.target.value })}><option value="">Select an assigned job</option>{jobs.map(job => <option key={job.id} value={job.id}>{job.jobCardNumber} — {job.vehicleRegistrationNumber}</option>)}</select></label>
      <label>Spare part<select value={form.sparePartId} onChange={e => setForm({ ...form, sparePartId: e.target.value })}><option value="">Select a part</option>{parts.map(part => <option key={part.id} value={part.id}>{part.name} (stock: {part.quantity})</option>)}</select></label>
      <label>Requested quantity<input required type="number" min="1" step="1" value={form.requestedQuantity} onChange={e => setForm({ ...form, requestedQuantity: e.target.value })} /></label>
      <div className="inventory-form-actions"><button className="portal-primary-button" disabled={saving}>{saving ? 'Submitting...' : 'Submit Request'}</button></div>
    </form></section>
    <section className="portal-card"><h2>My Requests</h2><div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>ID</th><th>Job</th><th>Part</th><th>Quantity</th><th>Requested</th><th>Status</th></tr></thead><tbody>{requests.length ? requests.map(r => <tr key={r.id}><td>#{r.id}</td><td>{r.jobCardNumber || `Job #${r.jobCardId}`}</td><td>{partName(r.sparePartId)}</td><td>{r.requestedQuantity}</td><td>{new Date(r.requestedAt).toLocaleString()}</td><td>{r.status}</td></tr>) : <tr><td colSpan="6">No part requests yet.</td></tr>}</tbody></table></div></section>
  </div></main></div>
}
export default PartRequestPage
