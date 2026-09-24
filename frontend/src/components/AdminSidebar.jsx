import { useLocation, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard,
  Users,
  BarChart3,
  ClipboardList,
  Building2,
  LogOut,
  Zap,
} from 'lucide-react'
import { authApi, clearAuth } from '../services/api'

function AdminSidebar({ activeTab, setActiveTab }) {
  const navigate = useNavigate()
  const location = useLocation()

  async function handleLogout() {
    try {
      await authApi.logout()
    } catch {
      // JWT logout is stateless.
    }
    clearAuth()
    navigate('/login')
  }

  const handleTabClick = (tab) => {
    if (location.pathname !== '/admin') {
      navigate('/admin')
    }
    if (setActiveTab) {
      setActiveTab(tab)
    }
  }

  return (
    <aside className="customer-sidebar">
      <div>
        <div className="sidebar-brand">
          <div className="sidebar-brand-icon">
            <Zap size={20} />
          </div>

          <div>
            <strong>ADMINISTRATOR</strong>
            <span>CONTROL CENTER</span>
          </div>
        </div>

        <nav className="sidebar-navigation">
          <button
            className={
              activeTab === 'dashboard'
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => handleTabClick('dashboard')}
          >
            <span className="sidebar-link-icon">
              <LayoutDashboard size={17} />
            </span>
            Dashboard
          </button>

          <button
            className={
              activeTab === 'users'
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => handleTabClick('users')}
          >
            <span className="sidebar-link-icon">
              <Users size={17} />
            </span>
            User &amp; Staff Management
          </button>

          <button
            className={
              activeTab === 'reports'
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => handleTabClick('reports')}
          >
            <span className="sidebar-link-icon">
              <BarChart3 size={17} />
            </span>
            Reports &amp; Analytics
          </button>

          <button
            className={
              location.pathname === '/reports/active-jobs'
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/reports/active-jobs')}
          >
            <span className="sidebar-link-icon">
              <ClipboardList size={17} />
            </span>
            Active Jobs Report
          </button>

          <button
            className={
              location.pathname === '/service-advisor'
                ? 'sidebar-link active'
                : 'sidebar-link'
            }
            onClick={() => navigate('/service-advisor')}
          >
            <span className="sidebar-link-icon">
              <Building2 size={17} />
            </span>
            Service Advisor Portal
          </button>
        </nav>
      </div>

      <div className="sidebar-bottom">
        <div className="sidebar-user-pill">
          <div className="sidebar-user-avatar">A</div>
          <div className="sidebar-user-info">
            <strong>Administrator</strong>
            <span>Admin Portal</span>
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

export default AdminSidebar
