import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import InventorySidebar from '../components/InventorySidebar'
import { clearAuth, partRequestApi, sparePartApi } from '../services/api'

export default function InventoryDashboardPage() {
  const navigate = useNavigate()
  const [stock, setStock] = useState([])
  const [lowStock, setLowStock] = useState([])
  const [requests, setRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError('')
      const [all, low, pending] = await Promise.all([
        sparePartApi.getCurrentStockReport(),
        sparePartApi.getLowStockReport(),
        partRequestApi.getPending(),
      ])
      setStock(Array.isArray(all) ? all : [])
      setLowStock(Array.isArray(low) ? low : [])
      setRequests(Array.isArray(pending) ? pending : [])
    } catch (requestError) {
      if ([401, 403].includes(requestError.status)) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(requestError.message || 'Inventory data is temporarily unavailable.')
    } finally {
      setLoading(false)
    }
  }, [navigate])

  useEffect(() => {
    const timer = window.setTimeout(() => { void load() }, 0)
    return () => window.clearTimeout(timer)
  }, [load])

  const totalUnits = stock.reduce((sum, item) => sum + Number(item.currentQuantity || 0), 0)
  const stockValue = stock.reduce((sum, item) => sum + Number(item.currentQuantity || 0) * Number(item.unitPrice || 0), 0)
  const metrics = [
    { label: 'Part types', value: stock.length, detail: 'Catalogued' },
    { label: 'Units in stock', value: totalUnits, detail: 'Available quantity' },
    { label: 'Low stock', value: lowStock.length, detail: 'Require attention' },
    { label: 'Stock value', value: stockValue.toFixed(2), detail: 'Estimated total' },
  ]

  return (
    <div className="portal-layout">
      <InventorySidebar />
      <main className="portal-main">
        <header className="portal-topbar">
          <div><span className="portal-eyebrow">PARTS &amp; INVENTORY</span><h1>Inventory Overview</h1></div>
          <button className="portal-secondary-button" onClick={load}>Refresh</button>
        </header>
        <div className="portal-content inventory-dashboard">
          {error && <div className="portal-error" role="alert"><div><strong>Inventory service unavailable</strong><span>{error}</span></div><button className="text-action" onClick={load}>Try again</button></div>}

          <section className="dashboard-metrics" aria-label="Inventory overview">
            {metrics.map((metric) => (
              <article className="metric-tile" key={metric.label}>
                <span>{metric.label}</span>
                <strong>{loading ? '—' : metric.value}</strong>
                <small>{metric.detail}</small>
              </article>
            ))}
          </section>

          <section className="dashboard-workspace inventory-workspace">
            <article className="portal-card dashboard-panel">
              <div className="section-title-row">
                <div><span className="section-kicker">STOCK CONTROL</span><h2>Low-stock attention</h2><p>Parts at or below their reorder threshold.</p></div>
                <button className="portal-secondary-button small-button" onClick={() => navigate('/inventory/low-stock-report')}>View report</button>
              </div>
              {loading ? <div className="compact-loading">Loading stock levels…</div> : lowStock.length === 0 ? <div className="compact-empty"><strong>Stock levels are healthy</strong><span>No parts currently require attention.</span></div> : <div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>Part</th><th>Available</th><th>Threshold</th></tr></thead><tbody>{lowStock.slice(0, 5).map((item) => <tr key={item.id}><td><strong>{item.name}</strong></td><td>{item.currentQuantity}</td><td>{item.lowStockThreshold}</td></tr>)}</tbody></table></div>}
            </article>

            <article className="portal-card dashboard-panel">
              <div className="section-title-row">
                <div><span className="section-kicker">FULFILMENT</span><h2>Pending requests</h2><p>Mechanic requests awaiting issue.</p></div>
                <button className="portal-secondary-button small-button" onClick={() => navigate('/inventory/part-requests')}>Manage requests</button>
              </div>
              {loading ? <div className="compact-loading">Loading requests…</div> : requests.length === 0 ? <div className="compact-empty"><strong>No pending requests</strong><span>New mechanic requests will appear here.</span></div> : <div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>Job</th><th>Part</th><th>Quantity</th></tr></thead><tbody>{requests.slice(0, 5).map((item) => <tr key={item.id}><td>{item.jobCardNumber || `#${item.jobCardId}`}</td><td><strong>{item.sparePartName}</strong></td><td>{item.requestedQuantity}</td></tr>)}</tbody></table></div>}
            </article>
          </section>
        </div>
      </main>
    </div>
  )
}