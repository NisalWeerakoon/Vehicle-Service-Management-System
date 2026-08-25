import { useEffect, useState } from 'react'
import {
  useNavigate,
  useParams,
} from 'react-router-dom'

import {
  bookingApi,
  clearAuth,
} from '../services/api'

function BookingDetailsPage() {
  const navigate = useNavigate()
  const { id } = useParams()

  const [booking, setBooking] = useState(null)
  const [loading, setLoading] = useState(true)
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

  if (loading) {
    return (
      <div className="page-container">
        <div className="profile-card">
          Loading booking...
        </div>
      </div>
    )
  }

  if (!booking) {
    return (
      <div className="page-container">
        <div className="profile-card">
          <h2>Booking unavailable</h2>

          <p>{error}</p>

          <button
            className="primary-button"
            onClick={() => navigate('/bookings')}
          >
            Back to My Bookings
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="dashboard-page">
      <header className="top-bar">
        <div>
          <h2>AutoCare Service Center</h2>
          <span>Booking Details</span>
        </div>

        <button
          className="secondary-button"
          onClick={() => navigate('/bookings')}
        >
          ← My Bookings
        </button>
      </header>

      <main className="dashboard-content">
        <div className="booking-details-card">
          <div className="booking-details-header">
            <div>
              <span className="booking-reference">
                {booking.bookingReference}
              </span>

              <h1>Service Booking</h1>

              <p>
                Created on{' '}
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

          <div className="booking-vehicle-banner">
            <div className="booking-vehicle-icon">
              🚘
            </div>

            <div>
              <span>Vehicle</span>

              <h2>
                {booking.vehicleName}
              </h2>

              <p>
                {booking.vehicleRegistrationNumber}
              </p>
            </div>
          </div>

          <div className="booking-details-grid">
            <div className="booking-detail-box">
              <span>Preferred Service Date</span>

              <strong>
                {new Date(
                  booking.preferredDate,
                ).toLocaleDateString()}
              </strong>
            </div>

            <div className="booking-detail-box">
              <span>Booking Status</span>

              <strong>
                {booking.status}
              </strong>
            </div>

            <div className="booking-detail-box">
              <span>Booking ID</span>

              <strong>
                #{booking.id}
              </strong>
            </div>

            <div className="booking-detail-box">
              <span>Vehicle ID</span>

              <strong>
                #{booking.vehicleId}
              </strong>
            </div>
          </div>

          <div className="booking-request-box">
            <span>
              Requested Service / Problem
            </span>

            <p>
              {booking.requestedServiceOrProblem}
            </p>
          </div>

          <div className="booking-timeline">
            <h3>Booking Progress</h3>

            <div className="timeline-item active">
              <div className="timeline-dot"></div>

              <div>
                <strong>Booking Created</strong>
                <p>
                  Your service request has been received.
                </p>
              </div>
            </div>

            <div
              className={`timeline-item ${
                booking.status !== 'Pending'
                  ? 'active'
                  : ''
              }`}
            >
              <div className="timeline-dot"></div>

              <div>
                <strong>Booking Confirmed</strong>
                <p>
                  Service center confirms your appointment.
                </p>
              </div>
            </div>

            <div className="timeline-item">
              <div className="timeline-dot"></div>

              <div>
                <strong>Vehicle Check-In</strong>

                <p>
                  Vehicle arrives at the service center.
                </p>
              </div>
            </div>

            <div className="timeline-item">
              <div className="timeline-dot"></div>

              <div>
                <strong>Service Completed</strong>

                <p>
                  Vehicle servicing is completed.
                </p>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}

export default BookingDetailsPage