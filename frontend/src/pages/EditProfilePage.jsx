import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  clearAuth,
  customerApi,
} from '../services/api'

function EditProfilePage() {
  const navigate = useNavigate()

  const [form, setForm] = useState({
    fullName: '',
    phone: '',
    address: '',
  })

  const [email, setEmail] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    async function fetchProfile() {
      try {
        const profile =
          await customerApi.getMyProfile()

        setEmail(profile.email)

        setForm({
          fullName: profile.fullName,
          phone: profile.phone,
          address: profile.address || '',
        })
      } catch (err) {
        if (err.status === 401) {
          clearAuth()
          navigate('/login')
          return
        }

        setError(err.message)
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
      await customerApi.updateMyProfile({
        fullName: form.fullName,
        phone: form.phone,
        address:
          form.address.trim() === ''
            ? null
            : form.address,
      })

      setSuccess(
        'Profile updated successfully.',
      )

      setTimeout(() => {
        navigate('/profile')
      }, 800)
    } catch (err) {
      setError(err.message)
    } finally {
      setSaving(false)
    }
  }

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
    form.fullName
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

            <h1>Edit Profile</h1>
          </div>

          <div className="portal-user">
            <div className="portal-user-avatar">
              {initial}
            </div>

            <div>
              <strong>
                {form.fullName || 'Customer'}
              </strong>

              <span>Customer</span>
            </div>
          </div>
        </header>

        <div className="portal-content">
          <section className="edit-profile-heading">
            <div>
              <span className="profile-welcome-label">
                ACCOUNT SETTINGS
              </span>

              <h2>
                Update your information
              </h2>

              <p>
                Keep your contact details accurate
                so we can provide you with the best
                service experience.
              </p>
            </div>

            <button
              className="portal-back-button"
              onClick={() =>
                navigate('/profile')
              }
            >
              ← Back to Profile
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
              <div className="edit-profile-avatar">
                {initial}
              </div>

              <div>
                <span>
                  PERSONAL INFORMATION
                </span>

                <h2>Edit Customer Profile</h2>

                <p>
                  Update the information associated
                  with your customer account.
                </p>
              </div>
            </div>

            <form
              className="modern-profile-form"
              onSubmit={handleSubmit}
            >
              <div className="modern-form-grid">
                <div className="modern-form-group">
                  <label htmlFor="fullName">
                    Full Name
                  </label>

                  <div className="modern-input-wrapper">
                    <span>♙</span>

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
                  <label htmlFor="phone">
                    Phone Number
                  </label>

                  <div className="modern-input-wrapper">
                    <span>☎</span>

                    <input
                      id="phone"
                      name="phone"
                      type="tel"
                      value={form.phone}
                      onChange={handleChange}
                      placeholder="Enter phone number"
                      required
                      maxLength="20"
                    />
                  </div>
                </div>

                <div className="modern-form-group modern-form-wide">
                  <label>
                    Email Address
                  </label>

                  <div className="modern-input-wrapper disabled-input">
                    <span>✉</span>

                    <input
                      type="email"
                      value={email}
                      disabled
                    />
                  </div>

                  <small>
                    Your login email cannot be
                    changed from this screen.
                  </small>
                </div>

                <div className="modern-form-group modern-form-wide">
                  <label htmlFor="address">
                    Address
                  </label>

                  <div className="modern-textarea-wrapper">
                    <span>⌂</span>

                    <textarea
                      id="address"
                      name="address"
                      value={form.address}
                      onChange={handleChange}
                      placeholder="Enter your address"
                      rows="4"
                      maxLength="250"
                    />
                  </div>

                  <small className="character-count">
                    {form.address.length}/250
                    characters
                  </small>
                </div>
              </div>

              <div className="modern-form-footer">
                <div>
                  <strong>
                    Ready to save?
                  </strong>

                  <p>
                    Review your information before
                    updating your profile.
                  </p>
                </div>

                <div className="modern-form-actions">
                  <button
                    type="button"
                    className="portal-secondary-button"
                    onClick={() =>
                      navigate('/profile')
                    }
                  >
                    Cancel
                  </button>

                  <button
                    type="submit"
                    className="portal-primary-button"
                    disabled={saving}
                  >
                    {saving
                      ? 'Saving...'
                      : 'Save Changes'}
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

export default EditProfilePage