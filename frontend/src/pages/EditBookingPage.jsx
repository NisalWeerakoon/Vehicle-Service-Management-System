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

  if (!canEdit) {
    return (
      <div className="portal-layout">
        <CustomerSidebar />

        <main className="portal-main">
          <div className="portal-content">
            <section className="modern-empty-state">
              <h2>Booking cannot be edited</h2>

              <p>
                Vehicle check-in or servicing has already
                started.
              </p>

              <button
                className="portal-primary-button"
                onClick={() =>
                  navigate(`/bookings/${id}`)
                }
              >
                Back to Booking
              </button>
            </section>
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

            <h1>Edit Booking</h1>
          </div>

          <button
            className="portal-back-button"
            onClick={() =>
              navigate(`/bookings/${id}`)
            }
          >
            ← Booking Details
          </button>
        </header>

        <div className="portal-content">
          <section className="booking-page-heading">
            <div>
              <span className="profile-welcome-label">
                UPDATE SERVICE REQUEST
              </span>

              <h2>Edit your booking</h2>

              <p>
                Update your preferred date or service
                information while the booking is eligible.
              </p>
            </div>
          </section>

          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          <section className="booking-create-layout">
            <aside className="booking-guide-card">
              <span>BOOKING DETAILS</span>

              <h2>{booking.bookingReference}</h2>

              <p>
                Changes are allowed while the booking is
                Pending or Confirmed.
              </p>

              <div className="booking-guide-step">
                <strong>✓</strong>
                <div>
                  <h3>{booking.vehicleName}</h3>
                  <p>
                    {booking.vehicleRegistrationNumber}
                  </p>
                </div>
              </div>

              <div className="booking-guide-step">
                <strong>✓</strong>
                <div>
                  <h3>Status</h3>
                  <p>{booking.status}</p>
                </div>
              </div>
            </aside>

            <div className="vehicle-form-card">
              <div className="vehicle-form-card-header">
                <div className="vehicle-form-header-icon">
                  ✎
                </div>

                <div>
                  <span>EDIT BOOKING</span>
                  <h2>Service Information</h2>
                  <p>
                    Update the details of your service request.
                  </p>
                </div>
              </div>

              <form
                className="vehicle-modern-form"
                onSubmit={handleSubmit}
              >
                <div className="modern-form-grid">
                  <div className="modern-form-group modern-form-wide">
                    <label htmlFor="preferredDate">
                      Preferred Service Date
                    </label>

                    <div className="modern-input-wrapper">
                      <span>◷</span>

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
                  </div>

                  <div className="modern-form-group modern-form-wide">
                    <label htmlFor="requestedServiceOrProblem">
                      Requested Service / Problem
                    </label>

                    <div className="modern-textarea-wrapper">
                      <span>✎</span>

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
                    </div>

                    <small className="character-count">
                      {
                        form
                          .requestedServiceOrProblem
                          .length
                      }
                      /500 characters
                    </small>
                  </div>
                </div>

                <div className="modern-form-footer">
                  <div>
                    <strong>Save booking changes</strong>
                    <p>
                      Review the updated details before
                      saving.
                    </p>
                  </div>

                  <div className="modern-form-actions">
                    <button
                      type="button"
                      className="portal-secondary-button"
                      onClick={() =>
                        navigate(`/bookings/${id}`)
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
            </div>
          </section>
        </div>
      </main>
    </div>
  )
}

export default EditBookingPage