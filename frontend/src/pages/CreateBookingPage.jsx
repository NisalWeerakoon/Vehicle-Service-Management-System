import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

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
        const data = await vehicleApi.getMyVehicles()
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
      <div className="page-container">
        <div className="profile-card">
          Loading your vehicles...
        </div>
      </div>
    )
  }

  return (
    <div className="dashboard-page">
      <header className="top-bar">
        <div>
          <h2>AutoCare Service Center</h2>
          <span>Book a Service</span>
        </div>

        <button
          className="secondary-button"
          onClick={() => navigate('/bookings')}
        >
          ← My Bookings
        </button>
      </header>

      <main className="dashboard-content">
        <div className="booking-form-layout">
          <div className="booking-form-intro">
            <div className="booking-icon">
              🔧
            </div>

            <h1>Schedule a Service</h1>

            <p>
              Choose your vehicle, preferred date and
              describe the service or problem.
            </p>

            <div className="booking-step">
              <span>1</span>
              Select your vehicle
            </div>

            <div className="booking-step">
              <span>2</span>
              Choose preferred date
            </div>

            <div className="booking-step">
              <span>3</span>
              Describe required work
            </div>

            <div className="booking-step">
              <span>4</span>
              Submit booking
            </div>
          </div>

          <div className="edit-card">
            <h2>Create Service Booking</h2>

            <p>
              Your booking will initially be marked as
              <strong> Pending</strong>.
            </p>

            {error && (
              <div className="alert error-alert">
                {error}
              </div>
            )}

            {vehicles.length === 0 ? (
              <div className="empty-state">
                <div className="empty-state-icon">
                  🚗
                </div>

                <h3>No vehicle available</h3>

                <p>
                  Register a vehicle before creating a
                  service booking.
                </p>

                <button
                  className="primary-button"
                  onClick={() =>
                    navigate('/vehicles/add')
                  }
                >
                  Register Vehicle
                </button>
              </div>
            ) : (
              <form onSubmit={handleSubmit}>
                <div className="form-group">
                  <label htmlFor="vehicleId">
                    Select Vehicle
                  </label>

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
                        value={vehicle.id}
                        key={vehicle.id}
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
                  <label htmlFor="requestedServiceOrProblem">
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
                    placeholder="Example: Engine oil change, brake inspection, unusual engine noise..."
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
                      navigate('/bookings')
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
                      ? 'Creating Booking...'
                      : 'Confirm Booking'}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      </main>
    </div>
  )
}

export default CreateBookingPage