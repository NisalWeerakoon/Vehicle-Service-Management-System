import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import CustomerSidebar from '../components/CustomerSidebar'
import PageHeader from '../components/PageHeader'
import { clearAuth, customerApi } from '../services/api'

function ProfilePage() {
  const navigate = useNavigate()
  const [profile, setProfile] = useState(null)
  const [hasNoProfile, setHasNoProfile] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    async function fetchProfile() {
      try {
        const data = await customerApi.getMyProfile()
        if (active) setProfile(data)
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
    void fetchProfile()
    return () => { active = false }
  }, [navigate])

  const name = profile?.fullName || 'Customer'
  const initial = name.charAt(0).toUpperCase()
  const actions = [
    { label: 'My vehicles', description: 'Manage the vehicles connected to your account.', path: '/vehicles', number: '01' },
    { label: 'Service bookings', description: 'Review appointments and follow their progress.', path: '/bookings', number: '02' },
    { label: 'Book a service', description: 'Schedule your next workshop appointment.', path: '/bookings/create', number: '03' },
  ]

  return (
    <div className="portal-layout customer-portal">
      <CustomerSidebar />
      <main className="portal-main">
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
              <section className="customer-account-hero">
                <div className="customer-account-copy">
                  <span className="customer-hero-kicker">WELCOME BACK, {name.toUpperCase()}</span>
                  <h2>Keep every journey<br />running smoothly.</h2>
                  <p>Plan your next visit, manage your vehicles and follow every service update from one calm, connected place.</p>
                  <div className="customer-hero-actions">
                    <button className="portal-primary-button" onClick={() => navigate('/bookings/create')}>Book a service</button>
                    <button className="customer-hero-secondary" onClick={() => navigate('/bookings')}>View my bookings</button>
                  </div>
                </div>
                <div className="customer-account-summary">
                  <div className="customer-avatar" aria-hidden="true">{initial}</div>
                  <div><span>YOUR ACCOUNT</span><strong>{name}</strong><small>Customer #{profile.id} · Active</small></div>
                </div>
              </section>

              <section className="customer-action-section">
                <div className="customer-section-heading"><div><span className="customer-section-label">QUICK ACCESS</span><h2>What would you like to do?</h2></div><p>Everything you need for your next service visit.</p></div>
                <div className="customer-action-grid">
                  {actions.map((action) => <button className="customer-action-card" key={action.path} onClick={() => navigate(action.path)}><span>{action.number}</span><strong>{action.label}</strong><p>{action.description}</p><b aria-hidden="true">→</b></button>)}
                </div>
              </section>

              <section className="customer-details-panel">
                <div className="customer-section-heading"><div><span className="customer-section-label">PERSONAL DETAILS</span><h2>Contact information</h2></div><button className="customer-text-button" onClick={() => navigate('/profile/edit')}>Edit profile</button></div>
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