import { useCallback, useEffect, useMemo, useState } from 'react'
import { FileText } from 'lucide-react'
import AccountsSidebar from '../components/AccountsSidebar'
import PageHeader from '../components/PageHeader'
import { invoiceApi } from '../services/api'

const money = (value) => Number(value || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const displayStatus = (status) => status === 'PartiallyPaid' ? 'Partially Paid' : status
const statusClass = (status) => `booking-status status-${status.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase()}`

export default function InvoicePaymentReportPage() {
  const [report, setReport] = useState(null)
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError('')
      setReport(await invoiceApi.getReport(status))
    } catch (err) {
      setError(err.message || 'Unable to load the invoice and payment report.')
    } finally {
      setLoading(false)
    }
  }, [status])

  useEffect(() => {
    const request = window.setTimeout(load, 0)
    return () => window.clearTimeout(request)
  }, [load])

  const items = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return report?.items || []
    return (report?.items || []).filter((item) =>
      item.invoiceNumber?.toLowerCase().includes(term) ||
      item.vehicleRegistration?.toLowerCase().includes(term))
  }, [report, search])

  const metrics = [
    ['Total Invoiced', money(report?.totalInvoiced), 'Value of generated invoices'],
    ['Total Paid', money(report?.totalPaid), `${report?.paidCount || 0} paid invoice(s)`],
    ['Outstanding Balance', money(report?.totalOutstanding), `${report?.partiallyPaidCount || 0} partial · ${report?.unpaidCount || 0} unpaid`],
    ['Total Invoices', report?.totalInvoices || 0, status ? `${displayStatus(status)} invoices` : 'All generated invoices'],
  ]

  return <div className="portal-layout"><AccountsSidebar /><main className="portal-main">
    <PageHeader eyebrow="ACCOUNTS" title="INVOICE & PAYMENT REPORT" description="Review invoicing, collections and outstanding balances." actions={<button className="portal-secondary-button" onClick={load}>Refresh</button>} />
    <div className="portal-content invoice-report-content">
      {error && <div className="portal-error">{error}</div>}
      <section className="dashboard-metrics" aria-label="Invoice report summary">
        {metrics.map(([label, value, detail]) => <article className="metric-tile" key={label}><span>{label}</span><strong>{loading ? '—' : value}</strong><small>{detail}</small></article>)}
      </section>
      <section className="portal-card">
        <div className="invoice-report-toolbar">
          <div><h2>Generated invoices</h2><p>{items.length} invoice{items.length === 1 ? '' : 's'} shown</p></div>
          <div className="invoice-report-filters">
            <label>Payment status<select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">All</option><option value="Unpaid">Unpaid</option><option value="PartiallyPaid">Partially Paid</option><option value="Paid">Paid</option></select></label>
            <label>Search<input type="search" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Invoice or vehicle" /></label>
          </div>
        </div>
        {loading ? <div className="portal-loading-card"><div className="loading-spinner" /><p>Loading invoice report...</p></div> : items.length === 0 ? <div className="modern-empty-state"><div className="modern-empty-icon"><FileText size={32} /></div><h2>No invoices found</h2><p>{search ? 'No invoices match your search.' : 'No generated invoices match the selected payment status.'}</p></div> : <div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>Invoice</th><th>Vehicle</th><th>Invoice Total</th><th>Amount Paid</th><th>Outstanding</th><th>Status</th><th>Payments</th><th>Last Payment</th></tr></thead><tbody>{items.map((item) => <tr key={item.invoiceId}><td><strong>{item.invoiceNumber}</strong><br /><small>Job {item.jobCardNumber || `#${item.jobCardId}`} · Customer #{item.customerId}</small></td><td>{item.vehicleRegistration || 'Not provided'}</td><td>{money(item.totalAmount)}</td><td>{money(item.amountPaid)}</td><td><strong>{money(item.outstandingAmount)}</strong></td><td><span className={statusClass(item.paymentStatus)}>{displayStatus(item.paymentStatus)}</span></td><td>{item.paymentCount}</td><td>{item.lastPaymentDate ? new Date(item.lastPaymentDate).toLocaleDateString() : '—'}</td></tr>)}</tbody></table></div>}
      </section>
    </div>
  </main></div>
}
