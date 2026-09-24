import { useLocation, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard,
  Wrench,
  ClipboardList,
  LogOut,
} from 'lucide-react'
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
            <Wrench size={20} />
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
              <LayoutDashboard size={17} />
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
              <ClipboardList size={17} />
            </span>
            My Assigned Jobs
          </button>
        </nav>
      </div>

      <div className="sidebar-bottom">
        <div className="sidebar-user-pill">
          <div className="sidebar-user-avatar">M</div>
          <div className="sidebar-user-info">
            <strong>Mechanic</strong>
            <span>Maintenance Portal</span>
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

export default MechanicSidebar
