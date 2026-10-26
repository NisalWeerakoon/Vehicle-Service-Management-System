import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { motion, useReducedMotion } from 'motion/react'
import { ArrowUpRight, CalendarCheck, Car, Plus } from 'lucide-react'
import CustomerSidebar from '../components/CustomerSidebar'
import PageHeader from '../components/PageHeader'
import { bookingApi, clearAuth, customerApi, vehicleApi } from '../services/api'

function statusClass(status) {
  const value = status ? status.toLowerCase().replace(/\s+/g, '-') : 'pending'
  return `booking-status status-${value}`
}

function ProfilePage() {
  const navigate = useNavigate()
  const reduceMotion = useReducedMotion()
  const [profile, setProfile] = useState(null)
  const [vehicles, setVehicles] = useState([])
  const [bookings, setBookings] = useState([])
  const [hasNoProfile, setHasNoProfile] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    async function fetchAccount() {
      try {
        const [profileData, vehicleData, bookingData] = await Promise.all([
          customerApi.getMyProfile(),
          vehicleApi.getMyVehicles().catch(() => []),
          bookingApi.getMyBookings().catch(() => []),
        ])
        if (!active) return
        setProfile(profileData)
        setVehicles(Array.isArray(vehicleData) ? vehicleData : [])
        setBookings(Array.isArray(bookingData) ? bookingData : [])
      } catch (requestError) {
        if (requestError.status === 401) {
          clearAuth()
          navigate('/login')
          return
        }
        if (active && (requestError.status === 404 || requestError.message?.includes('not been created'))) setHasNoProfile(true)
        else if (active) setError(requestError.message || 'Your profile could not be loaded.')
      } finally {
        if (active) setLoading(false)
      }
    }
    void fetchAccount()
    return () => { active = false }
  }, [navigate])

  const activeBooking = useMemo(() => bookings
    .filter((booking) => !['Completed', 'Cancelled'].includes(booking.status))
    .sort((a, b) => new Date(a.preferredDate) - new Date(b.preferredDate))[0], [bookings])

  const name = profile?.fullName || 'Customer'
  const initial = name.charAt(0).toUpperCase()
  const actions = [
    { label: 'Book a service', description: 'Choose a vehicle and arrange your next workshop visit.', path: '/bookings/create', number: '01', icon: Plus, featured: true },
    { label: 'My vehicles', description: 'Open your registered garage and service history.', path: '/vehicles', number: '02', icon: Car },
    { label: 'Service bookings', description: 'Review appointments and follow their progress.', path: '/bookings', number: '03', icon: CalendarCheck },
  ]

  return (
    <div className="portal-layout customer-portal">
      <CustomerSidebar />
      <main className="portal-main customer-profile-main">
        <PageHeader eyebrow="CUSTOMER PORTAL" title="My Account" description="Your vehicles, bookings and service information in one place." />
        <div className="portal-content customer-content customer-profile-page">
          {error && <div className="portal-error" role="alert">{error}</div>}

          {loading ? (
            <div className="portal-loading-card"><div className="loading-spinner" /><p>Loading your account…</p></div>
          ) : hasNoProfile ? (
            <section className="customer-onboarding-card">
              <span className="customer-section-label">WELCOME</span>
              <h2>Complete your customer profile</h2>
              <p>Add your contact information before registering vehicles and arranging service appointments.</p>
              <button className="portal-primary-button" onClick={() => navigate('/profile/edit')}>Complete profile</button>
            </section>
          ) : profile && (
            <>
              <motion.section className="customer-account-hero" initial={reduceMotion ? false : { opacity: 0, clipPath: 'inset(0 0 8% 0)' }} animate={{ opacity: 1, clipPath: 'inset(0 0 0% 0)' }} transition={{ duration: .62, ease: [0.22, 1, 0.36, 1] }}>
                <div className="customer-account-copy">
                  <span className="customer-hero-kicker">WELCOME BACK, {name.toUpperCase()}</span>
                  <h2>Your vehicle care,<br />simplified.</h2>
                  <p>Everything you need to plan your next service, follow its progress and keep your vehicle moving.</p>
                  <div className="customer-hero-actions">
                    <button className="portal-primary-button" onClick={() => navigate('/bookings/create')}>Book a service</button>
                    <button className="customer-hero-secondary" onClick={() => navigate('/bookings')}>View my bookings</button>
                  </div>
                </div>
                <div className="customer-account-summary">
                  <div className="customer-avatar" aria-hidden="true">{initial}</div>
                  <div><span>YOUR ACCOUNT</span><strong>{name}</strong><small>Customer #{profile.id} · Active</small></div>
                </div>
              </motion.section>

              <section className="customer-overview-grid" aria-label="Vehicles and bookings overview">
                <article className="customer-overview-panel">
                  <div className="customer-overview-heading"><div><span className="customer-section-label">YOUR GARAGE</span><h2>Registered vehicles</h2></div><button onClick={() => navigate('/vehicles')}>View all</button></div>
                  {vehicles.length ? <div className="customer-vehicle-list">{vehicles.slice(0, 4).map((vehicle) => <button className="customer-vehicle-item" key={vehicle.id} onClick={() => navigate(`/vehicles/${vehicle.id}/history`, { state: { vehicle } })}><span><strong>{vehicle.make} {vehicle.model}</strong><small>{vehicle.year} · {vehicle.fuelType || 'Vehicle'}</small></span><span>{vehicle.registrationNumber}</span></button>)}</div> : <div className="customer-overview-empty"><Car size={22} aria-hidden="true" /><strong>No vehicles registered</strong><p>Add a vehicle to begin managing its service history.</p><button className="customer-text-button" onClick={() => navigate('/vehicles/add')}>Add vehicle</button></div>}
                </article>

                <article className="customer-overview-panel customer-booking-focus">
                  <div className="customer-overview-heading"><div><span className="customer-section-label">NEXT STEP</span><h2>Service activity</h2></div></div>
                  {activeBooking ? <div><span className={statusClass(activeBooking.status)}>{activeBooking.status}</span><h3>{activeBooking.vehicleName || activeBooking.vehicleRegistrationNumber}</h3><p>{new Date(activeBooking.preferredDate).toLocaleDateString()} · {activeBooking.requestedServiceOrProblem}</p><button className="customer-text-button" onClick={() => navigate(`/bookings/${activeBooking.id}`)}>View booking</button></div> : <div className="customer-overview-empty"><CalendarCheck size={22} aria-hidden="true" /><strong>No active booking</strong><p>When you are ready, arrange your next workshop visit.</p><button className="customer-text-button" onClick={() => navigate('/bookings/create')}>Book a service</button></div>}
                </article>
              </section>

              <section className="customer-action-section">
                <div className="customer-section-heading"><div><span className="customer-section-label">QUICK ACCESS</span><h2>Continue your service journey</h2></div><p>Choose the next action that matters to you.</p></div>
                <div className="customer-action-grid">
                  {actions.map(({ icon: Icon, ...action }) => <motion.button className={`customer-action-card${action.featured ? ' featured' : ''}`} key={action.path} onClick={() => navigate(action.path)} whileHover={reduceMotion ? undefined : { y: -2 }}><span>{action.number}</span><Icon size={19} aria-hidden="true" /><strong>{action.label}</strong><p>{action.description}</p><b aria-hidden="true"><ArrowUpRight size={20} /></b></motion.button>)}
                </div>
              </section>

              <section className="customer-details-panel">
                <div className="customer-section-heading"><div><span className="customer-section-label">ACCOUNT DETAILS</span><h2>Personal information</h2></div><button className="customer-text-button" onClick={() => navigate('/profile/edit')}>Edit profile</button></div>
                <dl className="customer-detail-grid">
                  <div><dt>Email address</dt><dd>{profile.email}</dd></div>
                  <div><dt>Phone number</dt><dd>{profile.phone || 'Not provided'}</dd></div>
                  <div className="customer-detail-wide"><dt>Address</dt><dd>{profile.address || 'Not provided'}</dd></div>
                </dl>
              </section>
            </>
          )}
        </div>
      </main>
    </div>
  )
}

export default ProfilePage