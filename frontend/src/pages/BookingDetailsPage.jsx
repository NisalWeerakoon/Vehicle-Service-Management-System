import { useEffect, useState } from 'react'
import {
  useNavigate,
  useParams,
} from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  bookingApi,
  clearAuth,
} from '../services/api'

function BookingDetailsPage() {
  const navigate = useNavigate()
  const { id } = useParams()

  const [booking, setBooking] = useState(null)
  const [loading, setLoading] = useState(true)
  const [cancelling, setCancelling] =
    useState(false)

  const [error, setError] = useState('')

  useEffect(() => {
    async function fetchBooking() {
      try {
        const data =
          await bookingApi.getMyBooking(id)

        setBooking(data)
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

    fetchBooking()
  }, [id, navigate])

  async function handleCancelBooking() {
    const confirmed = window.confirm(
      'Are you sure you want to cancel this service booking?',
    )

    if (!confirmed) {
      return
    }

    setCancelling(true)
    setError('')

    try {
      const updatedBooking =
        await bookingApi.cancelMyBooking(id)

      setBooking(updatedBooking)
    } catch (err) {
      if (err.status === 401) {
        clearAuth()
        navigate('/login')
        return
      }

      setError(err.message)
    } finally {
      setCancelling(false)
    }
  }

  if (loading) {
    return (
      <div className="portal-layout">
        <CustomerSidebar />

        <main className="portal-main">
          <div className="portal-loading-card">
            <div className="loading-spinner" />
            <p>Loading booking...</p>
          </div>
        </main>
      </div>
    )
  }

  if (!booking) {
    return (
      <div className="portal-layout">
        <CustomerSidebar />

        <main className="portal-main">
          <div className="portal-content">
            <section className="modern-empty-state">
              <div className="modern-empty-icon">
                !
              </div>

              <h2>Booking unavailable</h2>
              <p>{error}</p>

              <button
                className="portal-primary-button"
                onClick={() =>
                  navigate('/bookings')
                }
              >
                Back to My Bookings
              </button>
            </section>
          </div>
        </main>
      </div>
    )
  }

  const canEdit =
    booking.status === 'Pending' ||
    booking.status === 'Confirmed'

  const canCancel =
    booking.status !== 'InService' &&
    booking.status !== 'Completed' &&
    booking.status !== 'Cancelled'

  return (
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">
              CUSTOMER PORTAL
            </span>

            <h1>Booking Details</h1>
          </div>

          <button
            className="portal-back-button"
            onClick={() => navigate('/bookings')}
          >
            ← My Bookings
          </button>
        </header>

        <div className="portal-content">
          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          <section className="modern-booking-details">
            <div className="booking-details-premium-header">
              <div>
                <span className="booking-reference">
                  {booking.bookingReference}
                </span>

                <h2>Service Booking</h2>

                <p>
                  Created{' '}
                  {new Date(
                    booking.createdAt,
                  ).toLocaleString()}
                </p>
              </div>

              <span
                className={`booking-status status-${booking.status.toLowerCase()}`}
              >
                {booking.status}
              </span>
            </div>

            <div className="premium-vehicle-banner">
              <div className="vehicle-form-header-icon">
                🚘
              </div>

              <div>
                <span>VEHICLE</span>

                <h2>{booking.vehicleName}</h2>

                <p>
                  {booking.vehicleRegistrationNumber}
                </p>
              </div>
            </div>

            <div className="premium-booking-grid">
              <div>
                <span>Preferred Service Date</span>
                <strong>
                  {new Date(
                    booking.preferredDate,
                  ).toLocaleDateString()}
                </strong>
              </div>

              <div>
                <span>Booking Status</span>
                <strong>{booking.status}</strong>
              </div>

              <div>
                <span>Booking ID</span>
                <strong>#{booking.id}</strong>
              </div>

              <div>
                <span>Vehicle ID</span>
                <strong>#{booking.vehicleId}</strong>
              </div>
            </div>

            <div className="premium-service-request">
              <span>REQUESTED SERVICE / PROBLEM</span>

              <p>
                {booking.requestedServiceOrProblem}
              </p>
            </div>

            <div className="premium-booking-actions">
              {canEdit && (
                <button
                  className="portal-primary-button"
                  onClick={() =>
                    navigate(
                      `/bookings/${id}/edit`,
                    )
                  }
                >
                  Edit Booking
                </button>
              )}

              {canCancel && (
                <button
                  className="premium-danger-button"
                  onClick={handleCancelBooking}
                  disabled={cancelling}
                >
                  {cancelling
                    ? 'Cancelling...'
                    : 'Cancel Booking'}
                </button>
              )}
            </div>

            {booking.status === 'Cancelled' && (
              <div className="portal-error premium-cancelled-message">
                <span>!</span>
                This booking has been cancelled.
              </div>
            )}

            <div className="premium-booking-timeline">
              <h3>Booking Progress</h3>

              <div className="premium-timeline-step active">
                <span />
                <div>
                  <strong>Booking Created</strong>
                  <p>Your request has been received.</p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${
                  booking.status !== 'Pending' &&
                  booking.status !== 'Cancelled'
                    ? 'active'
                    : ''
                }`}
              >
                <span />
                <div>
                  <strong>Booking Confirmed</strong>
                  <p>
                    Service center confirms the appointment.
                  </p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${
                  [
                    'CheckedIn',
                    'InService',
                    'Completed',
                  ].includes(booking.status)
                    ? 'active'
                    : ''
                }`}
              >
                <span />
                <div>
                  <strong>Vehicle Check-In</strong>
                  <p>
                    Vehicle arrives at the service center.
                  </p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${
                  booking.status === 'Completed'
                    ? 'active'
                    : ''
                }`}
              >
                <span />
                <div>
                  <strong>Service Completed</strong>
                  <p>Vehicle servicing is completed.</p>
                </div>
              </div>
            </div>
          </section>
        </div>
      </main>
    </div>
  )
}

export default BookingDetailsPage