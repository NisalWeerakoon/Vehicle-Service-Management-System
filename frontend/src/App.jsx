import {
  Navigate,
  Route,
  Routes,
} from 'react-router-dom'

import './App.css'

import ProtectedRoute from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ProfilePage from './pages/ProfilePage'
import EditProfilePage from './pages/EditProfilePage'
import VehiclesPage from './pages/VehiclesPage'
import AddVehiclePage from './pages/AddVehiclePage'
import EditVehiclePage from './pages/EditVehiclePage'

function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <Navigate
            to="/login"
            replace
          />
        }
      />

      <Route
        path="/login"
        element={<LoginPage />}
      />

      <Route
        path="/register"
        element={<RegisterPage />}
      />

      <Route
        path="/profile"
        element={
          <ProtectedRoute>
            <ProfilePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/profile/edit"
        element={
          <ProtectedRoute>
            <EditProfilePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="*"
        element={
          <Navigate
            to="/login"
            replace
          />
        }
      />

      <Route
        path="/vehicles"
        element={
          <ProtectedRoute>
            <VehiclesPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/vehicles/add"
        element={
          <ProtectedRoute>
            <AddVehiclePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/vehicles/:id/edit"
        element={
          <ProtectedRoute>
            <EditVehiclePage />
          </ProtectedRoute>
        }
      />
          </Routes>
        )
      }

export default App