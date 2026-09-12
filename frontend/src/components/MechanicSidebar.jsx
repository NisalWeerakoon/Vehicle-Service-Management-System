import { useLocation, useNavigate } from 'react-router-dom'
import { authApi, clearAuth } from '../services/api'

function MechanicSidebar() {
  const navigate = useNavigate()
  const location = useLocation()

  function isActive(path) {
    if (path === '/mechanic') {
      return location.pathname === '/mechanic'
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
            🔧
          </div>

          <div>
            <strong>MECHANIC</strong>
            <span>MAINTENANCE PORTAL</span>
          </div>
        </div>

        <nav className="sidebar-navigation">
          <button
            className={
              isActive('/mechanic')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/mechanic')}
          >
            <span className="sidebar-link-icon">
              📊
            </span>
            Dashboard
          </button>

          <button
            className={
              isActive('/mechanic/my-jobs')
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/mechanic/my-jobs')}
          >
            <span className="sidebar-link-icon">
              🛠️
            </span>
            My Assigned Jobs
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

export default MechanicSidebar
