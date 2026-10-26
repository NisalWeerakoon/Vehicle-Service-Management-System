import { useEffect, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { CalendarDays, ChevronLeft, ClipboardList, History, Wrench } from 'lucide-react'
import CustomerSidebar from '../components/CustomerSidebar'
import ServiceAdvisorSidebar from '../components/ServiceAdvisorSidebar'
import { clearAuth, getRole, serviceHistoryApi } from '../services/api'

function formatDate(value) {
  if (!value) return 'Not recorded'
  return new Intl.DateTimeFormat('en-LK', {
    dateStyle: 'medium', timeStyle: 'short',
  }).format(new Date(value))
}

function HistoryRecord({ record, index }) {
  const repairs = record.completedRepairTasks || []
  const notes = record.repairNotes || []
  return (
    <details className="service-history-record" open={index === 0}>
      <summary>
        <div className="history-date-mark"><CalendarDays size={18} /></div>
        <div className="history-summary-copy">
          <span>{formatDate(record.completedAt)}</span>
          <strong>{record.jobCardNumber}</strong>
          <small>{record.reportedProblems}</small>
        </div>
        <span className="history-status">{record.finalStatus}</span>
      </summary>

      <div className="history-detail-grid">
        <section className="history-detail-card history-problems">
          <span>Reported concerns</span>
          <p>{record.reportedProblems}</p>
        </section>
        <section className="history-detail-card">
          <span>Completion</span>
          <strong>{formatDate(record.completedAt)}</strong>
          <small>Recorded by {record.completedBy || 'service staff'}</small>
        </section>
      </div>

      <div className="history-work-grid">
        <section className="history-work-panel">
          <h3><Wrench size={17} /> Completed repairs</h3>
          {repairs.length === 0 ? <p className="history-muted">No completed repair tasks were recorded.</p> : (
            <ul>{repairs.map((repair) => (
              <li key={repair.id}>
                <strong>{repair.title}</strong>
                <span>{repair.description}</span>
                {repair.completedAt && <small>{formatDate(repair.completedAt)}</small>}
              </li>
            ))}</ul>
          )}
        </section>

        <section className="history-work-panel">
          <h3><ClipboardList size={17} /> Inspection findings</h3>
          {record.inspection ? (
            <div className="history-inspection">
              <div><span>Results</span><p>{record.inspection.inspectionResults || 'Not recorded'}</p></div>
              <div><span>Identified problems</span><p>{record.inspection.identifiedProblems || 'None recorded'}</p></div>
            </div>
          ) : <p className="history-muted">No inspection snapshot was available.</p>}
        </section>
      </div>

      <section className="history-notes">
        <h3>Repair notes</h3>
        {notes.length === 0 ? <p className="history-muted">No repair notes were recorded.</p> : (
          <ul>{notes.map((note) => <li key={note.id}><p>{note.note}</p><small>{formatDate(note.createdAt)}</small></li>)}</ul>
        )}
      </section>
    </details>
  )
}

function ServiceHistoryPage() {
  const { vehicleId } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const role = getRole()
  const isCustomer = role === 'Customer'
  const Sidebar = isCustomer ? CustomerSidebar : ServiceAdvisorSidebar
  const [history, setHistory] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    async function loadHistory() {
      try {
        const data = await serviceHistoryApi.getByVehicle(vehicleId)
        if (active) setHistory(data)
      } catch (err) {
        if (err.status === 401) {
          clearAuth()
          navigate('/login')
          return
        }
        if (active) setError(err.message || 'Service history could not be loaded.')
      } finally {
        if (active) setLoading(false)
      }
    }
    loadHistory()
    return () => { active = false }
  }, [navigate, vehicleId])

  const vehicle = location.state?.vehicle
  const registration = history?.vehicleRegistrationNumber ||
    vehicle?.registrationNumber || vehicle?.vehicleRegistrationNumber || `Vehicle #${vehicleId}`
  const backPath = isCustomer ? '/vehicles' : '/service-advisor/job-cards'

  return (
    <div className={`portal-layout service-history-page ${isCustomer ? 'customer-portal' : ''}`}>
      <Sidebar />
      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">{isCustomer ? 'CUSTOMER PORTAL' : 'SERVICE ADVISOR'}</span>
            <h1>Vehicle Service History</h1>
            <p className="portal-page-description">Completed service records captured at the time work was finished.</p>
          </div>
          <button className="portal-secondary-button" onClick={() => navigate(backPath)}>
            <ChevronLeft size={16} /> Back
          </button>
        </header>

        <div className="portal-content service-history-content">
          <section className="service-history-hero">
            <div>
              <span>VEHICLE RECORD</span>
              <h2>{registration}</h2>
              <p>{vehicle?.make && vehicle?.model ? `${vehicle.make} ${vehicle.model}` : 'Permanent completed-service snapshots'}</p>
            </div>
            <div className="history-count">
              <strong>{history?.records?.length || 0}</strong>
              <span>completed services</span>
            </div>
          </section>

          {loading ? (
            <section className="portal-loading-card history-state-card">
              <div className="loading-spinner" />
              <p>Loading service history...</p>
            </section>
          ) : error ? (
            <section className="history-state-card history-error-state">
              <strong>We could not load this vehicle's history</strong>
              <p>{error}</p>
              <button className="portal-secondary-button" onClick={() => window.location.reload()}>Try Again</button>
            </section>
          ) : history?.records?.length === 0 ? (
            <section className="history-state-card">
              <History size={34} />
              <strong>No completed service history yet</strong>
              <p>Completed services for this vehicle will appear here automatically.</p>
            </section>
          ) : (
            <section className="service-history-timeline" aria-label="Completed vehicle services">
              {history.records.map((record, index) => (
                <HistoryRecord key={record.id} record={record} index={index} />
              ))}
            </section>
          )}
        </div>
      </main>
    </div>
  )
}

export default ServiceHistoryPage
