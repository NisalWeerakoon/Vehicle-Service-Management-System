import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import {
  clearAuth,
  vehicleApi,
} from '../services/api'

function VehiclesPage() {
  const navigate = useNavigate()

  const [vehicles, setVehicles] =
    useState([])

  const [loading, setLoading] =
    useState(true)

  const [error, setError] =
    useState('')

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

  if (loading) {
    return (
      <div className="page-container">
        <div className="profile-card">
          Loading your garage...
        </div>
      </div>
    )
  }

  return (
    <div className="dashboard-page">

      <header className="top-bar">
        <div>
          <h2>
            AutoCare Service Center
          </h2>

          <span>
            Customer Garage
          </span>
        </div>

        <button
          className="secondary-button"
          onClick={() =>
            navigate('/profile')
          }
        >
          👤 My Profile
        </button>
      </header>

      <main className="dashboard-content">

        <div className="page-heading">
          <div>
            <h1>
              🚘 My Garage
            </h1>

            <p>
              View and manage all vehicles
              registered under your account.
            </p>
          </div>

          <button
            className=
              "primary-button small-button"
            onClick={() =>
              navigate('/vehicles/add')
            }
          >
            + Add Vehicle
          </button>
        </div>

        <div className="stats-grid">

          <div className="stat-card">
            <div className="stat-title">
              Registered Vehicles
            </div>

            <div className="stat-value">
              {vehicles.length}
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-title">
              Active Vehicles
            </div>

            <div className="stat-value">
              {vehicles.length}
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-title">
              Service Bookings
            </div>

            <div className="stat-value">
              0
            </div>
          </div>

        </div>

        {error && (
          <div className=
            "alert error-alert">
            {error}
          </div>
        )}

        {vehicles.length === 0 ? (

          <div className="empty-state">

            <div className=
              "empty-state-icon">
              🚗
            </div>

            <h2>
              Your garage is empty
            </h2>

            <p>
              Register your first vehicle
              to start creating service
              bookings.
            </p>

            <button
              className=
                "primary-button small-button"
              onClick={() =>
                navigate('/vehicles/add')
              }
            >
              Register First Vehicle
            </button>

          </div>

        ) : (

          <div className="vehicle-grid">

            {vehicles.map((vehicle) => (

              <div
                className="vehicle-card"
                key={vehicle.id}
              >

                <div
                  className=
                    "vehicle-registration"
                >
                  🚘 {vehicle.registrationNumber}
                </div>

                <h2>
                  {vehicle.make}{' '}
                  {vehicle.model}
                </h2>

                <div className=
                  "vehicle-details">

                  <p>
                    <strong>
                      📅 Year
                    </strong>
                    <br />
                    {vehicle.year}
                  </p>

                  <p>
                    <strong>
                      ⛽ Fuel
                    </strong>
                    <br />
                    {vehicle.fuelType}
                  </p>

                </div>

                <button
                  className=
                    "secondary-button"
                  onClick={() =>
                    navigate(
                      `/vehicles/${vehicle.id}/edit`,
                    )
                  }
                >
                  ✏️ Edit Vehicle
                </button>

              </div>

            ))}

          </div>
        )}

      </main>
    </div>
  )
}

export default VehiclesPage