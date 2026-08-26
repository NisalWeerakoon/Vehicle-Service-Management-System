import { useEffect, useState } from 'react'
import {
  useNavigate,
  useParams,
} from 'react-router-dom'

import {
  bookingApi,
  clearAuth,
} from '../services/api'

function EditBookingPage() {
  const navigate = useNavigate()
  const { id } = useParams()

  const [form, setForm] = useState({
    preferredDate: '',
    requestedServiceOrProblem: '',
  })

  const [booking, setBooking] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadBooking() {
      try {
        const data =
          await bookingApi.getMyBooking(id)

        setBooking(data)

        setForm({
          preferredDate:
            data.preferredDate.split('T')[0],

          requestedServiceOrProblem:
            data.requestedServiceOrProblem,
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

    loadBooking()
  }, [id, navigate])

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

    try {
      await bookingApi.updateMyBooking(id, {
        preferredDate:
          new Date(
            `${form.preferredDate}T09:00:00`,
          ).toISOString(),

        requestedServiceOrProblem:
          form.requestedServiceOrProblem,
      })

      navigate(`/bookings/${id}`)
    } catch (err) {
      if (err.status === 401) {
        clearAuth()
        navigate('/login')
        return
      }

      setError(err.message)
    } finally {
      setSaving(false)
    }
  }

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
            onClick={() =>
              navigate('/bookings')
            }
          >
            Back to My Bookings
          </button>
        </div>
      </div>
    )
  }

  const canEdit =
    booking.status === 'Pending' ||
    booking.status === 'Confirmed'

  if (!canEdit) {
    return (
      <div className="page-container">
        <div className="profile-card">
          <h2>Booking cannot be edited</h2>

          <p>
            This booking can no longer be edited
            because vehicle check-in or servicing has
            already started.
          </p>

          <button
            className="primary-button"
            onClick={() =>
              navigate(`/bookings/${id}`)
            }
          >
            Back to Booking
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
          <span>Edit Service Booking</span>
        </div>

        <button
          className="secondary-button"
          onClick={() =>
            navigate(`/bookings/${id}`)
          }
        >
          ← Booking Details
        </button>
      </header>

      <main className="dashboard-content">
        <div className="booking-form-layout">

          <div className="booking-form-intro">
            <div className="booking-icon">
              🛠️
            </div>

            <h1>Edit Booking</h1>

            <p>
              Update your preferred service date or
              requested service information.
            </p>

            <div className="booking-step">
              <span>✓</span>
              Booking {booking.bookingReference}
            </div>

            <div className="booking-step">
              <span>✓</span>
              {booking.vehicleName}
            </div>

            <div className="booking-step">
              <span>✓</span>
              {booking.vehicleRegistrationNumber}
            </div>

            <div className="booking-step">
              <span>✓</span>
              Status: {booking.status}
            </div>
          </div>

          <div className="edit-card">
            <h2>Update Service Request</h2>

            <p>
              Changes are allowed while the booking is
              Pending or Confirmed.
            </p>

            {error && (
              <div className="alert error-alert">
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit}>

              <div className="form-group">
                <label htmlFor="preferredDate">
                  Preferred Service Date
                </label>

                <input
                  id="preferredDate"
                  name="preferredDate"
                  type="date"
                  value={form.preferredDate}
                  onChange={handleChange}
                  min={
                    new Date()
                      .toISOString()
                      .split('T')[0]
                  }
                  required
                />
              </div>

              <div className="form-group">
                <label
                  htmlFor="requestedServiceOrProblem"
                >
                  Requested Service / Problem
                </label>

                <textarea
                  id="requestedServiceOrProblem"
                  name="requestedServiceOrProblem"
                  rows="6"
                  maxLength="500"
                  value={
                    form.requestedServiceOrProblem
                  }
                  onChange={handleChange}
                  required
                />

                <small>
                  {
                    form
                      .requestedServiceOrProblem
                      .length
                  }
                  /500 characters
                </small>
              </div>

              <div className="button-row">

                <button
                  type="button"
                  className="secondary-button"
                  onClick={() =>
                    navigate(`/bookings/${id}`)
                  }
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="primary-button"
                  disabled={saving}
                >
                  {saving
                    ? 'Saving Changes...'
                    : 'Save Changes'}
                </button>

              </div>
            </form>
          </div>
        </div>
      </main>
    </div>
  )
}

export default EditBookingPage