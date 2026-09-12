import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  clearAuth,
  customerApi,
} from '../services/api'

function ProfilePage() {
  const navigate = useNavigate()

  const [profile, setProfile] = useState(null)
  const [hasNoProfile, setHasNoProfile] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function fetchProfile() {
      setLoading(true)
      setError('')
      setHasNoProfile(false)

      try {
        const data =
          await customerApi.getMyProfile()

        setProfile(data)
      } catch (err) {
        if (err.status === 401) {
          clearAuth()
          navigate('/login')
          return
        }

        if (err.status === 404 || err.message?.includes('not been created')) {
          setHasNoProfile(true)
        } else {
          setError(err.message)
        }
      } finally {
        setLoading(false)
      }
    }

    fetchProfile()
  }, [navigate])

  if (loading) {
    return (
      <div className="portal-layout">
        <CustomerSidebar />

        <main className="portal-main">
          <div className="portal-loading-card">
            <div className="loading-spinner" />
            <p>Loading your profile...</p>
          </div>
        </main>
      </div>
    )
  }

  const initial =
    profile?.fullName
      ?.charAt(0)
      .toUpperCase() || 'C'

  return (
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">
              CUSTOMER PORTAL
            </span>

            <h1>Profile</h1>
          </div>

          {profile && (
            <div className="portal-user">
              <div className="portal-user-avatar">
                {initial}
              </div>

              <div>
                <strong>
                  {profile.fullName}
                </strong>

                <span>Customer</span>
              </div>
            </div>
          )}
        </header>

        <div className="portal-content">
          <section className="profile-welcome">
            <div>
              <span className="profile-welcome-label">
                ACCOUNT OVERVIEW
              </span>

              <h2>
                Your personal information
              </h2>

              <p>
                Manage your customer details and
                keep your service information
                up to date.
              </p>
            </div>

            {profile && (
              <button
                className="portal-primary-button"
                onClick={() =>
                  navigate('/profile/edit')
                }
              >
                Edit Profile
              </button>
            )}
          </section>

          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          {hasNoProfile && (
            <section className="portal-card" style={{ padding: '40px 24px', textAlign: 'center', marginTop: '20px' }}>
              <div style={{ fontSize: '48px', marginBottom: '16px' }}>👤</div>
              <h2 style={{ fontSize: '22px', fontWeight: 800, marginBottom: '8px', color: '#0f172a' }}>
                Welcome to Vehicle Service Center!
              </h2>
              <p style={{ color: '#64748b', maxWidth: '500px', margin: '0 auto 24px', lineHeight: '1.6' }}>
                Your account is active, but your Customer Profile details (Full Name, Phone Number, Address) have not been completed yet. Please set up your profile to manage your vehicles and service bookings.
              </p>
              <button
                className="portal-primary-button"
                style={{ padding: '12px 28px', fontSize: '15px' }}
                onClick={() => navigate('/profile/edit')}
              >
                + Complete Customer Profile
              </button>
            </section>
          )}

          {profile && (
            <>
              <section className="modern-profile-card">
                <div className="profile-card-header">
                  <div className="profile-avatar-large">
                    {initial}
                  </div>

                  <div className="profile-identity">
                    <span>Customer Profile</span>

                    <h2>
                      {profile.fullName}
                    </h2>

                    <p>
                      Customer #{profile.id}
                    </p>
                  </div>

                  <span className="account-status">
                    ● Active Account
                  </span>
                </div>

                <div className="profile-section-title">
                  <div>
                    <span>
                      PERSONAL INFORMATION
                    </span>

                    <h3>
                      Contact Details
                    </h3>
                  </div>
                </div>

                <div className="modern-profile-grid">
                  <div className="modern-profile-field">
                    <div className="field-icon">
                      ✉
                    </div>

                    <div>
                      <span>
                        Email Address
                      </span>

                      <strong>
                        {profile.email}
                      </strong>
                    </div>
                  </div>

                  <div className="modern-profile-field">
                    <div className="field-icon">
                      ☎
                    </div>

                    <div>
                      <span>
                        Phone Number
                      </span>

                      <strong>
                        {profile.phone ||
                          'Not provided'}
                      </strong>
                    </div>
                  </div>

                  <div className="modern-profile-field profile-field-wide">
                    <div className="field-icon">
                      ⌂
                    </div>

                    <div>
                      <span>
                        Address
                      </span>

                      <strong>
                        {profile.address ||
                          'Not provided'}
                      </strong>
                    </div>
                  </div>
                </div>
              </section>

              <section className="profile-quick-actions">
                <div
                  className="quick-action-card"
                  onClick={() =>
                    navigate('/vehicles')
                  }
                >
                  <div className="quick-action-icon">
                    🚘
                  </div>

                  <div>
                    <h3>My Vehicles</h3>

                    <p>
                      View and manage vehicles
                      linked to your account.
                    </p>
                  </div>

                  <span className="quick-action-arrow">
                    →
                  </span>
                </div>

                <div
                  className="quick-action-card"
                  onClick={() =>
                    navigate('/bookings')
                  }
                >
                  <div className="quick-action-icon">
                    ▣
                  </div>

                  <div>
                    <h3>My Bookings</h3>

                    <p>
                      Review your service
                      bookings and their status.
                    </p>
                  </div>

                  <span className="quick-action-arrow">
                    →
                  </span>
                </div>

                <div
                  className="quick-action-card"
                  onClick={() =>
                    navigate(
                      '/bookings/create',
                    )
                  }
                >
                  <div className="quick-action-icon">
                    ＋
                  </div>

                  <div>
                    <h3>Book a Service</h3>

                    <p>
                      Schedule your next vehicle
                      service appointment.
                    </p>
                  </div>

                  <span className="quick-action-arrow">
                    →
                  </span>
                </div>
              </section>
            </>
          )}
        </div>
      </main>
    </div>
  )
}

export default ProfilePage