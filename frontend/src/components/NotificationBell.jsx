import { useCallback, useEffect, useState } from 'react'
import { Bell } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { notificationApi } from '../services/api'

export default function NotificationBell() {
  const navigate = useNavigate()
  const [count, setCount] = useState(0)

  const loadCount = useCallback(async () => {
    try {
      const result = await notificationApi.getUnreadCount()
      setCount(result?.count || 0)
    } catch {
      setCount(0)
    }
  }, [])

  useEffect(() => {
    const request = window.setTimeout(loadCount, 0)
    window.addEventListener('notifications-changed', loadCount)
    return () => {
      window.clearTimeout(request)
      window.removeEventListener('notifications-changed', loadCount)
    }
  }, [loadCount])

  return <button className="notification-bell" onClick={() => navigate('/notifications')} aria-label={count ? `${count} unread notifications` : 'Notifications'} title="Notifications">
    <Bell size={19} />
    {count > 0 && <span className="notification-badge">{count > 99 ? '99+' : count}</span>}
  </button>
}
