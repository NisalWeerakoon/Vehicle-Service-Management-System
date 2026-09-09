import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import MechanicSidebar from '../components/MechanicSidebar'
import { clearAuth, customerApi } from '../services/api'

function EditMechanicProfilePage() {
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

      setSuccess('Mechanic profile updated successfully.')

      setTimeout(() => {
        navigate('/mechanic')
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
        <MechanicSidebar />
        <main className="portal-main">
          <div className="portal-loading-card">
            <div className="loading-spinner" />
            <p>Loading mechanic profile details...</p>
          </div>
        </main>
      </div>
    )
  }

  const initial = form.fullName?.charAt(0).toUpperCase() || 'M'

  return (
    <div className="portal-layout">
      <MechanicSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">MECHANIC PORTAL</span>
            <h1>Edit Profile</h1>
          </div>

          <div className="portal-user">
            <div
              className="portal-user-avatar"
              style={{ background: '#059669', color: 'white' }}
            >
              {initial}
            </div>
            <div>
              <strong>{form.fullName || 'Mechanic'}</strong>
              <span>Mechanic Specialist</span>
            </div>
          </div>
        </header>

        <div className="portal-content">
          <section className="edit-profile-heading">
            <div>
              <span className="profile-welcome-label">TECHNICIAN ACCOUNT SETTINGS</span>
              <h2>Update Mechanic Profile</h2>
              <p>Keep your technician contact details accurate for workshop operations.</p>
            </div>

            <button
              className="portal-back-button"
              onClick={() => navigate('/mechanic')}
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
                  background: 'linear-gradient(135deg, #059669, #10b981)',
                  color: 'white',
                }}
              >
                {initial}
              </div>

              <div>
                <span>MECHANIC PROFILE DETAILS</span>
                <h2>Edit Technician Profile</h2>
                <p>Update your display name and contact numbers for the workshop team.</p>
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
                      placeholder="Enter full name"
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
                  <small>Your login email is managed by system administration.</small>
                </div>

                <div className="modern-form-group modern-form-wide">
                  <label htmlFor="address">Workshop / Home Address</label>
                  <div className="modern-textarea-wrapper">
                    <span>🏢</span>
                    <textarea
                      id="address"
                      name="address"
                      value={form.address}
                      onChange={handleChange}
                      placeholder="Enter address details"
                      rows="4"
                      maxLength="250"
                    />
                  </div>
                </div>
              </div>

              <div className="modern-form-footer">
                <div>
                  <strong>Save Changes?</strong>
                  <p>Confirm information updates for your mechanic account.</p>
                </div>

                <div className="modern-form-actions">
                  <button
                    type="button"
                    className="portal-secondary-button"
                    onClick={() => navigate('/mechanic')}
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

export default EditMechanicProfilePage
