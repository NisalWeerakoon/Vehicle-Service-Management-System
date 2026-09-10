import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  bookingApi,
  clearAuth,
  jobCardApi,
} from '../services/api'

function BookingsPage() {
  const navigate = useNavigate()

  const [bookings, setBookings] = useState([])
  const [jobCards, setJobCards] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    async function fetchBookings() {
      try {
        const [bookingsData, jobCardsData] = await Promise.all([
          bookingApi.getMyBookings(),
          jobCardApi.getAll().catch(() => []),
        ])

        setBookings(bookingsData || [])
        setJobCards(jobCardsData || [])
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
    const s = status ? status.toLowerCase().replace(/\s+/g, '-') : ''
    return `booking-status status-${s}`
  }

  const pendingCount = bookings.filter(
    (booking) => booking.status === 'Pending',
  ).length

  const completedCount = bookings.filter(
    (booking) => booking.status === 'Completed',
  ).length

  if (loading) {
    return (
      <div className="portal-layout">
        <CustomerSidebar />

        <main className="portal-main">
          <div className="portal-loading-card">
            <div className="loading-spinner" />
            <p>Loading service bookings...</p>
          </div>
        </main>
      </div>
    )
  }

  return (
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">
              CUSTOMER PORTAL
            </span>

            <h1>My Bookings</h1>
          </div>

          <button
            className="portal-primary-button"
            onClick={() =>
              navigate('/bookings/create')
            }
          >
            + Create Booking
          </button>
        </header>

        <div className="portal-content">
          <section className="booking-page-heading">
            <div>
              <span className="profile-welcome-label">
                SERVICE OVERVIEW
              </span>

              <h2>Your service bookings</h2>

              <p>
                Track your appointments, booking status and
                requested vehicle services.
              </p>
            </div>
          </section>

          <section className="booking-stats-grid">
            <div className="booking-stat-card">
              <span>Total Bookings</span>
              <strong>{bookings.length}</strong>
              <p>All service requests</p>
            </div>

            <div className="booking-stat-card">
              <span>Pending</span>
              <strong>{pendingCount}</strong>
              <p>Waiting for confirmation</p>
            </div>

            <div className="booking-stat-card">
              <span>Completed</span>
              <strong>{completedCount}</strong>
              <p>Finished services</p>
            </div>
          </section>

          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          {bookings.length === 0 ? (
            <section className="modern-empty-state">
              <div className="modern-empty-icon">
                🛠️
              </div>

              <span>NO BOOKINGS YET</span>

              <h2>No service bookings found</h2>

              <p>
                Create your first booking and schedule your
                vehicle for service.
              </p>

              <button
                className="portal-primary-button"
                onClick={() =>
                  navigate('/bookings/create')
                }
              >
                Create First Booking
              </button>
            </section>
          ) : (
            <section className="modern-booking-list">
              {bookings.map((booking) => {
                const matchedJobCard = jobCards.find(
                  (j) =>
                    j.bookingId === booking.id ||
                    (j.vehicleRegistrationNumber &&
                      booking.vehicleRegistrationNumber &&
                      j.vehicleRegistrationNumber.trim().toLowerCase() ===
                        booking.vehicleRegistrationNumber.trim().toLowerCase()),
                )

                return (
                  <article
                    className="modern-booking-card"
                    key={booking.id}
                  >
                    <div className="modern-booking-top">
                      <div>
                        <span className="booking-reference">
                          {booking.bookingReference}
                        </span>

                        <h2>{booking.vehicleName}</h2>

                        <p>
                          {booking.vehicleRegistrationNumber}
                        </p>
                      </div>

                      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: '6px' }}>
                        <span className={getStatusClass(booking.status)}>
                          Booking: {booking.status}
                        </span>

                        {matchedJobCard && (
                          <span
                            className={getStatusClass(matchedJobCard.status)}
                            style={{ background: '#eff6ff', color: '#1d4ed8', border: '1px solid #bfdbfe' }}
                          >
                            Live Maintenance: {matchedJobCard.status}
                          </span>
                        )}
                      </div>
                    </div>

                    <div className="modern-booking-info">
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

                    <div className="modern-booking-request">
                      <span>
                        Requested Service / Problem
                      </span>

                      <p>
                        {booking.requestedServiceOrProblem}
                      </p>
                    </div>

                    <div className="modern-booking-footer" style={{ gap: '12px', flexWrap: 'wrap' }}>
                      <button
                        className="vehicle-outline-button"
                        onClick={() =>
                          navigate(
                            `/bookings/${booking.id}`,
                          )
                        }
                      >
                        View Details
                      </button>

                      {matchedJobCard && (
                        <button
                          className="portal-primary-button"
                          onClick={() =>
                            navigate(`/jobs/${matchedJobCard.id}/status`)
                          }
                        >
                          Track Live Status 🛠️
                        </button>
                      )}
                    </div>
                  </article>
                )
              })}
            </section>
          )}
        </div>
      </main>
    </div>
  )
}

export default BookingsPage