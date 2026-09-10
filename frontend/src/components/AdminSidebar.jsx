import { useLocation, useNavigate } from 'react-router-dom'
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
          <div className="sidebar-brand-icon" style={{ background: 'linear-gradient(135deg, #2563eb, #0ea5e9)' }}>
            ⚡
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
              📊
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
              👥
            </span>
            User & Staff Management
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
              📈
            </span>
            Reports & Analytics
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
              🏢
            </span>
            Service Advisor Portal
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

export default AdminSidebar
