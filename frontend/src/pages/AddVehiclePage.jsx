import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  clearAuth,
  vehicleApi,
} from '../services/api'

function AddVehiclePage() {
  const navigate = useNavigate()

  const [form, setForm] = useState({
    registrationNumber: '',
    make: '',
    model: '',
    year: '',
    fuelType: '',
  })

  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

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
      await vehicleApi.createMyVehicle({
        registrationNumber: form.registrationNumber,
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

  return (
    <div className="portal-layout">
      <CustomerSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">
              CUSTOMER PORTAL
            </span>
            <h1>Add Vehicle</h1>
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
                VEHICLE REGISTRATION
              </span>

              <h2>Add a vehicle to your garage</h2>

              <p>
                Enter your vehicle details below. Once
                registered, you can create service bookings
                for this vehicle.
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
                <span>NEW VEHICLE</span>
                <h2>Vehicle Information</h2>
                <p>
                  Provide the basic details of your vehicle.
                </p>
              </div>
            </div>

            <form
              className="vehicle-modern-form"
              onSubmit={handleSubmit}
            >
              <div className="modern-form-grid">
                <div className="modern-form-group modern-form-wide">
                  <label htmlFor="registrationNumber">
                    Registration Number
                  </label>

                  <div className="modern-input-wrapper">
                    <span>▣</span>

                    <input
                      id="registrationNumber"
                      name="registrationNumber"
                      value={form.registrationNumber}
                      onChange={handleChange}
                      required
                      maxLength="30"
                      placeholder="Example: ABC-1234"
                    />
                  </div>

                  <small>
                    Enter the registration number exactly as
                    shown on the vehicle.
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
                      placeholder="Example: Toyota"
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
                      placeholder="Example: Corolla"
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
                      placeholder="2024"
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
                      <option value="">
                        Select fuel type
                      </option>
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
                  <strong>Register your vehicle</strong>
                  <p>
                    You can edit these vehicle details later.
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
                      ? 'Registering...'
                      : '+ Register Vehicle'}
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

export default AddVehiclePage