import { useLocation, useNavigate } from 'react-router-dom'
import {
  authApi,
  clearAuth,
} from '../services/api'

function CustomerSidebar() {
  const navigate = useNavigate()
  const location = useLocation()

  function isActive(path) {
    if (path === '/profile') {
      return location.pathname.startsWith('/profile')
    }

    if (path === '/vehicles') {
      return location.pathname.startsWith('/vehicles')
    }

    if (path === '/bookings') {
      return (
        location.pathname.startsWith('/bookings') &&
        location.pathname !== '/bookings/create'
      )
    }

    return location.pathname === path
  }

  async function handleLogout() {
    try {
      await authApi.logout()
    } catch {
      // JWT logout is stateless.
    }

    clearAuth()
    navigate('/login')
  }

  return (
    <aside className="customer-sidebar">
      <div>
        <div className="sidebar-brand">
          <div className="sidebar-brand-icon">
            ⚙
          </div>

          <div>
            <strong>VEHICLE</strong>
            <span>SERVICE CENTER</span>
          </div>
        </div>

        <nav className="sidebar-navigation">
          <button
            className={
              isActive('/profile')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/profile')}
          >
            <span className="sidebar-link-icon">
              ◉
            </span>

            Profile
          </button>

          <button
            className={
              isActive('/vehicles')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/vehicles')}
          >
            <span className="sidebar-link-icon">
              ◇
            </span>

            Vehicles
          </button>

          <button
            className={
              isActive('/bookings')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/bookings')}
          >
            <span className="sidebar-link-icon">
              ▣
            </span>

            My Bookings
          </button>

          <button
            className={
              isActive('/bookings/create')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() =>
              navigate('/bookings/create')
            }
          >
            <span className="sidebar-link-icon">
              ＋
            </span>

            Create Booking
          </button>

          <button
            className={
              isActive('/service-advisor/check-in')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() =>
              navigate('/service-advisor/check-in')
            }
          >
            <span className="sidebar-link-icon">
              📋
            </span>

            Vehicle Check-In
          </button>

          <button
            className={
              isActive('/service-advisor/job-cards')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() =>
              navigate('/service-advisor/job-cards')
            }
          >
            <span className="sidebar-link-icon">
              📑
            </span>

            Job Cards
          </button>

          <button
            className={
              isActive('/service-advisor/mechanic-assignments')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() =>
              navigate('/service-advisor/mechanic-assignments')
            }
          >
            <span className="sidebar-link-icon">
              👨‍🔧
            </span>

            Assignments
          </button>

          <button
            className={
              isActive('/mechanic/my-jobs')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() =>
              navigate('/mechanic/my-jobs')
            }
          >
            <span className="sidebar-link-icon">
              🔧
            </span>

            My Jobs
          </button>
        </nav>
      </div>

      <button
        className="sidebar-logout"
        onClick={handleLogout}
      >
        <span>↪</span>
        Logout
      </button>
    </aside>
  )
}

export default CustomerSidebar