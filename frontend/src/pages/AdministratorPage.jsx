import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import AdminSidebar from '../components/AdminSidebar'
import { adminApi, authApi, clearAuth } from '../services/api'

function AdministratorPage() {
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState('dashboard')

  // Dashboard Data State
  const [stats, setStats] = useState({
    totalUsers: 0,
    usersByRole: {},
    totalBookings: 0,
    bookingsByStatus: {},
    totalVehicles: 0,
    totalCheckIns: 0,
  })
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [successMsg, setSuccessMsg] = useState('')

  // Filter States for User Management
  const [searchTerm, setSearchTerm] = useState('')
  const [roleFilter, setRoleFilter] = useState('ALL')
  const [statusFilter, setStatusFilter] = useState('ALL')

  // Modal State for Staff / User Creation
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [createForm, setCreateForm] = useState({
    email: '',
    password: '',
    role: 'ServiceAdvisor',
    fullName: '',
    phone: '',
  })
  const [createLoading, setCreateLoading] = useState(false)
  const [createError, setCreateError] = useState('')

  // Admin User Info
  const [adminUser, setAdminUser] = useState({
    email: localStorage.getItem('email') || 'admin@system.com',
    role: localStorage.getItem('role') || 'Administrator',
    userId: localStorage.getItem('userId') || '',
  })

  // Load All Data
  const loadAdminData = async () => {
    try {
      setLoading(true)
      setError('')

      const [statsRes, usersRes, meRes] = await Promise.allSettled([
        adminApi.getStats(),
        adminApi.getAllUsers(),
        authApi.me(),
      ])

      if (statsRes.status === 'fulfilled' && statsRes.value) {
        setStats(statsRes.value)
      }

      if (usersRes.status === 'fulfilled' && Array.isArray(usersRes.value)) {
        setUsers(usersRes.value)
      }

      if (meRes.status === 'fulfilled' && meRes.value) {
        setAdminUser((prev) => ({
          ...prev,
          ...meRes.value,
        }))
      }
    } catch (err) {
      if (err.status === 401 || err.status === 403) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(err.message || 'Failed to load administrator control panel data.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadAdminData()
  }, [])

  // Auto-dismiss success notification
  useEffect(() => {
    if (successMsg) {
      const timer = setTimeout(() => setSuccessMsg(''), 4000)
      return () => clearTimeout(timer)
    }
  }, [successMsg])

  // Handle Role Change
  const handleRoleChange = async (userId, newRole) => {
    try {
      setError('')
      await adminApi.updateUserRole(userId, newRole)
      setUsers((prev) =>
        prev.map((u) => (u.id === userId ? { ...u, role: newRole } : u))
      )
      setSuccessMsg(`User #${userId} role updated to ${newRole}.`)
      adminApi.getStats().then(setStats).catch(() => {})
    } catch (err) {
      setError(err.message || 'Failed to update user role.')
    }
  }

  // Handle Status Toggle (Active / Inactive)
  const handleToggleStatus = async (userId, currentStatus) => {
    const nextStatus = !currentStatus
    try {
      setError('')
      await adminApi.toggleUserStatus(userId, nextStatus)
      setUsers((prev) =>
        prev.map((u) => (u.id === userId ? { ...u, isActive: nextStatus } : u))
      )
      setSuccessMsg(
        `User #${userId} has been ${nextStatus ? 'Activated' : 'Deactivated'}.`
      )
    } catch (err) {
      setError(err.message || 'Failed to change user account status.')
    }
  }

  // Handle Create User Submit
  const handleCreateSubmit = async (e) => {
    e.preventDefault()
    setCreateError('')

    if (!createForm.email || !createForm.password) {
      setCreateError('Email and Password are required.')
      return
    }

    try {
      setCreateLoading(true)
      const newUser = await adminApi.createUser(createForm)
      setUsers((prev) => [newUser, ...prev])
      setSuccessMsg(`Account for ${newUser.email} created successfully as ${newUser.role}!`)
      setShowCreateModal(false)
      setCreateForm({
        email: '',
        password: '',
        role: 'ServiceAdvisor',
        fullName: '',
        phone: '',
      })
      adminApi.getStats().then(setStats).catch(() => {})
    } catch (err) {
      setCreateError(err.message || 'Failed to create user account.')
    } finally {
      setCreateLoading(false)
    }
  }

  // Filter Users
  const filteredUsers = users.filter((u) => {
    const term = searchTerm.toLowerCase()
    const matchesSearch =
      u.email.toLowerCase().includes(term) ||
      (u.fullName && u.fullName.toLowerCase().includes(term)) ||
      u.id.toString().includes(term)

    const matchesRole = roleFilter === 'ALL' || u.role === roleFilter
    const matchesStatus =
      statusFilter === 'ALL' ||
      (statusFilter === 'ACTIVE' && u.isActive) ||
      (statusFilter === 'INACTIVE' && !u.isActive)

    return matchesSearch && matchesRole && matchesStatus
  })

  const adminName = adminUser.email.split('@')[0] || 'Admin'
  const initial = adminName.charAt(0).toUpperCase()

  return (
    <div className="portal-layout">
      <AdminSidebar activeTab={activeTab} setActiveTab={setActiveTab} />

      <main className="portal-main">
        {/* Top Header */}
        <header className="portal-topbar">
          <div>
            <span className="portal-eyebrow">SYSTEM ADMINISTRATION</span>
            <h1>
              {activeTab === 'dashboard' && 'Administrator Dashboard'}
              {activeTab === 'users' && 'User & Staff Management'}
              {activeTab === 'reports' && 'System Analytics & Reports'}
            </h1>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div className="portal-user">
              <div
                className="portal-user-avatar"
                style={{ background: '#2563eb', color: 'white' }}
              >
                {initial}
              </div>
              <div>
                <strong>{adminUser.email}</strong>
                <span>System Administrator</span>
              </div>
            </div>

            <button
              className="portal-primary-button"
              onClick={() => setShowCreateModal(true)}
            >
              + Create Staff / User
            </button>
          </div>
        </header>

        {/* Notifications */}
        {error && (
          <div className="portal-error" style={{ marginBottom: '20px' }}>
            <span>⚠️</span> {error}
          </div>
        )}

        {successMsg && (
          <div className="portal-success" style={{ marginBottom: '20px' }}>
            <span>✅</span> {successMsg}
          </div>
        )}

        {loading ? (
          <div className="portal-card portal-loading-card" style={{ padding: '60px', textAlign: 'center' }}>
            <div className="spinner" style={{ margin: '0 auto 16px' }}></div>
            <p>Loading control panel data...</p>
          </div>
        ) : (
          <>
            {/* Profile / Status Banner */}
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
                    ⚡
                  </div>

                  <div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                      <h2 style={{ margin: 0, fontSize: '22px', fontWeight: 800 }}>
                        {adminUser.email}
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
                        ● Active System Administrator
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
                      <span>✉️ {adminUser.email}</span>
                      <span>🆔 Admin ID #{adminUser.userId || '1'}</span>
                      <span>🛡️ Full System Access</span>
                    </div>
                  </div>
                </div>
              </div>
            </section>

            {/* ========================================================
               TAB 1: DASHBOARD OVERVIEW
               ======================================================== */}
            {activeTab === 'dashboard' && (
              <>
                {/* Stats Grid */}
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
                      👥
                    </div>
                    <div>
                      <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                        TOTAL USERS
                      </span>
                      <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                        {stats.totalUsers}
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
                      📅
                    </div>
                    <div>
                      <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                        TOTAL BOOKINGS
                      </span>
                      <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                        {stats.totalBookings}
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
                      🚘
                    </div>
                    <div>
                      <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                        REGISTERED VEHICLES
                      </span>
                      <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                        {stats.totalVehicles}
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
                        background: 'rgba(139, 92, 246, 0.1)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: '#8b5cf6',
                      }}
                    >
                      📋
                    </div>
                    <div>
                      <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 600 }}>
                        TOTAL CHECK-INS
                      </span>
                      <h2 style={{ margin: '4px 0 0 0', fontSize: '24px', fontWeight: 800 }}>
                        {stats.totalCheckIns}
                      </h2>
                    </div>
                  </div>
                </section>

                {/* Role Allocation Summary Card */}
                <section className="portal-card" style={{ marginBottom: '28px' }}>
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      marginBottom: '20px',
                    }}
                  >
                    <h2 style={{ margin: 0, fontSize: '18px', fontWeight: 800 }}>
                      Role Allocation Summary
                    </h2>
                    <button
                      className="portal-secondary-button"
                      onClick={() => setActiveTab('users')}
                    >
                      View All Users →
                    </button>
                  </div>

                  <div className="portal-grid">
                    <div
                      style={{
                        padding: '16px',
                        background: '#f8fafc',
                        borderRadius: '12px',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '14px',
                      }}
                    >
                      <span style={{ fontSize: '24px' }}>👨‍💼</span>
                      <div>
                        <strong style={{ display: 'block', fontSize: '15px' }}>Service Advisors</strong>
                        <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 700 }}>
                          {stats.usersByRole['ServiceAdvisor'] || 0} Accounts
                        </span>
                      </div>
                    </div>

                    <div
                      style={{
                        padding: '16px',
                        background: '#f8fafc',
                        borderRadius: '12px',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '14px',
                      }}
                    >
                      <span style={{ fontSize: '24px' }}>👨‍🔧</span>
                      <div>
                        <strong style={{ display: 'block', fontSize: '15px' }}>Mechanics</strong>
                        <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 700 }}>
                          {stats.usersByRole['Mechanic'] || 0} Accounts
                        </span>
                      </div>
                    </div>

                    <div
                      style={{
                        padding: '16px',
                        background: '#f8fafc',
                        borderRadius: '12px',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '14px',
                      }}
                    >
                      <span style={{ fontSize: '24px' }}>📦</span>
                      <div>
                        <strong style={{ display: 'block', fontSize: '15px' }}>Inventory Officers</strong>
                        <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 700 }}>
                          {stats.usersByRole['InventoryOfficer'] || 0} Accounts
                        </span>
                      </div>
                    </div>

                    <div
                      style={{
                        padding: '16px',
                        background: '#f8fafc',
                        borderRadius: '12px',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '14px',
                      }}
                    >
                      <span style={{ fontSize: '24px' }}>💳</span>
                      <div>
                        <strong style={{ display: 'block', fontSize: '15px' }}>Accounts / Cashiers</strong>
                        <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 700 }}>
                          {stats.usersByRole['Accounts'] || 0} Accounts
                        </span>
                      </div>
                    </div>

                    <div
                      style={{
                        padding: '16px',
                        background: '#f8fafc',
                        borderRadius: '12px',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '14px',
                      }}
                    >
                      <span style={{ fontSize: '24px' }}>👤</span>
                      <div>
                        <strong style={{ display: 'block', fontSize: '15px' }}>Customers</strong>
                        <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 700 }}>
                          {stats.usersByRole['Customer'] || 0} Accounts
                        </span>
                      </div>
                    </div>

                    <div
                      style={{
                        padding: '16px',
                        background: '#f8fafc',
                        borderRadius: '12px',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '14px',
                      }}
                    >
                      <span style={{ fontSize: '24px' }}>⚡</span>
                      <div>
                        <strong style={{ display: 'block', fontSize: '15px' }}>Administrators</strong>
                        <span style={{ fontSize: '13px', color: '#64748b', fontWeight: 700 }}>
                          {stats.usersByRole['Administrator'] || 0} Accounts
                        </span>
                      </div>
                    </div>
                  </div>
                </section>
              </>
            )}

            {/* ========================================================
               TAB 2: USER & STAFF MANAGEMENT
               ======================================================== */}
            {activeTab === 'users' && (
              <section className="portal-card">
                <div
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    flexWrap: 'wrap',
                    gap: '16px',
                    marginBottom: '20px',
                  }}
                >
                  <h2 style={{ margin: 0, fontSize: '18px', fontWeight: 800 }}>
                    System User Accounts ({filteredUsers.length} of {users.length})
                  </h2>

                  <button
                    className="portal-primary-button"
                    onClick={() => setShowCreateModal(true)}
                  >
                    + Create Staff / User
                  </button>
                </div>

                {/* Filter Controls */}
                <div
                  style={{
                    display: 'flex',
                    gap: '16px',
                    marginBottom: '20px',
                    flexWrap: 'wrap',
                    alignItems: 'center',
                  }}
                >
                  <input
                    type="text"
                    placeholder="Search by email, name, or ID..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    style={{
                      flex: 1,
                      minWidth: '220px',
                      padding: '10px 14px',
                      border: '1px solid #cbd5e1',
                      borderRadius: '10px',
                      fontSize: '14px',
                    }}
                  />

                  <select
                    value={roleFilter}
                    onChange={(e) => setRoleFilter(e.target.value)}
                    style={{
                      padding: '10px 14px',
                      border: '1px solid #cbd5e1',
                      borderRadius: '10px',
                      fontSize: '14px',
                      background: 'white',
                    }}
                  >
                    <option value="ALL">All Roles</option>
                    <option value="ServiceAdvisor">Service Advisor</option>
                    <option value="Mechanic">Mechanic</option>
                    <option value="InventoryOfficer">Inventory Officer</option>
                    <option value="Accounts">Accounts / Cashier</option>
                    <option value="Customer">Customer</option>
                    <option value="Administrator">Administrator</option>
                  </select>

                  <select
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    style={{
                      padding: '10px 14px',
                      border: '1px solid #cbd5e1',
                      borderRadius: '10px',
                      fontSize: '14px',
                      background: 'white',
                    }}
                  >
                    <option value="ALL">All Statuses</option>
                    <option value="ACTIVE">Active Accounts Only</option>
                    <option value="INACTIVE">Deactivated Accounts Only</option>
                  </select>
                </div>

                {/* User Table */}
                {filteredUsers.length === 0 ? (
                  <div style={{ textAlign: 'center', padding: '40px', color: '#64748b' }}>
                    No user accounts matching the criteria were found.
                  </div>
                ) : (
                  <div className="portal-table-wrapper" style={{ overflowX: 'auto' }}>
                    <table className="portal-table">
                      <thead>
                        <tr>
                          <th>ID</th>
                          <th>Email Address</th>
                          <th>Full Name / Phone</th>
                          <th>Role</th>
                          <th>Account Status</th>
                          <th>Registration Date</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {filteredUsers.map((u) => (
                          <tr key={u.id}>
                            <td>
                              <strong>#{u.id}</strong>
                            </td>
                            <td>
                              <strong>{u.email}</strong>
                            </td>
                            <td>
                              {u.fullName ? (
                                <div>
                                  <div>{u.fullName}</div>
                                  <small style={{ color: '#64748b' }}>
                                    {u.phone || 'No phone'}
                                  </small>
                                </div>
                              ) : (
                                <span style={{ color: '#94a3b8' }}>N/A (Staff)</span>
                              )}
                            </td>
                            <td>
                              <select
                                value={u.role}
                                onChange={(e) =>
                                  handleRoleChange(u.id, e.target.value)
                                }
                                style={{
                                  padding: '6px 10px',
                                  border: '1px solid #cbd5e1',
                                  borderRadius: '8px',
                                  fontSize: '13px',
                                  fontWeight: 700,
                                  background: 'white',
                                }}
                              >
                                <option value="ServiceAdvisor">ServiceAdvisor</option>
                                <option value="Mechanic">Mechanic</option>
                                <option value="InventoryOfficer">InventoryOfficer</option>
                                <option value="Accounts">Accounts</option>
                                <option value="Customer">Customer</option>
                                <option value="Administrator">Administrator</option>
                              </select>
                            </td>
                            <td>
                              <span
                                style={{
                                  display: 'inline-block',
                                  padding: '4px 10px',
                                  borderRadius: '20px',
                                  fontSize: '12px',
                                  fontWeight: 700,
                                  background: u.isActive ? '#d1fae5' : '#ffe4e6',
                                  color: u.isActive ? '#047857' : '#be123c',
                                }}
                              >
                                {u.isActive ? 'Active' : 'Deactivated'}
                              </span>
                            </td>
                            <td>
                              {new Date(u.createdAt).toLocaleDateString(undefined, {
                                year: 'numeric',
                                month: 'short',
                                day: 'numeric',
                              })}
                            </td>
                            <td>
                              <button
                                className="portal-secondary-button"
                                style={{
                                  padding: '6px 12px',
                                  fontSize: '12px',
                                  color: u.isActive ? '#be123c' : '#047857',
                                  borderColor: u.isActive ? '#fecdd3' : '#a7f3d0',
                                }}
                                onClick={() =>
                                  handleToggleStatus(u.id, u.isActive)
                                }
                              >
                                {u.isActive ? 'Deactivate' : 'Activate'}
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </section>
            )}

            {/* ========================================================
               TAB 3: REPORTS & ANALYTICS
               ======================================================== */}
            {activeTab === 'reports' && (
              <section className="portal-card">
                <h2 style={{ margin: '0 0 8px 0', fontSize: '18px', fontWeight: 800 }}>
                  Service Booking Status Breakdown
                </h2>
                <p style={{ color: '#64748b', marginBottom: '20px', fontSize: '14px' }}>
                  Distribution of customer service bookings across operational stages.
                </p>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                  {Object.entries(stats.bookingsByStatus).length === 0 ? (
                    <p style={{ color: '#64748b' }}>No bookings recorded yet.</p>
                  ) : (
                    Object.entries(stats.bookingsByStatus).map(([status, count]) => {
                      const pct = stats.totalBookings
                        ? Math.round((count / stats.totalBookings) * 100)
                        : 0
                      return (
                        <div key={status} style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '14px' }}>
                            <strong>{status}</strong>
                            <span style={{ color: '#64748b' }}>
                              {count} bookings ({pct}%)
                            </span>
                          </div>
                          <div
                            style={{
                              width: '100%',
                              height: '10px',
                              background: '#e2e8f0',
                              borderRadius: '10px',
                              overflow: 'hidden',
                            }}
                          >
                            <div
                              style={{
                                width: `${pct}%`,
                                height: '100%',
                                background: '#2563eb',
                                borderRadius: '10px',
                              }}
                            ></div>
                          </div>
                        </div>
                      )
                    })
                  )}
                </div>
              </section>
            )}
          </>
        )}
      </main>

      {/* MODAL: CREATE STAFF / USER ACCOUNT */}
      {showCreateModal && (
        <div className="modal-backdrop">
          <div className="modal-card" style={{ maxWidth: '500px', width: '90%' }}>
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '20px',
              }}
            >
              <h2 style={{ margin: 0, fontSize: '20px', fontWeight: 800 }}>
                Create Staff or User Account
              </h2>
              <button
                style={{
                  background: 'none',
                  border: 'none',
                  fontSize: '20px',
                  color: '#64748b',
                  cursor: 'pointer',
                }}
                onClick={() => setShowCreateModal(false)}
              >
                ✕
              </button>
            </div>

            {createError && (
              <div className="portal-error" style={{ marginBottom: '15px' }}>
                {createError}
              </div>
            )}

            <form onSubmit={handleCreateSubmit}>
              <div className="form-group">
                <label>Email Address *</label>
                <input
                  type="email"
                  required
                  placeholder="e.g. advisor@company.com"
                  value={createForm.email}
                  onChange={(e) =>
                    setCreateForm({ ...createForm, email: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>Password *</label>
                <input
                  type="password"
                  required
                  minLength={6}
                  placeholder="Minimum 6 characters"
                  value={createForm.password}
                  onChange={(e) =>
                    setCreateForm({ ...createForm, password: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>Assigned System Role *</label>
                <select
                  value={createForm.role}
                  onChange={(e) =>
                    setCreateForm({ ...createForm, role: e.target.value })
                  }
                >
                  <option value="ServiceAdvisor">Service Advisor</option>
                  <option value="Mechanic">Mechanic</option>
                  <option value="InventoryOfficer">Inventory Officer</option>
                  <option value="Accounts">Accounts / Cashier</option>
                  <option value="Administrator">Administrator</option>
                  <option value="Customer">Customer</option>
                </select>
              </div>

              {createForm.role === 'Customer' && (
                <>
                  <div className="form-group">
                    <label>Customer Full Name</label>
                    <input
                      type="text"
                      placeholder="e.g. John Doe"
                      value={createForm.fullName}
                      onChange={(e) =>
                        setCreateForm({ ...createForm, fullName: e.target.value })
                      }
                    />
                  </div>

                  <div className="form-group">
                    <label>Phone Number</label>
                    <input
                      type="text"
                      placeholder="e.g. +94 77 123 4567"
                      value={createForm.phone}
                      onChange={(e) =>
                        setCreateForm({ ...createForm, phone: e.target.value })
                      }
                    />
                  </div>
                </>
              )}

              <div
                style={{
                  display: 'flex',
                  justifyContent: 'flex-end',
                  gap: '12px',
                  marginTop: '24px',
                }}
              >
                <button
                  type="button"
                  className="portal-secondary-button"
                  onClick={() => setShowCreateModal(false)}
                  disabled={createLoading}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="portal-primary-button"
                  disabled={createLoading}
                >
                  {createLoading ? 'Creating...' : 'Create Account'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default AdministratorPage
