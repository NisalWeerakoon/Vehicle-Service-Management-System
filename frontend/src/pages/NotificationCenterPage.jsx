import { useCallback, useEffect, useMemo, useState } from 'react'
import { Bell } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import AccountsSidebar from '../components/AccountsSidebar'
import AdminSidebar from '../components/AdminSidebar'
import CustomerSidebar from '../components/CustomerSidebar'
import InventorySidebar from '../components/InventorySidebar'
import MechanicSidebar from '../components/MechanicSidebar'
import PageHeader from '../components/PageHeader'
import ServiceAdvisorSidebar from '../components/ServiceAdvisorSidebar'
import { clearAuth, getRole, notificationApi } from '../services/api'

function PortalSidebar() {
  const role = getRole()
  if (role === 'Administrator') return <AdminSidebar />
  if (role === 'Accounts') return <AccountsSidebar />
  if (role === 'InventoryOfficer') return <InventorySidebar />
  if (role === 'Mechanic') return <MechanicSidebar />
  if (role === 'ServiceAdvisor' || role === 'Staff') return <ServiceAdvisorSidebar />
  return <CustomerSidebar />
}

export default function NotificationCenterPage() {
  const navigate = useNavigate()
  const [notifications, setNotifications] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [updating, setUpdating] = useState(false)

  const load = useCallback(async () => {
    try {
      setLoading(true)
      setError('')
      setNotifications(await notificationApi.getMine())
    } catch (err) {
      if (err.status === 401) { clearAuth(); navigate('/login'); return }
      setError(err.message || 'Unable to load notifications.')
    } finally {
      setLoading(false)
    }
  }, [navigate])

  useEffect(() => {
    const request = window.setTimeout(load, 0)
    return () => window.clearTimeout(request)
  }, [load])

  const unreadCount = useMemo(() => notifications.filter((item) => !item.isRead).length, [notifications])

  async function markAsRead(id) {
    try {
      setUpdating(true); setError('')
      const updated = await notificationApi.markAsRead(id)
      setNotifications((current) => current.map((item) => item.id === id ? updated : item))
      window.dispatchEvent(new Event('notifications-changed'))
    } catch (err) {
      setError(err.message || 'Unable to mark the notification as read.')
    } finally { setUpdating(false) }
  }

  async function markAllAsRead() {
    try {
      setUpdating(true); setError('')
      await notificationApi.markAllAsRead()
      const readAt = new Date().toISOString()
      setNotifications((current) => current.map((item) => item.isRead ? item : { ...item, isRead: true, readAt }))
      window.dispatchEvent(new Event('notifications-changed'))
    } catch (err) {
      setError(err.message || 'Unable to mark all notifications as read.')
    } finally { setUpdating(false) }
  }

  return <div className="portal-layout"><PortalSidebar /><main className="portal-main">
    <PageHeader eyebrow="ACCOUNT ACTIVITY" title="NOTIFICATIONS" description="Stay updated with activity related to your account and service." actions={<button className="portal-secondary-button" onClick={load}>Refresh</button>} />
    <div className="portal-content notification-center-content">
      {error && <div className="portal-error">{error}</div>}
      <section className="notification-summary portal-card">
        <div><span className="portal-eyebrow">UNREAD</span><strong>{loading ? '—' : unreadCount}</strong><p>{unreadCount === 1 ? 'notification needs' : 'notifications need'} your attention</p></div>
        <button className="portal-secondary-button" disabled={updating || unreadCount === 0} onClick={markAllAsRead}>{updating ? 'Updating...' : unreadCount === 0 ? 'All notifications read' : 'Mark all as read'}</button>
      </section>
      <section className="portal-card">
        <div className="notification-list-heading"><div><h2>Recent notifications</h2><p>Newest activity appears first.</p></div></div>
        {loading ? <div className="portal-loading-card"><div className="loading-spinner" /><p>Loading notifications...</p></div> : notifications.length === 0 ? <div className="modern-empty-state"><div className="modern-empty-icon"><Bell size={32} /></div><h2>No notifications</h2><p>Activity related to your account and service will appear here.</p></div> : <div className="notification-list">{notifications.map((item) => <article className={item.isRead ? 'notification-item' : 'notification-item unread'} key={item.id}>
          <div className="notification-item-main"><div className="notification-item-heading"><span className="notification-type">{item.type || 'General'}</span>{!item.isRead && <span className="notification-unread-label">Unread</span>}</div><h3>{item.title}</h3><p>{item.message}</p><time dateTime={item.createdAt}>{new Date(item.createdAt).toLocaleString()}</time></div>
          {!item.isRead && <button className="portal-secondary-button" disabled={updating} onClick={() => markAsRead(item.id)}>Mark as read</button>}
        </article>)}</div>}
      </section>
    </div>
  </main></div>
}
