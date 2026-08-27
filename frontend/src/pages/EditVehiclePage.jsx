import { useEffect, useState } from 'react'
import {
  useNavigate,
  useParams,
} from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  clearAuth,
  vehicleApi,
} from '../services/api'

function EditVehiclePage() {
  const navigate = useNavigate()
  const { id } = useParams()

  const [registrationNumber, setRegistrationNumber] =
    useState('')

  const [form, setForm] = useState({
    make: '',
    model: '',
    year: '',
    fuelType: '',
  })

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    async function fetchVehicle() {
      try {
        const vehicle =
          await vehicleApi.getMyVehicle(id)

        setRegistrationNumber(
          vehicle.registrationNumber,
        )

        setForm({
          make: vehicle.make,
          model: vehicle.model,
          year: vehicle.year,
          fuelType: vehicle.fuelType,
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

    fetchVehicle()
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
      await vehicleApi.updateMyVehicle(id, {
        make: form.make,
        model: form.model,
        year: Number(form.year),
        fuelType: form.fuelType,
      })

      navigate('/vehicles')
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
            <p>Loading vehicle...</p>
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
            <h1>Edit Vehicle</h1>
          </div>

          <button
            className="portal-back-button"
            onClick={() => navigate('/vehicles')}
          >
            ← My Vehicles
          </button>
        </header>

        <div className="portal-content">
          <section className="vehicle-form-heading">
            <div>
              <span className="profile-welcome-label">
                VEHICLE SETTINGS
              </span>

              <h2>Update vehicle information</h2>

              <p>
                Keep your vehicle information accurate for
                future service appointments.
              </p>
            </div>
          </section>

          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          <section className="vehicle-form-card">
            <div className="vehicle-form-card-header">
              <div className="vehicle-form-header-icon">
                🚘
              </div>

              <div>
                <span>REGISTERED VEHICLE</span>

                <h2>
                  {form.make} {form.model}
                </h2>

                <p>{registrationNumber}</p>
              </div>
            </div>

            <form
              className="vehicle-modern-form"
              onSubmit={handleSubmit}
            >
              <div className="modern-form-grid">
                <div className="modern-form-group modern-form-wide">
                  <label>
                    Registration Number
                  </label>

                  <div className="modern-input-wrapper disabled-input">
                    <span>▣</span>

                    <input
                      value={registrationNumber}
                      disabled
                    />
                  </div>

                  <small>
                    Registration number cannot be changed
                    after the vehicle is registered.
                  </small>
                </div>

                <div className="modern-form-group">
                  <label htmlFor="make">
                    Vehicle Make
                  </label>

                  <div className="modern-input-wrapper">
                    <span>◆</span>

                    <input
                      id="make"
                      name="make"
                      value={form.make}
                      onChange={handleChange}
                      required
                      maxLength="80"
                    />
                  </div>
                </div>

                <div className="modern-form-group">
                  <label htmlFor="model">
                    Vehicle Model
                  </label>

                  <div className="modern-input-wrapper">
                    <span>◇</span>

                    <input
                      id="model"
                      name="model"
                      value={form.model}
                      onChange={handleChange}
                      required
                      maxLength="80"
                    />
                  </div>
                </div>

                <div className="modern-form-group">
                  <label htmlFor="year">
                    Manufacturing Year
                  </label>

                  <div className="modern-input-wrapper">
                    <span>◷</span>

                    <input
                      id="year"
                      name="year"
                      type="number"
                      value={form.year}
                      onChange={handleChange}
                      min="1900"
                      max={new Date().getFullYear() + 1}
                      required
                    />
                  </div>
                </div>

                <div className="modern-form-group">
                  <label htmlFor="fuelType">
                    Fuel Type
                  </label>

                  <div className="modern-select-wrapper">
                    <span>◆</span>

                    <select
                      id="fuelType"
                      name="fuelType"
                      value={form.fuelType}
                      onChange={handleChange}
                      required
                    >
                      <option value="Petrol">
                        Petrol
                      </option>
                      <option value="Diesel">
                        Diesel
                      </option>
                      <option value="Hybrid">
                        Hybrid
                      </option>
                      <option value="Electric">
                        Electric
                      </option>
                    </select>
                  </div>
                </div>
              </div>

              <div className="modern-form-footer">
                <div>
                  <strong>Save vehicle changes</strong>

                  <p>
                    Your updated information will be used for
                    future service bookings.
                  </p>
                </div>

                <div className="modern-form-actions">
                  <button
                    type="button"
                    className="portal-secondary-button"
                    onClick={() =>
                      navigate('/vehicles')
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
          </section>
        </div>
      </main>
    </div>
  )
}

export default EditVehiclePage