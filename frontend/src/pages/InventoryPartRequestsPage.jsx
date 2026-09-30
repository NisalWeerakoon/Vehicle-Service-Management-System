import { useEffect, useState } from 'react'
import InventorySidebar from '../components/InventorySidebar'
import { partRequestApi } from '../services/api'

function InventoryPartRequestsPage() {
  const [requests, setRequests] = useState([]); const [error, setError] = useState(''); const [success, setSuccess] = useState(''); const [issuing, setIssuing] = useState(null)
  async function load() { try { setError(''); setRequests(await partRequestApi.getPending()) } catch (err) { setError(err.message || 'Unable to load pending requests.') } }
  useEffect(() => { load() }, [])
  async function issue(request) {
    if (!window.confirm(`Issue ${request.requestedQuantity} × ${request.sparePartName} to ${request.jobCardNumber || `job #${request.jobCardId}`}? Current stock: ${request.currentStock}`)) return
    try { setIssuing(request.id); setError(''); setSuccess(''); const result = await partRequestApi.issue(request.id); setSuccess(`Request #${result.id} issued successfully. ${result.issue.quantityIssued} item(s) issued for ${result.jobCardNumber || `job #${result.jobCardId}`}.`); await load(); window.dispatchEvent(new Event('inventory-stock-changed')) }
    catch (err) { setError(err.message || 'Unable to issue the part request.') } finally { setIssuing(null) }
  }
  return <div className="portal-layout"><InventorySidebar /><main className="portal-main"><header className="portal-topbar"><div><span className="portal-eyebrow">INVENTORY MANAGEMENT</span><h1>Pending Part Requests</h1></div><button className="portal-secondary-button" onClick={load}>Refresh</button></header><div className="portal-content">
    {error && <div className="portal-error">{error}</div>}{success && <div className="checkin-alert success">{success}</div>}
    <section className="portal-card"><p>Issue only after checking the job, part, requested quantity, and available stock. The server prevents duplicate issues.</p><div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>Request</th><th>Job</th><th>Part</th><th>Requested</th><th>Current stock</th><th>Mechanic</th><th>Request date</th><th>Status</th><th /></tr></thead><tbody>{requests.length ? requests.map(r => <tr key={r.id}><td>#{r.id}</td><td>{r.jobCardNumber || `Job #${r.jobCardId}`}</td><td>{r.sparePartName}</td><td>{r.requestedQuantity}</td><td>{r.currentStock}</td><td>{r.requestingMechanicName}</td><td>{new Date(r.requestedAt).toLocaleString()}</td><td>{r.status}</td><td><button className="portal-primary-button" disabled={issuing === r.id} onClick={() => issue(r)}>{issuing === r.id ? 'Issuing...' : 'Issue'}</button></td></tr>) : <tr><td colSpan="9">No pending part requests.</td></tr>}</tbody></table></div></section>
  </div></main></div>
}
export default InventoryPartRequestsPage
