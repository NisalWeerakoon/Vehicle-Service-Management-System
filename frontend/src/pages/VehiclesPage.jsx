import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import CustomerSidebar from '../components/CustomerSidebar'

import {
  clearAuth,
  vehicleApi,
} from '../services/api'

function VehiclesPage() {
  const navigate = useNavigate()

  const [vehicles, setVehicles] = useState([])
  const [loading, setLoading] = useState(true)
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

            <h1>My Vehicles</h1>
          </div>

          <button
            className="portal-primary-button"
            onClick={() => navigate('/vehicles/add')}
          >
            + Add Vehicle
          </button>
        </header>

        <div className="portal-content">
          <section className="vehicles-hero">
            <div>
              <span className="profile-welcome-label">
                GARAGE OVERVIEW
              </span>

              <h2>Your registered vehicles</h2>

              <p>
                Manage the vehicles connected to your account
                and keep their information up to date.
              </p>
            </div>

            <button
              className="portal-secondary-button"
              onClick={() => navigate('/bookings')}
            >
              View Service Bookings
            </button>
          </section>

          <section className="vehicle-stats">
            <div className="vehicle-stat-card">
              <span>Total Vehicles</span>
              <strong>{vehicles.length}</strong>
              <p>Registered under your account</p>
            </div>

            <div className="vehicle-stat-card">
              <span>Active Vehicles</span>
              <strong>{vehicles.length}</strong>
              <p>Available for service booking</p>
            </div>

            <div className="vehicle-stat-card">
              <span>Service Access</span>
              <strong>Ready</strong>
              <p>Create a booking anytime</p>
            </div>
          </section>

          {error && (
            <div className="portal-error">
              <span>!</span>
              {error}
            </div>
          )}

          {vehicles.length === 0 ? (
            <section className="modern-empty-state">
              <div className="modern-empty-icon">
                🚘
              </div>

              <span>NO VEHICLES REGISTERED</span>

              <h2>Your garage is empty</h2>

              <p>
                Register your first vehicle to start creating
                service bookings and managing maintenance.
              </p>

              <button
                className="portal-primary-button"
                onClick={() => navigate('/vehicles/add')}
              >
                Register First Vehicle
              </button>
            </section>
          ) : (
            <section className="modern-vehicle-grid">
              {vehicles.map((vehicle) => (
                <article
                  className="modern-vehicle-card"
                  key={vehicle.id}
                >
                  <div className="vehicle-card-header">
                    <div className="vehicle-card-icon">
                      🚘
                    </div>

                    <span className="vehicle-status-badge">
                      ● Active
                    </span>
                  </div>

                  <div className="vehicle-card-body">
                    <span className="vehicle-number">
                      {vehicle.registrationNumber}
                    </span>

                    <h2>
                      {vehicle.make} {vehicle.model}
                    </h2>

                    <div className="vehicle-card-details">
                      <div>
                        <span>Year</span>
                        <strong>{vehicle.year}</strong>
                      </div>

                      <div>
                        <span>Fuel Type</span>
                        <strong>{vehicle.fuelType}</strong>
                      </div>
                    </div>
                  </div>

                  <div className="vehicle-card-footer">
                    <button
                      className="vehicle-outline-button"
                      onClick={() =>
                        navigate(
                          `/vehicles/${vehicle.id}/edit`,
                        )
                      }
                    >
                      Edit Vehicle
                    </button>

                    <button
                      className="vehicle-dark-button"
                      onClick={() =>
                        navigate('/bookings/create')
                      }
                    >
                      Book Service
                    </button>
                  </div>
                </article>
              ))}
            </section>
          )}
        </div>
      </main>
    </div>
  )
}

export default VehiclesPage