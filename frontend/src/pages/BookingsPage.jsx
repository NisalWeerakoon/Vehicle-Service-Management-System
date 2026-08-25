import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import {
  bookingApi,
  clearAuth,
} from '../services/api'

function BookingsPage() {
  const navigate = useNavigate()

  const [bookings, setBookings] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    async function fetchBookings() {
      try {
        const data = await bookingApi.getMyBookings()
        setBookings(data)
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

    fetchBookings()
  }, [navigate])

  function getStatusClass(status) {
    return `booking-status status-${status.toLowerCase()}`
  }

  if (loading) {
    return (
      <div className="page-container">
        <div className="profile-card">
          Loading service bookings...
        </div>
      </div>
    )
  }

  return (
    <div className="dashboard-page">
      <header className="top-bar">
        <div>
          <h2>AutoCare Service Center</h2>
          <span>Customer Portal</span>
        </div>

        <button
          className="secondary-button"
          onClick={() => navigate('/profile')}
        >
          👤 My Profile
        </button>
      </header>

      <main className="dashboard-content">
        <div className="page-heading">
          <div>
            <h1>🔧 My Service Bookings</h1>
            <p>
              Track your vehicle service appointments and
              requested work.
            </p>
          </div>

          <button
            className="primary-button small-button"
            onClick={() => navigate('/bookings/create')}
          >
            + Create Booking
          </button>
        </div>

        <div className="stats-grid">
          <div className="stat-card">
            <div className="stat-title">
              Total Bookings
            </div>

            <div className="stat-value">
              {bookings.length}
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-title">
              Pending
            </div>

            <div className="stat-value">
              {
                bookings.filter(
                  (booking) =>
                    booking.status === 'Pending',
                ).length
              }
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-title">
              Completed
            </div>

            <div className="stat-value">
              {
                bookings.filter(
                  (booking) =>
                    booking.status === 'Completed',
                ).length
              }
            </div>
          </div>
        </div>

        {error && (
          <div className="alert error-alert">
            {error}
          </div>
        )}

        {bookings.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">
              🛠️
            </div>

            <h2>No service bookings yet</h2>

            <p>
              Create your first booking and schedule your
              vehicle for service.
            </p>

            <button
              className="primary-button small-button"
              onClick={() =>
                navigate('/bookings/create')
              }
            >
              Create First Booking
            </button>
          </div>
        ) : (
          <div className="booking-grid">
            {bookings.map((booking) => (
              <div
                className="booking-card"
                key={booking.id}
              >
                <div className="booking-card-top">
                  <div>
                    <span className="booking-reference">
                      {booking.bookingReference}
                    </span>

                    <h2>
                      {booking.vehicleName}
                    </h2>

                    <p className="booking-registration">
                      🚘 {booking.vehicleRegistrationNumber}
                    </p>
                  </div>

                  <span
                    className={getStatusClass(
                      booking.status,
                    )}
                  >
                    {booking.status}
                  </span>
                </div>

                <div className="booking-info-grid">
                  <div>
                    <span>Preferred Date</span>
                    <strong>
                      {new Date(
                        booking.preferredDate,
                      ).toLocaleDateString()}
                    </strong>
                  </div>

                  <div>
                    <span>Created</span>
                    <strong>
                      {new Date(
                        booking.createdAt,
                      ).toLocaleDateString()}
                    </strong>
                  </div>
                </div>

                <div className="booking-problem">
                  <span>Requested Service / Problem</span>

                  <p>
                    {booking.requestedServiceOrProblem}
                  </p>
                </div>

                <button
                  className="secondary-button"
                  onClick={() =>
                    navigate(
                      `/bookings/${booking.id}`,
                    )
                  }
                >
                  View Booking Details →
                </button>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  )
}

export default BookingsPage