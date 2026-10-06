import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import ServiceAdvisorSidebar from '../components/ServiceAdvisorSidebar'
import {
  authApi,
  bookingApi,
  clearAuth,
  customerApi,
  inspectionApi,
  jobCardApi,
  mechanicApi,
} from '../services/api'

function ServiceAdvisor() {
  const navigate = useNavigate()
  const [jobCards, setJobCards] = useState([])
  const [completedInspections, setCompletedInspections] = useState([])
  const [readyBookings, setReadyBookings] = useState([])
  const [mechanics, setMechanics] = useState([])
  const [profile, setProfile] = useState(null)
  const [userInfo, setUserInfo] = useState({
    email: localStorage.getItem('email') || '',
    role: localStorage.getItem('role') || 'ServiceAdvisor',
    userId: localStorage.getItem('userId') || '',
  })
  const [loading, setLoading] = useState(true)
  const [notice, setNotice] = useState('')
  const [searchTerm, setSearchTerm] = useState('')

  const loadDashboardData = useCallback(async () => {
    setLoading(true)
    setNotice('')

    const requests = await Promise.allSettled([
      jobCardApi.getAll(),
      inspectionApi.getCompleted(),
      bookingApi.getStaffCheckInReady(),
      mechanicApi.getActiveMechanics(),
      customerApi.getMyProfile(),
      authApi.me(),
    ])

    const [jobs, inspections, bookings, activeMechanics, profileResult, account] = requests

    if (account.status === 'rejected' && account.reason?.status === 401) {
      clearAuth()
      navigate('/login')
      return
    }
    if (jobs.status === 'fulfilled') setJobCards(Array.isArray(jobs.value) ? jobs.value : [])
    if (inspections.status === 'fulfilled') setCompletedInspections(Array.isArray(inspections.value) ? inspections.value : [])
    if (bookings.status === 'fulfilled') setReadyBookings(Array.isArray(bookings.value) ? bookings.value : [])
    if (activeMechanics.status === 'fulfilled') setMechanics(Array.isArray(activeMechanics.value) ? activeMechanics.value : [])
    if (profileResult.status === 'fulfilled' && profileResult.value) setProfile(profileResult.value)
    if (account.status === 'fulfilled' && account.value) setUserInfo((current) => ({ ...current, ...account.value }))

    const operationalFailures = requests.slice(0, 4).filter((result) => result.status === 'rejected')
    if (operationalFailures.length) {
      setNotice('Some live dashboard totals are temporarily unavailable. The available information is shown below.')
    }

    setLoading(false)
  }, [navigate])

  useEffect(() => {
    const timer = window.setTimeout(() => { void loadDashboardData() }, 0)
    return () => window.clearTimeout(timer)
  }, [loadDashboardData])

  const filteredJobs = jobCards.filter((job) => {
    const term = searchTerm.toLowerCase()
    return [job.jobCardNumber, job.vehicleRegistrationNumber, job.reportedProblems]
      .some((value) => value?.toLowerCase().includes(term))
  })

  const advisorName = profile?.fullName || userInfo?.fullName || userInfo?.email?.split('@')[0] || 'Service Advisor'
  const advisorEmail = profile?.email || userInfo?.email || 'Not provided'
  const advisorPhone = profile?.phoneNumber || 'Not provided'
  const staffId = userInfo?.userId || profile?.id || 'SA-01'
  const metrics = [
    { label: 'Job cards', value: jobCards.length, detail: 'Total registered' },
    { label: 'Check-ins', value: readyBookings.length, detail: 'Vehicles ready' },
    { label: 'Inspections', value: completedInspections.length, detail: 'Completed' },
    { label: 'Mechanics', value: mechanics.length, detail: 'Currently active' },
  ]

  return (
    <div className="portal-layout">
      <ServiceAdvisorSidebar />
      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">SERVICE OPERATIONS</span>
            <h1>Service Advisor</h1>
          </div>
          <div className="page-header-actions">
            <button className="portal-secondary-button" onClick={loadDashboardData}>Refresh</button>
            <button className="portal-primary-button" onClick={() => navigate('/service-advisor/check-in')}>New vehicle check-in</button>
          </div>
        </header>

        <div className="portal-content advisor-dashboard">
          {notice && <div className="portal-notice" role="status">{notice}</div>}

          <section className="staff-summary" aria-label="Signed-in service advisor">
            <div className="staff-summary-main">
              <div className="staff-monogram" aria-hidden="true">{advisorName.charAt(0).toUpperCase()}</div>
              <div>
                <span className="summary-kicker">Signed-in staff member</span>
                <h2>{advisorName}</h2>
                <p>Service Advisor · Staff #{staffId}</p>
              </div>
            </div>
            <dl className="staff-contact-list">
              <div><dt>Email</dt><dd>{advisorEmail}</dd></div>
              <div><dt>Phone</dt><dd>{advisorPhone}</dd></div>
            </dl>
            <button className="portal-secondary-button" onClick={() => navigate('/service-advisor/profile/edit')}>Edit profile</button>
          </section>

          <section className="dashboard-metrics" aria-label="Service overview">
            {metrics.map((metric) => (
              <article className="metric-tile" key={metric.label}>
                <span>{metric.label}</span>
                <strong>{loading ? '—' : metric.value}</strong>
                <small>{metric.detail}</small>
              </article>
            ))}
          </section>

          <section className="dashboard-workspace">
            <article className="portal-card dashboard-panel">
              <div className="section-title-row">
                <div><span className="section-kicker">ARRIVALS</span><h2>Awaiting check-in</h2><p>Confirmed bookings expected at the workshop.</p></div>
                <button className="portal-secondary-button small-button" onClick={() => navigate('/service-advisor/check-in')}>Open check-in</button>
              </div>
              {loading ? (
                <div className="compact-loading">Loading arrivals…</div>
              ) : readyBookings.length === 0 ? (
                <div className="compact-empty"><strong>No vehicles are waiting</strong><span>Confirmed bookings will appear here.</span></div>
              ) : (
                <div className="compact-list">
                  {readyBookings.slice(0, 4).map((booking) => (
                    <div className="compact-list-row" key={booking.id}>
                      <div><strong>{booking.vehicleRegistrationNumber || `Booking #${booking.id}`}</strong><span>{booking.serviceType || 'Standard service'}</span></div>
                      <button className="text-action" onClick={() => navigate('/service-advisor/check-in')}>Check in</button>
                    </div>
                  ))}
                </div>
              )}
            </article>

            <article className="portal-card dashboard-panel dashboard-panel-wide">
              <div className="section-title-row dashboard-table-heading">
                <div><span className="section-kicker">WORKSHOP</span><h2>Recent job cards</h2><p>Active repair and maintenance work registered in the system.</p></div>
                <input className="dashboard-search" type="search" placeholder="Search job or registration" value={searchTerm} onChange={(event) => setSearchTerm(event.target.value)} />
              </div>
              {loading ? (
                <div className="compact-loading">Loading job cards…</div>
              ) : filteredJobs.length === 0 ? (
                <div className="compact-empty"><strong>No job cards found</strong><span>Newly created work will appear here.</span></div>
              ) : (
                <div className="portal-table-wrapper">
                  <table className="portal-table">
                    <thead><tr><th>Job card</th><th>Vehicle</th><th>Reported work</th><th>Action</th></tr></thead>
                    <tbody>{filteredJobs.map((job) => (
                      <tr key={job.id || job.jobCardNumber}>
                        <td><strong>{job.jobCardNumber || `#${job.id}`}</strong></td>
                        <td>{job.vehicleRegistrationNumber || 'Not recorded'}</td>
                        <td>{job.reportedProblems || 'No description provided'}</td>
                        <td><div className="table-actions"><button className="text-action" onClick={() => navigate(`/jobs/${job.id || job.jobCardNumber}/status`)}>View status</button><button className="text-action" onClick={() => navigate('/service-advisor/mechanic-assignments')}>Assign</button></div></td>
                      </tr>
                    ))}</tbody>
                  </table>
                </div>
              )}
            </article>
          </section>
        </div>
      </main>
    </div>
  )
}

export default ServiceAdvisor