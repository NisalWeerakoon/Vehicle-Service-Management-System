import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import MechanicSidebar from '../components/MechanicSidebar'
import {
  authApi,
  clearAuth,
  customerApi,
  jobCardApi,
  mechanicAssignmentApi,
} from '../services/api'

function MechanicPage() {
  const navigate = useNavigate()
  const [assignments, setAssignments] = useState([])
  const [jobs, setJobs] = useState({})
  const [profile, setProfile] = useState(null)
  const [userInfo, setUserInfo] = useState({
    email: localStorage.getItem('email') || '',
    role: localStorage.getItem('role') || 'Mechanic',
    userId: localStorage.getItem('userId') || '',
  })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [searchTerm, setSearchTerm] = useState('')

  const loadDashboardData = async () => {
    try {
      setLoading(true)
      setError('')

      const [assignmentsData, profileData, meData] = await Promise.allSettled([
        mechanicAssignmentApi.getMyJobs(),
        customerApi.getMyProfile(),
        authApi.me(),
      ])

      let myAssignments = []
      if (assignmentsData.status === 'fulfilled') {
        myAssignments = Array.isArray(assignmentsData.value)
          ? assignmentsData.value
          : []
        setAssignments(myAssignments)

        // Fetch details for each job card
        const details = await Promise.all(
          myAssignments.map(async (assignment) => {
            try {
              const job = await jobCardApi.getById(assignment.jobCardId)
              return [assignment.jobCardId, job]
            } catch {
              return [assignment.jobCardId, null]
            }
          })
        )
        setJobs(Object.fromEntries(details))
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
      setError(err.message || 'Failed to load mechanic dashboard data.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDashboardData()
  }, [])

  const filteredAssignments = assignments.filter((assignment) => {
    const job = jobs[assignment.jobCardId]
    const term = searchTerm.toLowerCase()
    const jobNum = job?.jobCardNumber || `Job #${assignment.jobCardId}`
    const regNum = job?.vehicleRegistrationNumber || ''
    const problems = job?.reportedProblems || ''

    return (
      jobNum.toLowerCase().includes(term) ||
      regNum.toLowerCase().includes(term) ||
      problems.toLowerCase().includes(term)
    )
  })

  const mechanicName =
    profile?.fullName ||
    userInfo?.fullName ||
    userInfo?.email?.split('@')[0] ||
    'Mechanic'
  const mechanicEmail = profile?.email || userInfo?.email || 'N/A'
  const mechanicPhone = profile?.phoneNumber || 'Not provided'
  const initial = mechanicName.charAt(0).toUpperCase()

  return (
    <div className="portal-layout">
      <MechanicSidebar />

      <main className="portal-main">
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">MECHANIC INTERFACE</span>
            <h1>Mechanic Workspace</h1>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div className="portal-user">
              <div
                className="portal-user-avatar"
                style={{ background: '#059669', color: 'white' }}
              >
                {initial}
              </div>
              <div>
                <strong>{mechanicName}</strong>
                <span>Mechanic Specialist</span>
              </div>
            </div>

            <button
              className="portal-primary-button"
              onClick={loadDashboardData}
            >
              ↻ Refresh Jobs
            </button>
          </div>
        </header>

        {error && (
          <div className="portal-error" style={{ marginBottom: '20px' }}>
            {error}
          </div>
        )}

        {/* Mechanic Profile Details Card */}
        <section
          className="portal-card"
          style={{
            marginBottom: '28px',
            padding: '24px',
            background: 'linear-gradient(135deg, #ffffff 0%, #f0fdf4 100%)',
            borderLeft: '5px solid #059669',
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
                  background: 'linear-gradient(135deg, #059669, #10b981)',
                  color: 'white',
                  fontSize: '26px',
                  fontWeight: '800',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  boxShadow: '0 8px 20px rgba(16, 185, 129, 0.25)',
                }}
              >
                {initial}
              </div>

              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                  <h2 style={{ margin: 0, fontSize: '22px', fontWeight: 800 }}>
                    {mechanicName}
                  </h2>
                  <span
                    style={{
                      background: '#d1fae5',
                      color: '#065f46',
                      fontSize: '12px',
                      fontWeight: 700,
                      padding: '4px 10px',
                      borderRadius: '20px',
                    }}
                  >
                    ● Active Workshop Specialist
                  </span>
                </div>

                <div
                  style={{
                    display: 'flex',
                    gap: '20px',
                    marginTop: '8px',
                    color: '#4b5563',
                    fontSize: '14px',
                    flexWrap: 'wrap',
                  }}
                >
                  <span>✉️ {mechanicEmail}</span>
                  <span>📞 {mechanicPhone}</span>
                  <span>🆔 Staff #{userInfo?.userId || profile?.id || 'TECH-01'}</span>
                </div>
              </div>
            </div>

            <button
              className="portal-secondary-button"
              onClick={() => navigate('/mechanic/profile/edit')}
              style={{ fontSize: '14px', padding: '10px 18px' }}
            >
              ✏️ Edit Profile
            </button>
          </div>
        </section>

        {/* Workload Metric Cards */}
        <section className="portal-grid" style={{ marginBottom: '28px' }}>
          <div className="portal-card" style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div
              style={{
                fontSize: '28px',
                width: '52px',
                height: '52px',
                borderRadius: '12px',
                background: 'rgba(5, 150, 105, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#059669',
              }}
            >
              🛠️
            </div>
            <div>
              <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                ASSIGNED JOBS
              </span>
              <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                {loading ? '...' : assignments.length}
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
                background: 'rgba(37, 99, 235, 0.1)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#2563eb',
              }}
            >
              📋
            </div>
            <div>
              <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                INSPECTION REQUIRED
              </span>
              <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                {loading ? '...' : assignments.length}
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
              ⚙️
            </div>
            <div>
              <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                REPAIR OPERATIONS
              </span>
              <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                {loading ? '...' : assignments.length}
              </h2>
            </div>
          </div>
        </section>

        {/* Assigned Jobs List */}
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
                My Assigned Maintenance Queue
              </h2>
              <span style={{ fontSize: '13px', color: '#64748b' }}>
                Service jobs assigned to your technician account
              </span>
            </div>

            <input
              type="text"
              placeholder="Search assigned jobs..."
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
            <p style={{ color: '#64748b' }}>Loading assigned jobs...</p>
          ) : filteredAssignments.length === 0 ? (
            <p style={{ color: '#64748b' }}>No maintenance jobs currently assigned to you.</p>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '14px' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid #e2e8f0', textAlign: 'left', color: '#64748b' }}>
                    <th style={{ padding: '12px 8px' }}>Job Card #</th>
                    <th style={{ padding: '12px 8px' }}>Vehicle Reg</th>
                    <th style={{ padding: '12px 8px' }}>Reported Problems</th>
                    <th style={{ padding: '12px 8px' }}>Assigned Date</th>
                    <th style={{ padding: '12px 8px' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredAssignments.map((assignment) => {
                    const job = jobs[assignment.jobCardId]
                    return (
                      <tr key={assignment.id} style={{ borderBottom: '1px solid #f1f5f9' }}>
                        <td style={{ padding: '12px 8px', fontWeight: 600 }}>
                          {job?.jobCardNumber || `Job #${assignment.jobCardId}`}
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
                            {job?.vehicleRegistrationNumber || 'N/A'}
                          </span>
                        </td>
                        <td style={{ padding: '12px 8px', color: '#475569' }}>
                          {job?.reportedProblems || 'No specific problems reported'}
                        </td>
                        <td style={{ padding: '12px 8px', color: '#64748b', fontSize: '13px' }}>
                          {new Date(assignment.assignedAt).toLocaleDateString()}
                        </td>
                        <td style={{ padding: '12px 8px' }}>
                          <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                            <button
                              className="portal-primary-button"
                              style={{ padding: '6px 12px', fontSize: '13px' }}
                              onClick={() => navigate(`/mechanic/inspections/${assignment.jobCardId}`)}
                            >
                              Inspection
                            </button>
                            <button
                              className="portal-secondary-button"
                              style={{ padding: '6px 12px', fontSize: '13px' }}
                              onClick={() => navigate(`/mechanic/repairs/${assignment.jobCardId}`)}
                            >
                              Repairs
                            </button>
                            <button
                              className="portal-secondary-button"
                              style={{ padding: '6px 12px', fontSize: '13px' }}
                              onClick={() => navigate(`/jobs/${assignment.jobCardId}/status`)}
                            >
                              Status
                            </button>
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </main>
    </div>
  )
}

export default MechanicPage
