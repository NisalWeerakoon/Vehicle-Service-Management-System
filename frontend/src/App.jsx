import {
  Navigate,
  Route,
  Routes,
} from 'react-router-dom'
import { MotionConfig } from 'motion/react'

import './App.css'
import './styles/premium.css'
import './styles/premium-components.css'
import './styles/premium-states.css'
import './styles/regression-fixes.css'
import './styles/phase-two.css'
import './styles/register-experience.css'
import './styles/brand-logo.css'

import ProtectedRoute from './components/ProtectedRoute'
import { getRole, isAuthenticated } from './services/api'
import { getRoleHomeRoute, ROLES } from './auth/roles'
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
import MechanicAssignmentsPage from './pages/MechanicAssignmentsPage'
import MyAssignedJobsPage from './pages/MyAssignedJobsPage'
import InspectionPage from './pages/InspectionPage'
import CompletedInspectionsPage from './pages/CompletedInspectionsPage'
import RepairTasksPage from './pages/RepairTasksPage'
import JobStatusPage from './pages/JobStatusPage'
import ServiceAdvisor from './pages/ServiceAdvisor'
import EditServiceAdvisorProfilePage from './pages/EditServiceAdvisorProfilePage'
import MechanicPage from './pages/MechanicPage'
import EditMechanicProfilePage from './pages/EditMechanicProfilePage'
import AdministratorPage from './pages/AdministratorPage'
import ActiveJobsDashboardPage from './pages/ActiveJobsDashboardPage'
import SparePartsPage from './pages/SparePartsPage'
import PartRequestPage from './pages/PartRequestPage'
import InventoryPartRequestsPage from './pages/InventoryPartRequestsPage'
import InventoryStockReportPage from './pages/InventoryStockReportPage'
import InventoryDashboardPage from './pages/InventoryDashboardPage'
import BillingChargesPage from './pages/BillingChargesPage'
import InvoicesPage from './pages/InvoicesPage'
import MyInvoicesPage from './pages/MyInvoicesPage'
import ServiceHistoryPage from './pages/ServiceHistoryPage'

import NotificationCenterPage from './pages/NotificationCenterPage'
import InvoicePaymentReportPage from './pages/InvoicePaymentReportPage'

function HomeRedirect() {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />
  }

  return <Navigate to={getRoleHomeRoute(getRole()) || '/login'} replace />
}

