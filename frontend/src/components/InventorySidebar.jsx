import { AlertTriangle, Boxes, ClipboardList, LogOut, PackageCheck } from 'lucide-react'
import { useLocation, useNavigate } from 'react-router-dom'
import { authApi, clearAuth, getRole } from '../services/api'

function InventorySidebar() {
  const navigate = useNavigate()
  const location = useLocation()
  const role = getRole()

  async function handleLogout() {
    try { await authApi.logout() } catch { /* JWT logout is stateless. */ }
    clearAuth()
    navigate('/login')
  }

  return (
    <aside className="customer-sidebar">
      <div>
        <div className="sidebar-brand">
          <div className="sidebar-brand-icon"><Boxes size={20} /></div>
          <div><strong>INVENTORY</strong><span>MANAGEMENT PORTAL</span></div>
        </div>
        <nav className="sidebar-navigation">
          <button className={location.pathname === '/inventory/dashboard' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/inventory/dashboard')}>
            <span className="sidebar-link-icon"><Boxes size={17} /></span>
            Dashboard
          </button>
          <button className={location.pathname === '/inventory/spare-parts' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/inventory/spare-parts')}>
            <span className="sidebar-link-icon"><ClipboardList size={17} /></span>
            Spare Parts
          </button>
          <button className={location.pathname === '/inventory/part-requests' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/inventory/part-requests')}>
            <span className="sidebar-link-icon"><PackageCheck size={17} /></span>
            Part Requests
          </button>
          <button className={location.pathname === '/inventory/stock-report' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/inventory/stock-report')}>
            <span className="sidebar-link-icon"><Boxes size={17} /></span>
            Current Stock
          </button>
          <button className={location.pathname === '/inventory/low-stock-report' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/inventory/low-stock-report')}>
            <span className="sidebar-link-icon"><AlertTriangle size={17} /></span>
            Low Stock
          </button>
          {role === 'Administrator' && (
            <button className="sidebar-link" onClick={() => navigate('/admin')}>Administrator Portal</button>
          )}
        </nav>
      </div>
      <div className="sidebar-bottom">
        <div className="sidebar-user-pill">
          <div className="sidebar-user-avatar">{role === 'Administrator' ? 'A' : 'I'}</div>
          <div className="sidebar-user-info"><strong>{role || 'Inventory Officer'}</strong><span>Staff Portal</span></div>
        </div>
        <button className="sidebar-logout" onClick={handleLogout}><LogOut size={16} />Logout</button>
      </div>
    </aside>
  )
}

export default InventorySidebar
