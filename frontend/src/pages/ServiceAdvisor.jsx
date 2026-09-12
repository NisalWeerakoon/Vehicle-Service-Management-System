import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import ServiceAdvisorSidebar from '../components/ServiceAdvisorSidebar'
import {
  authApi,
  bookingApi,
  clearAuth,
  customerApi,
  inspectionApi,
  jobCardApi,
  mechanicApi,
} from '../services/api'

function ServiceAdvisor() {
  const navigate = useNavigate()
  const [jobCards, setJobCards] = useState([])
  const [completedInspections, setCompletedInspections] = useState([])
  const [readyBookings, setReadyBookings] = useState([])
  const [mechanics, setMechanics] = useState([])
  const [profile, setProfile] = useState(null)
  const [userInfo, setUserInfo] = useState({
    email: localStorage.getItem('email') || '',
    role: localStorage.getItem('role') || 'ServiceAdvisor',
    userId: localStorage.getItem('userId') || '',
  })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [searchTerm, setSearchTerm] = useState('')

  const loadDashboardData = async () => {
    try {
      setLoading(true)
      setError('')

      const [
        jobsData,
        inspectionsData,
        readyBookingsData,
        mechanicsData,
        profileData,
        meData,
      ] = await Promise.allSettled([
        jobCardApi.getAll(),
        inspectionApi.getCompleted(),
        bookingApi.getStaffCheckInReady(),
        mechanicApi.getActiveMechanics(),
        customerApi.getMyProfile(),
        authApi.me(),
      ])

      if (jobsData.status === 'fulfilled') {
        setJobCards(Array.isArray(jobsData.value) ? jobsData.value : [])
      }

      if (inspectionsData.status === 'fulfilled') {
        setCompletedInspections(
          Array.isArray(inspectionsData.value) ? inspectionsData.value : []
        )
      }

      if (readyBookingsData.status === 'fulfilled') {
        setReadyBookings(
          Array.isArray(readyBookingsData.value) ? readyBookingsData.value : []
        )
      }

      if (mechanicsData.status === 'fulfilled') {
        setMechanics(
          Array.isArray(mechanicsData.value) ? mechanicsData.value : []
        )
      }

      if (profileData.status === 'fulfilled' && profileData.value) {
        setProfile(profileData.value)
      }

      if (meData.status === 'fulfilled' && meData.value) {
        setUserInfo((prev) => ({
          ...prev,
          ...meData.value,
        }))
      }
    } catch (err) {
      if (err.status === 401 || err.status === 403) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(err.message || 'Failed to load dashboard data.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDashboardData()
  }, [])

  const filteredJobs = jobCards.filter((job) => {
    const term = searchTerm.toLowerCase()
    return (
      (job.jobCardNumber && job.jobCardNumber.toLowerCase().includes(term)) ||
      (job.vehicleRegistrationNumber &&
        job.vehicleRegistrationNumber.toLowerCase().includes(term)) ||
      (job.reportedProblems &&
        job.reportedProblems.toLowerCase().includes(term))
    )
  })

  const advisorName =
    profile?.fullName ||
    userInfo?.fullName ||
    userInfo?.email?.split('@')[0] ||
    'Service Advisor'
  const advisorEmail = profile?.email || userInfo?.email || 'N/A'
  const advisorPhone = profile?.phoneNumber || 'Not provided'
  const initial = advisorName.charAt(0).toUpperCase()

  return (
    <div className="portal-layout">
      <ServiceAdvisorSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">SERVICE ADVISOR DASHBOARD</span>
            <h1>Service Advisor Hub</h1>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div className="portal-user">
              <div className="portal-user-avatar" style={{ background: '#2563eb', color: 'white' }}>
                {initial}
              </div>
              <div>
                <strong>{advisorName}</strong>
                <span>Service Advisor</span>
              </div>
            </div>

            <button
              className="portal-primary-button"
              onClick={() => navigate('/service-advisor/check-in')}
            >
              📋 Vehicle Check-In
            </button>
          </div>
        </header>

        {error && (
          <div className="portal-error" style={{ marginBottom: '20px' }}>
            {error}
          </div>
        )}

        {/* Service Advisor Profile Card */}
        <section
          className="portal-card"
          style={{
            marginBottom: '28px',
            padding: '24px',
            background: 'linear-gradient(135deg, #ffffff 0%, #f8fafc 100%)',
            borderLeft: '5px solid #2563eb',
          }}
        >
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              flexWrap: 'wrap',
              gap: '20px',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '20px' }}>
              <div
                style={{
                  width: '64px',
                  height: '64px',
                  borderRadius: '16px',
                  background: 'linear-gradient(135deg, #2563eb, #0ea5e9)',
                  color: 'white',
                  fontSize: '26px',
                  fontWeight: '800',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  boxShadow: '0 8px 20px rgba(37, 99, 235, 0.25)',
                }}
              >
                {initial}
              </div>

              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                  <h2 style={{ margin: 0, fontSize: '22px', fontWeight: 800 }}>
                    {advisorName}
                  </h2>
                  <span
                    style={{
                      background: '#dbeafe',
                      color: '#1e40af',
                      fontSize: '12px',
                      fontWeight: 700,
                      padding: '4px 10px',
                      borderRadius: '20px',
                    }}
                  >
                    ● Active Service Advisor
                  </span>
                </div>

                <div
                  style={{
                    display: 'flex',
                    gap: '20px',
                    marginTop: '8px',
                    color: '#64748b',
                    fontSize: '14px',
                    flexWrap: 'wrap',
                  }}
                >
                  <span>✉️ {advisorEmail}</span>
                  <span>📞 {advisorPhone}</span>
                  <span>🆔 Staff #{userInfo?.userId || profile?.id || 'SA-01'}</span>
                </div>
              </div>
            </div>

            <button
              className="portal-secondary-button"
              onClick={() => navigate('/service-advisor/profile/edit')}
              style={{ fontSize: '14px', padding: '10px 18px' }}
            >
              ✏️ Edit Profile
            </button>
          </div>
        </section>

        {/* Overview Stats Cards */}
        <section className="portal-grid" style={{ marginBottom: '28px' }}>
          <div className="portal-card" style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div
              style={{
                fontSize: '28px',
                width: '52px',
                height: '52px',
                borderRadius: '12px',
                background: 'rgba(37, 99, 235, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#2563eb',
              }}
            >
              📑
            </div>
            <div>
              <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                TOTAL JOB CARDS
              </span>
              <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                {loading ? '...' : jobCards.length}
              </h2>
            </div>
          </div>

          <div className="portal-card" style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div
              style={{
                fontSize: '28px',
                width: '52px',
                height: '52px',
                borderRadius: '12px',
                background: 'rgba(14, 165, 233, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#0ea5e9',
              }}
            >
              🚘
            </div>
            <div>
              <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                READY FOR CHECK-IN
              </span>
              <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                {loading ? '...' : readyBookings.length}
              </h2>
            </div>
          </div>

          <div className="portal-card" style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div
              style={{
                fontSize: '28px',
                width: '52px',
                height: '52px',
                borderRadius: '12px',
                background: 'rgba(16, 185, 129, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#10b981',
              }}
            >
              ✅
            </div>
            <div>
              <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                COMPLETED INSPECTIONS
              </span>
              <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                {loading ? '...' : completedInspections.length}
              </h2>
            </div>
          </div>

          <div className="portal-card" style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div
              style={{
                fontSize: '28px',
                width: '52px',
                height: '52px',
                borderRadius: '12px',
                background: 'rgba(245, 158, 11, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#f59e0b',
              }}
            >
              👨‍🔧
            </div>
            <div>
              <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                ACTIVE MECHANICS
              </span>
              <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                {loading ? '...' : mechanics.length}
              </h2>
            </div>
          </div>
        </section>

        {/* Ready for Check-In Widget */}
        {readyBookings.length > 0 && (
          <section className="portal-card" style={{ marginBottom: '28px', padding: '24px' }}>
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '16px',
              }}
            >
              <div>
                <h2 style={{ margin: 0, fontSize: '18px', fontWeight: 700 }}>
                  🚘 Booked Vehicles Awaiting Check-In
                </h2>
                <span style={{ fontSize: '13px', color: '#64748b' }}>
                  Confirmed customer bookings arriving at workshop
                </span>
              </div>
              <button
                className="portal-secondary-button"
                onClick={() => navigate('/service-advisor/check-in')}
              >
                View All Check-Ins →
              </button>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              {readyBookings.slice(0, 3).map((b) => (
                <div
                  key={b.id}
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    background: '#f8fafc',
                    padding: '12px 16px',
                    borderRadius: '10px',
                    border: '1px solid #e2e8f0',
                  }}
                >
                  <div>
                    <strong style={{ fontSize: '15px' }}>{b.vehicleRegistrationNumber || `Booking #${b.id}`}</strong>
                    <span style={{ marginLeft: '12px', color: '#64748b', fontSize: '13px' }}>
                      {b.serviceType || 'Standard Service'}
                    </span>
                  </div>
                  <button
                    className="portal-primary-button"
                    style={{ padding: '6px 12px', fontSize: '13px' }}
                    onClick={() => navigate('/service-advisor/check-in')}
                  >
                    Check In Now →
                  </button>
                </div>
              ))}
            </div>
          </section>
        )}

        {/* Recent Job Cards List */}
        <section className="portal-card" style={{ padding: '24px' }}>
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: '20px',
              flexWrap: 'wrap',
              gap: '12px',
            }}
          >
            <div>
              <h2 style={{ margin: 0, fontSize: '18px', fontWeight: 700 }}>
                Recent Job Cards
              </h2>
              <span style={{ fontSize: '13px', color: '#64748b' }}>
                All active vehicle repair job cards registered in the system
              </span>
            </div>

            <input
              type="text"
              placeholder="Search by job #, vehicle reg..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{
                padding: '10px 14px',
                borderRadius: '10px',
                border: '1px solid #cbd5e1',
                fontSize: '14px',
                width: '260px',
              }}
            />
          </div>

          {loading ? (
            <p style={{ color: '#64748b' }}>Loading job cards...</p>
          ) : filteredJobs.length === 0 ? (
            <p style={{ color: '#64748b' }}>No job cards found.</p>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '14px' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid #e2e8f0', textAlign: 'left', color: '#64748b' }}>
                    <th style={{ padding: '12px 8px' }}>Job Card #</th>
                    <th style={{ padding: '12px 8px' }}>Reg Number</th>
                    <th style={{ padding: '12px 8px' }}>Reported Problems</th>
                    <th style={{ padding: '12px 8px' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredJobs.map((job) => (
                    <tr
                      key={job.id || job.jobCardNumber}
                      style={{ borderBottom: '1px solid #f1f5f9' }}
                    >
                      <td style={{ padding: '12px 8px', fontWeight: 600 }}>
                        {job.jobCardNumber || `#${job.id}`}
                      </td>
                      <td style={{ padding: '12px 8px' }}>
                        <span
                          style={{
                            background: '#e2e8f0',
                            padding: '4px 8px',
                            borderRadius: '6px',
                            fontWeight: 700,
                            fontFamily: 'monospace',
                          }}
                        >
                          {job.vehicleRegistrationNumber || 'N/A'}
                        </span>
                      </td>
                      <td style={{ padding: '12px 8px', color: '#475569' }}>
                        {job.reportedProblems || 'None specified'}
                      </td>
                      <td style={{ padding: '12px 8px' }}>
                        <div style={{ display: 'flex', gap: '8px' }}>
                          <button
                            className="portal-secondary-button"
                            style={{ padding: '6px 12px', fontSize: '13px' }}
                            onClick={() => navigate(`/jobs/${job.id || job.jobCardNumber}/status`)}
                          >
                            Status
                          </button>
                          <button
                            className="portal-secondary-button"
                            style={{ padding: '6px 12px', fontSize: '13px' }}
                            onClick={() => navigate('/service-advisor/mechanic-assignments')}
                          >
                            Assign
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </main>
    </div>
  )
}

export default ServiceAdvisor
