import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
        const profile = await customerApi.getMyProfile()

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

      setSuccess('Profile updated successfully.')

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
      <div className="page-container">
        <div className="profile-card">
          Loading profile...
        </div>
      </div>
    )
  }

  return (
    <div className="dashboard-page">
      <header className="top-bar">
        <div>
          <h2>Vehicle Service Center</h2>
          <span>Customer Portal</span>
        </div>
      </header>

      <main className="dashboard-content">
        <div className="edit-card">
          <h1>Edit Profile</h1>

          <p>
            Update your permitted customer information.
          </p>

          {error && (
            <div className="alert error-alert">
              {error}
            </div>
          )}

          {success && (
            <div className="alert success-alert">
              {success}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Email</label>

              <input
                type="email"
                value={email}
                disabled
              />

              <small>
                Login email cannot be changed
                from the profile screen.
              </small>
            </div>

            <div className="form-group">
              <label htmlFor="fullName">
                Full Name
              </label>

              <input
                id="fullName"
                name="fullName"
                value={form.fullName}
                onChange={handleChange}
                required
                maxLength="120"
              />
            </div>

            <div className="form-group">
              <label htmlFor="phone">
                Phone Number
              </label>

              <input
                id="phone"
                name="phone"
                type="tel"
                value={form.phone}
                onChange={handleChange}
                required
                maxLength="20"
              />
            </div>

            <div className="form-group">
              <label htmlFor="address">
                Address
              </label>

              <textarea
                id="address"
                name="address"
                value={form.address}
                onChange={handleChange}
                rows="4"
                maxLength="250"
              />
            </div>

            <div className="button-row">
              <button
                type="button"
                className="secondary-button"
                onClick={() => navigate('/profile')}
              >
                Cancel
              </button>

              <button
                type="submit"
                className="primary-button"
                disabled={saving}
              >
                {saving
                  ? 'Saving...'
                  : 'Save Changes'}
              </button>
            </div>
          </form>
        </div>
      </main>
    </div>
  )
}

export default EditProfilePage