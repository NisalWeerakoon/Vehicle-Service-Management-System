import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  bookingApi,
  clearAuth,
  vehicleApi,
} from '../services/api'

function CreateBookingPage() {
  const navigate = useNavigate()

  const [vehicles, setVehicles] = useState([])

  const [form, setForm] = useState({
    vehicleId: '',
    preferredDate: '',
    requestedServiceOrProblem: '',
  })

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    async function fetchVehicles() {
      try {
        const data =
          await vehicleApi.getMyVehicles()

        setVehicles(data)
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

    fetchVehicles()
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

    try {
      const booking =
        await bookingApi.createMyBooking({
          vehicleId: Number(form.vehicleId),

          preferredDate:
            new Date(
              `${form.preferredDate}T09:00:00`,
            ).toISOString(),

          requestedServiceOrProblem:
            form.requestedServiceOrProblem,
        })

      navigate(`/bookings/${booking.id}`)
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
            <p>Loading your vehicles...</p>
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

            <h1>Create Booking</h1>
          </div>

          <button
            className="portal-back-button"
            onClick={() => navigate('/bookings')}
          >
            ← My Bookings
          </button>
        </header>

        <div className="portal-content">
          <section className="booking-page-heading">
            <div>
              <span className="profile-welcome-label">
                BOOK A SERVICE
              </span>

              <h2>Schedule your next service</h2>

              <p>
                Select a vehicle, choose your preferred date
                and describe the required service.
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
              <span>BOOKING PROCESS</span>

              <h2>Simple and convenient</h2>

              <p>
                Complete the details and your booking will
                initially be marked as Pending.
              </p>

              <div className="booking-guide-step">
                <strong>01</strong>
                <div>
                  <h3>Select Vehicle</h3>
                  <p>Choose one of your vehicles.</p>
                </div>
              </div>

              <div className="booking-guide-step">
                <strong>02</strong>
                <div>
                  <h3>Choose Date</h3>
                  <p>Select your preferred service date.</p>
                </div>
              </div>

              <div className="booking-guide-step">
                <strong>03</strong>
                <div>
                  <h3>Describe Service</h3>
                  <p>Tell us what your vehicle needs.</p>
                </div>
              </div>
            </aside>

            <div className="vehicle-form-card">
              <div className="vehicle-form-card-header">
                <div className="vehicle-form-header-icon">
                  🛠️
                </div>

                <div>
                  <span>NEW SERVICE REQUEST</span>
                  <h2>Booking Information</h2>
                  <p>
                    Fill in the details for your service
                    appointment.
                  </p>
                </div>
              </div>

              {vehicles.length === 0 ? (
                <div className="booking-no-vehicle">
                  <div className="modern-empty-icon">
                    🚘
                  </div>

                  <h3>No vehicle available</h3>

                  <p>
                    Register a vehicle before creating a
                    service booking.
                  </p>

                  <button
                    className="portal-primary-button"
                    onClick={() =>
                      navigate('/vehicles/add')
                    }
                  >
                    Register Vehicle
                  </button>
                </div>
              ) : (
                <form
                  className="vehicle-modern-form"
                  onSubmit={handleSubmit}
                >
                  <div className="modern-form-grid">
                    <div className="modern-form-group modern-form-wide">
                      <label htmlFor="vehicleId">
                        Select Vehicle
                      </label>

                      <div className="modern-select-wrapper">
                        <span>🚘</span>

                        <select
                          id="vehicleId"
                          name="vehicleId"
                          value={form.vehicleId}
                          onChange={handleChange}
                          required
                        >
                          <option value="">
                            Choose your vehicle
                          </option>

                          {vehicles.map((vehicle) => (
                            <option
                              key={vehicle.id}
                              value={vehicle.id}
                            >
                              {vehicle.registrationNumber}
                              {' - '}
                              {vehicle.make}
                              {' '}
                              {vehicle.model}
                            </option>
                          ))}
                        </select>
                      </div>
                    </div>

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
                          placeholder="Example: Engine oil change, brake inspection, unusual engine noise..."
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
                      <strong>Ready to book?</strong>
                      <p>
                        Your request will start with Pending
                        status.
                      </p>
                    </div>

                    <div className="modern-form-actions">
                      <button
                        type="button"
                        className="portal-secondary-button"
                        onClick={() =>
                          navigate('/bookings')
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
                          ? 'Creating...'
                          : 'Confirm Booking'}
                      </button>
                    </div>
                  </div>
                </form>
              )}
            </div>
          </section>
        </div>
      </main>
    </div>
  )
}

export default CreateBookingPage