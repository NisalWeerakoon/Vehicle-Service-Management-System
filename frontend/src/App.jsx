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
import BookingsPage from './pages/BookingsPage'
import CreateBookingPage from './pages/CreateBookingPage'
import BookingDetailsPage from './pages/BookingDetailsPage'
import EditBookingPage from './pages/EditBookingPage'
import VehicleCheckInPage from './pages/VehicleCheckInPage'
import JobCardsPage from './pages/JobCardsPage'

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
        path="/service-advisor/check-in"
        element={
          <ProtectedRoute>
            <VehicleCheckInPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/service-advisor/job-cards"
        element={
          <ProtectedRoute>
            <JobCardsPage />
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

      <Route
        path="/bookings"
        element={
          <ProtectedRoute>
            <BookingsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/bookings/create"
        element={
          <ProtectedRoute>
            <CreateBookingPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/bookings/:id"
        element={
          <ProtectedRoute>
            <BookingDetailsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/bookings/:id/edit"
        element={
          <ProtectedRoute>
            <EditBookingPage />
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}

export default App