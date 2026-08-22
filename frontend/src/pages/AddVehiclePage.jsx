import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
    <div className="dashboard-page">
      <header className="top-bar">
        <div>
          <h2>Vehicle Service Center</h2>
          <span>Customer Portal</span>
        </div>
      </header>

      <main className="dashboard-content">
        <div className="edit-card">
          <h1>Add Vehicle</h1>

          <p>
            Register a vehicle under your customer profile.
          </p>

          {error && (
            <div className="alert error-alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="registrationNumber">
                Registration Number
              </label>

              <input
                id="registrationNumber"
                name="registrationNumber"
                value={form.registrationNumber}
                onChange={handleChange}
                required
                maxLength="30"
                placeholder="ABC-1234"
              />
            </div>

            <div className="form-group">
              <label htmlFor="make">Make</label>

              <input
                id="make"
                name="make"
                value={form.make}
                onChange={handleChange}
                required
                maxLength="80"
                placeholder="Toyota"
              />
            </div>

            <div className="form-group">
              <label htmlFor="model">Model</label>

              <input
                id="model"
                name="model"
                value={form.model}
                onChange={handleChange}
                required
                maxLength="80"
                placeholder="Corolla"
              />
            </div>

            <div className="form-group">
              <label htmlFor="year">Year</label>

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

            <div className="form-group">
              <label htmlFor="fuelType">
                Fuel Type
              </label>

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
                <option value="Petrol">Petrol</option>
                <option value="Diesel">Diesel</option>
                <option value="Hybrid">Hybrid</option>
                <option value="Electric">Electric</option>
              </select>
            </div>

            <div className="button-row">
              <button
                type="button"
                className="secondary-button"
                onClick={() => navigate('/vehicles')}
              >
                Cancel
              </button>

              <button
                type="submit"
                className="primary-button"
                disabled={saving}
              >
                {saving
                  ? 'Saving...'
                  : 'Register Vehicle'}
              </button>
            </div>
          </form>
        </div>
      </main>
    </div>
  )
}

export default AddVehiclePage