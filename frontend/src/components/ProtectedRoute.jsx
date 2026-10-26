import { Navigate } from 'react-router-dom'
import { getRole, isAuthenticated } from '../services/api'
import { getRoleHomeRoute, roleIsAllowed } from '../auth/roles'

function ProtectedRoute({ children, roles = [] }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />
  }

  const role = getRole()
  if (roles.length > 0 && !roleIsAllowed(role, roles)) {
    return <Navigate to={getRoleHomeRoute(role) || '/login'} replace />
  }

  return children
}

export default ProtectedRoute