function App() {
  return (
    <MotionConfig reducedMotion="user">
      <a className="skip-link" href="#main-content">Skip to main content</a>
      <div id="main-content" className="route-stage">
        <Routes>
      <Route
        path="/"
        element={<HomeRedirect />}
      />

      <Route path="/notifications" element={<ProtectedRoute><NotificationCenterPage /></ProtectedRoute>} />

      <Route
        path="/admin"
        element={
          <ProtectedRoute roles={[ROLES.ADMINISTRATOR]}>
            <AdministratorPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/inventory/spare-parts"
        element={
          <ProtectedRoute roles={['InventoryOfficer', 'Administrator']}>
            <SparePartsPage />
          </ProtectedRoute>
        }
      />
      <Route path="/inventory/dashboard" element={<ProtectedRoute roles={['InventoryOfficer', 'Administrator']}><InventoryDashboardPage /></ProtectedRoute>} />
      <Route path="/inventory/part-requests" element={<ProtectedRoute roles={['InventoryOfficer', 'Administrator']}><InventoryPartRequestsPage /></ProtectedRoute>} />
      <Route path="/inventory/stock-report" element={<ProtectedRoute roles={['InventoryOfficer', 'Administrator']}><InventoryStockReportPage /></ProtectedRoute>} />
      <Route path="/inventory/low-stock-report" element={<ProtectedRoute roles={['InventoryOfficer', 'Administrator']}><InventoryStockReportPage lowStockOnly /></ProtectedRoute>} />
      <Route path="/mechanic/part-requests" element={<ProtectedRoute roles={['Mechanic']}><PartRequestPage /></ProtectedRoute>} />
      <Route path="/billing/part-charges" element={<ProtectedRoute roles={['Accounts', 'Administrator']}><BillingChargesPage /></ProtectedRoute>} />
      <Route path="/billing/invoices" element={<ProtectedRoute roles={['Accounts', 'Administrator']}><InvoicesPage /></ProtectedRoute>} />
      <Route path="/billing/reports/invoice-payments" element={<ProtectedRoute roles={['Accounts', 'Administrator']}><InvoicePaymentReportPage /></ProtectedRoute>} />
      <Route path="/invoices" element={<ProtectedRoute roles={['Customer']}><MyInvoicesPage /></ProtectedRoute>} />


      <Route
        path="/service-advisor"
        element={
          <ProtectedRoute roles={[ROLES.SERVICE_ADVISOR, ROLES.ADMINISTRATOR]}>
            <ServiceAdvisor />
          </ProtectedRoute>
        }
      />

      <Route
        path="/service-advisor/profile/edit"
        element={
          <ProtectedRoute roles={[ROLES.SERVICE_ADVISOR, ROLES.ADMINISTRATOR]}>
            <EditServiceAdvisorProfilePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/mechanic"
        element={
          <ProtectedRoute roles={[ROLES.MECHANIC]}>
            <MechanicPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/mechanic/profile/edit"
        element={
          <ProtectedRoute roles={[ROLES.MECHANIC]}>
            <EditMechanicProfilePage />
          </ProtectedRoute>
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
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <ProfilePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/profile/edit"
        element={
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <EditProfilePage />
          </ProtectedRoute>
        }
      />


      <Route
        path="/service-advisor/check-in"
        element={
          <ProtectedRoute roles={[ROLES.SERVICE_ADVISOR, ROLES.ADMINISTRATOR]}>
            <VehicleCheckInPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/service-advisor/job-cards"
        element={
          <ProtectedRoute roles={[ROLES.SERVICE_ADVISOR, ROLES.ADMINISTRATOR]}>
            <JobCardsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/service-advisor/mechanic-assignments"
        element={
          <ProtectedRoute roles={[ROLES.SERVICE_ADVISOR, ROLES.ADMINISTRATOR]}>
            <MechanicAssignmentsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/mechanic/inspections/:jobCardId"
        element={
          <ProtectedRoute roles={[ROLES.MECHANIC]}>
            <InspectionPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/mechanic/repairs/:jobCardId"
        element={
          <ProtectedRoute roles={[ROLES.MECHANIC]}>
            <RepairTasksPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/jobs/:jobCardId/status"
        element={
          <ProtectedRoute roles={[ROLES.MECHANIC, ROLES.SERVICE_ADVISOR, ROLES.ADMINISTRATOR]}>
            <JobStatusPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/reports/active-jobs"
        element={<ActiveJobsDashboardPage />}
      />

      <Route
        path="/service-advisor/completed-inspections"
        element={
          <ProtectedRoute roles={[ROLES.SERVICE_ADVISOR, ROLES.ADMINISTRATOR]}>
            <CompletedInspectionsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/mechanic/my-jobs"
        element={
          <ProtectedRoute roles={[ROLES.MECHANIC]}>
            <MyAssignedJobsPage />
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
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <VehiclesPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/vehicles/add"
        element={
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <AddVehiclePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/vehicles/:id/edit"
        element={
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <EditVehiclePage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/vehicles/:vehicleId/history"
        element={
          <ProtectedRoute roles={['Customer', 'ServiceAdvisor', 'Administrator']}>
            <ServiceHistoryPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/bookings"
        element={
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <BookingsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/bookings/create"
        element={
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <CreateBookingPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/bookings/:id"
        element={
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <BookingDetailsPage />
          </ProtectedRoute>
        }
      />

      <Route
        path="/bookings/:id/edit"
        element={
          <ProtectedRoute roles={[ROLES.CUSTOMER]}>
            <EditBookingPage />
          </ProtectedRoute>
        }
      />
        </Routes>
      </div>
    </MotionConfig>
  )
}

export default App
