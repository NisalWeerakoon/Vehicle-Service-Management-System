import { useLocation, useNavigate } from 'react-router-dom'
import {
  User,
  Car,
  CalendarCheck,
  PlusCircle,
  Wrench,
  LogOut,
} from 'lucide-react'
import {
  authApi,
  clearAuth,
  getRole,
  getUserEmail,
} from '../services/api'

function CustomerSidebar() {
  const navigate = useNavigate()
  const location = useLocation()
  const role = getRole()
  const email = getUserEmail ? getUserEmail() : ''

  const isServiceAdvisor =
    role === 'ServiceAdvisor' ||
    role === 'Staff' ||
    role === 'Administrator'

  const isMechanic =
    role === 'Mechanic' ||
    role === 'Administrator'

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

  const initial = email ? email.charAt(0).toUpperCase() : 'C'

  return (
    <aside className="customer-sidebar">
      <div>
        <div className="sidebar-brand">
          <div className="sidebar-brand-icon">
            <Wrench size={20} />
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
              <User size={17} />
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
              <Car size={17} />
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
              <CalendarCheck size={17} />
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
              <PlusCircle size={17} />
            </span>
            Create Booking
          </button>

          {isMechanic && (
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
                <Wrench size={17} />
              </span>
              My Jobs
            </button>
          )}
        </nav>
      </div>

      <div className="sidebar-bottom">
        <div className="sidebar-user-pill">
          <div className="sidebar-user-avatar">
            {initial}
          </div>
          <div className="sidebar-user-info">
            <strong>{role || 'Customer'}</strong>
            <span>{email || 'Logged in'}</span>
          </div>
        </div>

        <button
          className="sidebar-logout"
          onClick={handleLogout}
        >
          <LogOut size={16} />
          Logout
        </button>
      </div>
    </aside>
  )
}

export default CustomerSidebar