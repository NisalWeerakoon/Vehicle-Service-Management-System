import { Navigate } from 'react-router-dom'
import { getRole, isAuthenticated } from '../services/api'

function ProtectedRoute({ children, roles = [] }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />
  }

  if (roles.length > 0 && !roles.includes(getRole())) {
    return <Navigate to="/" replace />
  }

  return children
}

export default ProtectedRoute
