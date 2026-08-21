import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  authApi,
  clearAuth,
  customerApi,
} from '../services/api'

function ProfilePage() {
  const navigate = useNavigate()

  const [profile, setProfile] = useState(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function fetchProfile() {
      setLoading(true)
      setError('')

      try {
        const data = await customerApi.getMyProfile()
        setProfile(data)
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

  async function handleLogout() {
    try {
      await authApi.logout()
    } catch {
      // Logout is stateless for JWT.
      // Local authentication will still be removed.
    }

    clearAuth()
    navigate('/login')
  }

  if (loading) {
    return (
      <div className="page-container">
        <div className="profile-card">
          <p>Loading profile...</p>
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

        <button
          className="logout-button"
          onClick={handleLogout}
        >
          Logout
        </button>
      </header>

      <main className="dashboard-content">
        <div className="page-heading">
          <div>
            <h1>My Profile</h1>
            <p>
              View and maintain your customer information.
            </p>
          </div>

          {profile && (
            <button
              className="primary-button small-button"
              onClick={() => navigate('/profile/edit')}
            >
              Edit Profile
            </button>
          )}
        </div>

        {error && (
          <div className="alert error-alert">
            {error}
          </div>
        )}

        {profile && (
          <div className="profile-card">
            <div className="profile-avatar">
              {profile.fullName
                ?.charAt(0)
                .toUpperCase()}
            </div>

            <h2>{profile.fullName}</h2>

            <p className="profile-subtitle">
              Customer #{profile.id}
            </p>

            <div className="profile-grid">
              <div className="profile-field">
                <span>Email</span>
                <strong>{profile.email}</strong>
              </div>

              <div className="profile-field">
                <span>Phone</span>
                <strong>{profile.phone}</strong>
              </div>

              <div className="profile-field full-width">
                <span>Address</span>
                <strong>
                  {profile.address || 'Not provided'}
                </strong>
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  )
}

export default ProfilePage