import { useLocation, useNavigate } from 'react-router-dom'
import { authApi, clearAuth, getRole } from '../services/api'

function ServiceAdvisorSidebar() {
  const navigate = useNavigate()
  const location = useLocation()
  const role = getRole()

  function isActive(path) {
    if (path === '/service-advisor') {
      return location.pathname === '/service-advisor'
    }
    return location.pathname.startsWith(path)
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
            🏢
          </div>

          <div>
            <strong>SERVICE ADVISOR</strong>
            <span>MANAGEMENT PORTAL</span>
          </div>
        </div>

        <nav className="sidebar-navigation">
          {role === 'Administrator' && (
            <button
              className={
                isActive('/admin')
                  ? 'sidebar-link active'
                  : 'sidebar-link'
              }
              onClick={() => navigate('/admin')}
            >
              <span className="sidebar-link-icon">
                ⚡
              </span>
              Admin Control Panel
            </button>
          )}

          <button
            className={
              isActive('/service-advisor')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/service-advisor')}
          >
            <span className="sidebar-link-icon">
              📊
            </span>
            Dashboard
          </button>

          <button
            className={
              isActive('/service-advisor/check-in')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/service-advisor/check-in')}
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
            onClick={() => navigate('/service-advisor/job-cards')}
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
            onClick={() => navigate('/service-advisor/mechanic-assignments')}
          >
            <span className="sidebar-link-icon">
              👨‍🔧
            </span>
            Assignments
          </button>

          <button
            className={
              isActive('/service-advisor/completed-inspections')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/service-advisor/completed-inspections')}
          >
            <span className="sidebar-link-icon">
              🔍
            </span>
            Completed Inspections
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

export default ServiceAdvisorSidebar
