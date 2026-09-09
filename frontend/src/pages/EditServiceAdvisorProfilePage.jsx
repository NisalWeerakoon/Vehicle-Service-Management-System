import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import ServiceAdvisorSidebar from '../components/ServiceAdvisorSidebar'
import { clearAuth, customerApi } from '../services/api'

function EditServiceAdvisorProfilePage() {
  const navigate = useNavigate()

  const [form, setForm] = useState({
    fullName: '',
    phone: '',
    address: '',
  })

  const [email, setEmail] = useState(localStorage.getItem('email') || '')
  const [profileExists, setProfileExists] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    async function fetchProfile() {
      try {
        setLoading(true)
        const profile = await customerApi.getMyProfile()
        if (profile) {
          setEmail(profile.email || localStorage.getItem('email') || '')
          setForm({
            fullName: profile.fullName || '',
            phone: profile.phone || '',
            address: profile.address || '',
          })
          setProfileExists(true)
        }
      } catch (err) {
        if (err.status === 401) {
          clearAuth()
          navigate('/login')
          return
        }
        // Profile might not exist yet for new staff account
        setProfileExists(false)
      } finally {
        setLoading(false)
      }
    }

    fetchProfile()
  }, [navigate])

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({
      ...current,
      [name]: value,
    }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setSaving(true)
    setError('')
    setSuccess('')

    try {
      const payload = {
        fullName: form.fullName,
        phone: form.phone,
        address: form.address.trim() === '' ? null : form.address,
      }

      if (profileExists) {
        await customerApi.updateMyProfile(payload)
      } else {
        await customerApi.createMyProfile(payload)
      }

      setSuccess('Service Advisor profile updated successfully.')

      setTimeout(() => {
        navigate('/service-advisor')
      }, 800)
    } catch (err) {
      setError(err.message || 'Failed to update profile.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="portal-layout">
        <ServiceAdvisorSidebar />
        <main className="portal-main">
          <div className="portal-loading-card">
            <div className="loading-spinner" />
            <p>Loading profile details...</p>
          </div>
        </main>
      </div>
    )
  }

  const initial = form.fullName?.charAt(0).toUpperCase() || 'S'

  return (
    <div className="portal-layout">
      <ServiceAdvisorSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">SERVICE ADVISOR PORTAL</span>
            <h1>Edit Profile</h1>
          </div>

          <div className="portal-user">
            <div
              className="portal-user-avatar"
              style={{ background: '#2563eb', color: 'white' }}
            >
              {initial}
            </div>
            <div>
              <strong>{form.fullName || 'Service Advisor'}</strong>
              <span>Service Advisor</span>
            </div>
          </div>
        </header>

        <div className="portal-content">
          <section className="edit-profile-heading">
            <div>
              <span className="profile-welcome-label">STAFF ACCOUNT SETTINGS</span>
              <h2>Update Advisor Information</h2>
              <p>
                Keep your staff contact details accurate for operational management and job card assignments.
              </p>
            </div>

            <button
              className="portal-back-button"
              onClick={() => navigate('/service-advisor')}
            >
              ← Back to Dashboard
            </button>
          </section>

          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          {success && (
            <div className="portal-success">
              <span>✓</span>
              {success}
            </div>
          )}

          <section className="modern-edit-profile-card">
            <div className="edit-profile-card-header">
              <div
                className="edit-profile-avatar"
                style={{
                  background: 'linear-gradient(135deg, #2563eb, #0ea5e9)',
                  color: 'white',
                }}
              >
                {initial}
              </div>

              <div>
                <span>SERVICE ADVISOR DETAILS</span>
                <h2>Edit Staff Profile</h2>
                <p>Update your display name and contact numbers for the service management system.</p>
              </div>
            </div>

            <form className="modern-profile-form" onSubmit={handleSubmit}>
              <div className="modern-form-grid">
                <div className="modern-form-group">
                  <label htmlFor="fullName">Full Name</label>
                  <div className="modern-input-wrapper">
                    <span>👤</span>
                    <input
                      id="fullName"
                      name="fullName"
                      type="text"
                      value={form.fullName}
                      onChange={handleChange}
                      placeholder="Enter your full name"
                      required
                      maxLength="120"
                    />
                  </div>
                </div>

                <div className="modern-form-group">
                  <label htmlFor="phone">Phone Number</label>
                  <div className="modern-input-wrapper">
                    <span>📞</span>
                    <input
                      id="phone"
                      name="phone"
                      type="tel"
                      value={form.phone}
                      onChange={handleChange}
                      placeholder="Enter contact phone number"
                      required
                      maxLength="20"
                    />
                  </div>
                </div>

                <div className="modern-form-group modern-form-wide">
                  <label>Email Address</label>
                  <div className="modern-input-wrapper disabled-input">
                    <span>✉️</span>
                    <input type="email" value={email} disabled />
                  </div>
                  <small>Your account email is locked and managed by administration.</small>
                </div>

                <div className="modern-form-group modern-form-wide">
                  <label htmlFor="address">Office / Branch Address</label>
                  <div className="modern-textarea-wrapper">
                    <span>🏢</span>
                    <textarea
                      id="address"
                      name="address"
                      value={form.address}
                      onChange={handleChange}
                      placeholder="Enter branch or work location address"
                      rows="4"
                      maxLength="250"
                    />
                  </div>
                  <small className="character-count">
                    {form.address.length}/250 characters
                  </small>
                </div>
              </div>

              <div className="modern-form-footer">
                <div>
                  <strong>Save Changes?</strong>
                  <p>Confirm information updates for your Service Advisor account.</p>
                </div>

                <div className="modern-form-actions">
                  <button
                    type="button"
                    className="portal-secondary-button"
                    onClick={() => navigate('/service-advisor')}
                  >
                    Cancel
                  </button>

                  <button
                    type="submit"
                    className="portal-primary-button"
                    disabled={saving}
                  >
                    {saving ? 'Saving...' : 'Save Profile'}
                  </button>
                </div>
              </div>
            </form>
          </section>
        </div>
      </main>
    </div>
  )
}

export default EditServiceAdvisorProfilePage
