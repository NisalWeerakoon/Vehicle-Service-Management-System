import { useEffect, useState } from 'react'
import { AlertTriangle, Boxes } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import InventorySidebar from '../components/InventorySidebar'
import { clearAuth, sparePartApi } from '../services/api'

function InventoryStockReportPage({ lowStockOnly = false }) {
  const navigate = useNavigate()
  const [parts, setParts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = async () => {
    try {
      setLoading(true); setError('')
      setParts(await (lowStockOnly ? sparePartApi.getLowStockReport() : sparePartApi.getCurrentStockReport()))
    } catch (err) {
      if (err.status === 401 || err.status === 403) { clearAuth(); navigate('/login'); return }
      setError(err.message || 'Unable to load the stock report.')
    } finally { setLoading(false) }
  }

  useEffect(() => {
    load()
    window.addEventListener('inventory-stock-changed', load)
    return () => window.removeEventListener('inventory-stock-changed', load)
  }, [lowStockOnly])
  const title = lowStockOnly ? 'Low-Stock Report' : 'Current Stock Report'
  const emptyMessage = lowStockOnly ? 'No parts are currently at or below their low-stock threshold.' : 'No spare parts are available.'

  return <div className="portal-layout"><InventorySidebar /><main className="portal-main"><header className="portal-topbar"><div><span className="portal-eyebrow">INVENTORY MANAGEMENT</span><h1>{title}</h1></div><button className="portal-secondary-button" onClick={load}>Refresh</button></header><div className="portal-content">
    {error && <div className="portal-error">{error}</div>}
    <section className="portal-card"><div className="inventory-list-header"><div><h2>{lowStockOnly ? 'Parts requiring attention' : 'All available parts'}</h2><p>{parts.length} part{parts.length === 1 ? '' : 's'} shown</p></div></div>
      {loading ? <div className="portal-loading-card"><div className="loading-spinner" /><p>Loading stock report...</p></div> : parts.length === 0 ? <div className="modern-empty-state"><div className="modern-empty-icon">{lowStockOnly ? <AlertTriangle size={32} /> : <Boxes size={32} />}</div><h2>{lowStockOnly ? 'No low-stock parts' : 'No stock data'}</h2><p>{emptyMessage}</p></div> : <div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>Part ID</th><th>Part</th><th>Current stock</th><th>Threshold</th><th>Status</th><th>Unit price</th></tr></thead><tbody>{parts.map(part => <tr key={part.id}><td>#{part.id}</td><td><strong>{part.name}</strong><br /><small>{part.description}</small></td><td>{part.currentQuantity}</td><td>{part.lowStockThreshold}</td><td><strong className={part.isLowStock ? 'portal-error-text' : ''}>{part.stockStatus}</strong></td><td>{Number(part.unitPrice).toFixed(2)}</td></tr>)}</tbody></table></div>}
    </section>
  </div></main></div>
}

export default InventoryStockReportPage
