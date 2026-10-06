import { BarChart3, CreditCard, FileText, LogOut } from 'lucide-react'
import { useLocation, useNavigate } from 'react-router-dom'
import { authApi, clearAuth, getRole } from '../services/api'

function AccountsSidebar() {
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
          <div className="sidebar-brand-icon"><CreditCard size={18} /></div>
          <div><strong>ACCOUNTS</strong><span>BILLING OPERATIONS</span></div>
        </div>
        <nav className="sidebar-navigation">
          <button className={location.pathname === '/billing/invoices' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/billing/invoices')}>
            <span className="sidebar-link-icon"><FileText size={17} /></span>Invoices &amp; payments
          </button>
          <button className={location.pathname === '/billing/part-charges' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/billing/part-charges')}>
            <span className="sidebar-link-icon"><CreditCard size={17} /></span>Invoice charges
          </button>
          <button className={location.pathname === '/billing/reports/invoice-payments' ? 'sidebar-link active' : 'sidebar-link'} onClick={() => navigate('/billing/reports/invoice-payments')}>
            <span className="sidebar-link-icon"><BarChart3 size={17} /></span>Invoice &amp; Payment Report
          </button>
          {role === 'Administrator' && <button className="sidebar-link" onClick={() => navigate('/admin')}>Administrator portal</button>}
        </nav>
      </div>
      <div className="sidebar-bottom">
        <div className="sidebar-user-pill"><div className="sidebar-user-avatar">A</div><div className="sidebar-user-info"><strong>{role || 'Accounts'}</strong><span>Billing staff</span></div></div>
        <button className="sidebar-logout" onClick={handleLogout}><LogOut size={16} />Logout</button>
      </div>
    </aside>
  )
}

export default AccountsSidebar
