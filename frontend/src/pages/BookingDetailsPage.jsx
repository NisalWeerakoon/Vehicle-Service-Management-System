import { useEffect, useState } from 'react'
import {
  useNavigate,
  useParams,
} from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  bookingApi,
  clearAuth,
  jobCardApi,
} from '../services/api'
import { Car } from 'lucide-react'

function BookingDetailsPage() {
  const navigate = useNavigate()
  const { id } = useParams()

  const [booking, setBooking] = useState(null)
  const [jobCard, setJobCard] = useState(null)
  const [loading, setLoading] = useState(true)
  const [cancelling, setCancelling] =
    useState(false)

  const [error, setError] = useState('')

  useEffect(() => {
    async function fetchBookingAndJob() {
      try {
        const data =
          await bookingApi.getMyBooking(id)
        setBooking(data)

        if (data) {
          const allJobs = await jobCardApi.getAll().catch(() => [])
          const matched = (allJobs || []).find(
            (j) =>
              j.bookingId === data.id ||
              (j.vehicleRegistrationNumber &&
                data.vehicleRegistrationNumber &&
                j.vehicleRegistrationNumber.trim().toLowerCase() ===
                data.vehicleRegistrationNumber.trim().toLowerCase()),
          )
          setJobCard(matched || null)
        }
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

    fetchBookingAndJob()
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
      <div className="portal-layout customer-portal">
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
      <div className="portal-layout customer-portal">
        <CustomerSidebar />

        <main className="portal-main">
          <div className="portal-content customer-content">
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

  const jobStatus = jobCard?.status || ''
  const isInspected = ['Inspected', 'In Progress', 'Ready for Collection', 'Completed'].includes(jobStatus)
  const isInProgress = ['In Progress', 'Ready for Collection', 'Completed'].includes(jobStatus)
  const isReady = ['Ready for Collection', 'Completed'].includes(jobStatus)
  const isCompleted = jobStatus === 'Completed' || booking.status === 'Completed'

  return (
    <div className="portal-layout customer-portal">
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
            Back to bookings
          </button>
        </header>

        <div className="portal-content customer-content">
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

              <div className="booking-status-stack">
                <span
                  className={`booking-status status-${booking.status.toLowerCase()}`}
                >
                  Booking: {booking.status}
                </span>
                {jobCard && (
                  <span
                    className="booking-status service-status"
                  >
                    Live Job: {jobCard.status}
                  </span>
                )}
              </div>
            </div>

            {jobCard && (
              <section className="customer-live-status">
                <div>
                  <span>LIVE MAINTENANCE STATUS</span>
                  <h3>{jobCard.status}</h3>
                  <p>Job Card #{jobCard.id} · {jobCard.vehicleRegistrationNumber}</p>
                </div>
                <button className="customer-light-button" onClick={() => navigate(`/jobs/${jobCard.id}/status`)}>Track live progress</button>
              </section>
            )}

            <div className="premium-vehicle-banner">
              <div className="vehicle-form-header-icon">
                <Car size={24} />
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
              <h3>Booking & Live Maintenance Progress</h3>

              <div className="premium-timeline-step active">
                <span />
                <div>
                  <strong>Booking Created</strong>
                  <p>Your request has been received.</p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${booking.status !== 'Pending' &&
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
                className={`premium-timeline-step ${[
                    'CheckedIn',
                    'InService',
                    'Completed',
                  ].includes(booking.status) || jobCard
                    ? 'active'
                    : ''
                  }`}
              >
                <span />
                <div>
                  <strong>Vehicle Check-In & Job Card</strong>
                  <p>
                    Vehicle checked in at service center. Job card #{jobCard?.id || 'created'}.
                  </p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${isInspected ? 'active' : ''}`}
              >
                <span />
                <div>
                  <strong>Mechanic Inspection</strong>
                  <p>Vehicle initial inspection complete.</p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${isInProgress ? 'active' : ''}`}
              >
                <span />
                <div>
                  <strong>Servicing In Progress</strong>
                  <p>Mechanic is actively performing repairs/maintenance.</p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${isReady ? 'active' : ''}`}
              >
                <span />
                <div>
                  <strong>Ready for Collection</strong>
                  <p>Service completed and vehicle is ready for pickup.</p>
                </div>
              </div>

              <div
                className={`premium-timeline-step ${isCompleted ? 'active' : ''}`}
              >
                <span />
                <div>
                  <strong>Completed</strong>
                  <p>Vehicle servicing finished and delivered.</p>
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