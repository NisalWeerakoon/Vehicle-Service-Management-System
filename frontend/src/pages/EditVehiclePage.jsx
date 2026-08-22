import { useEffect, useState } from 'react'
import {
  useNavigate,
  useParams,
} from 'react-router-dom'

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
      setError(err.message)
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="page-container">
        <div className="profile-card">
          Loading vehicle...
        </div>
      </div>
    )
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
          <h1>Edit Vehicle</h1>

          {error && (
            <div className="alert error-alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Registration Number</label>

              <input
                value={registrationNumber}
                disabled
              />

              <small>
                Registration number cannot be changed.
              </small>
            </div>

            <div className="form-group">
              <label htmlFor="make">
                Make
              </label>

              <input
                id="make"
                name="make"
                value={form.make}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="model">
                Model
              </label>

              <input
                id="model"
                name="model"
                value={form.model}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="year">
                Year
              </label>

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
                  : 'Save Changes'}
              </button>
            </div>
          </form>
        </div>
      </main>
    </div>
  )
}

export default EditVehiclePage