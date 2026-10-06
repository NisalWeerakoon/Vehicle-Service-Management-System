import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import CustomerSidebar from '../components/CustomerSidebar'
import PageHeader from '../components/PageHeader'
import { authApi, clearAuth, customerApi, invoiceApi, saveAuth } from '../services/api'

const money = (value) => `LKR ${Number(value || 0).toLocaleString('en-LK', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

export default function MyInvoicesPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState([])
  const [selected, setSelected] = useState(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const loadInvoices = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      let invoices
      try {
        invoices = await invoiceApi.getMine()
      } catch (requestError) {
        if (requestError.status !== 403) throw requestError
        await customerApi.getMyProfile()
        const refreshedAuth = await authApi.refresh()
        saveAuth(refreshedAuth)
        invoices = await invoiceApi.getMine()
      }
      setItems(Array.isArray(invoices) ? invoices : [])
    } catch (requestError) {
      if (requestError.status === 401) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(requestError.message || 'Your invoices are temporarily unavailable.')
    } finally {
      setLoading(false)
    }
  }, [navigate])

  useEffect(() => {
    const timer = window.setTimeout(() => { void loadInvoices() }, 0)
    return () => window.clearTimeout(timer)
  }, [loadInvoices])

  const openInvoice = async (id) => {
    try {
      setError('')
      setSelected(await invoiceApi.getById(id))
    } catch (requestError) {
      setError(requestError.message || 'This invoice could not be opened.')
    }
  }

  return (
    <div className="portal-layout customer-portal">
      <CustomerSidebar />
      <main className="portal-main">
        <PageHeader eyebrow="CUSTOMER PORTAL" title="Invoices" description="Review service charges, balances and payment status." />
        <div className="portal-content customer-content customer-invoice-page">
          {error && <div className="portal-error customer-error" role="alert"><div><strong>Invoices unavailable</strong><span>{error}</span></div><button className="text-action" onClick={loadInvoices}>Try again</button></div>}

          <section className="customer-section-heading customer-page-intro"><div><span className="customer-section-label">BILLING HISTORY</span><h2>Your service invoices</h2><p>Generated invoices appear here after workshop service is completed.</p></div></section>

          {loading ? <div className="portal-loading-card"><div className="loading-spinner" /><p>Loading invoices…</p></div> : items.length === 0 ? (
            <section className="customer-empty-state"><span className="customer-section-label">NO INVOICES</span><h2>Nothing to pay right now</h2><p>Your completed-service invoices will be available here when generated.</p><button className="portal-secondary-button" onClick={() => navigate('/bookings')}>View service bookings</button></section>
          ) : (
            <section className="customer-invoice-list">
              {items.map((item) => <article className="customer-invoice-row" key={item.id}><div><span>{item.invoiceNumber}</span><strong>{item.vehicleRegistrationNumber}</strong><small>{item.paymentStatus}</small></div><div className="invoice-balance"><span>Balance</span><strong>{money(item.remainingBalance)}</strong></div><button className="portal-secondary-button small-button" onClick={() => openInvoice(item.id)}>View invoice</button></article>)}
            </section>
          )}

          {selected && <section className="customer-invoice-detail"><div className="customer-invoice-detail-header"><div><span className="customer-section-label">INVOICE DETAIL</span><h2>{selected.invoiceNumber}</h2><p>{selected.vehicleRegistrationNumber} · Job {selected.jobCardNumber || `#${selected.jobCardId}`}</p></div><div className="invoice-total"><span>Total</span><strong>{money(selected.totalAmount)}</strong><small>{selected.paymentStatus}</small></div></div><div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>Type</th><th>Description</th><th>Quantity</th><th>Unit price</th><th>Total</th></tr></thead><tbody>{selected.chargeLines.map((line) => <tr key={line.id}><td>{line.chargeType}</td><td>{line.description}</td><td>{line.quantity}</td><td>{money(line.unitPrice)}</td><td><strong>{money(line.lineTotal)}</strong></td></tr>)}</tbody></table></div><div className="invoice-summary"><span>Paid {money(selected.amountPaid)}</span><strong>Remaining {money(selected.remainingBalance)}</strong></div></section>}
        </div>
      </main>
    </div>
  )
}