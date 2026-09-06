const CUSTOMER_BOOKING_API =
  import.meta.env.VITE_API_BASE_URL ||
  'http://localhost:5001'

const JOB_MAINTENANCE_API =
  import.meta.env.VITE_JOB_MAINTENANCE_API_BASE_URL ||
  'http://localhost:5002'


export function getToken() {
  return localStorage.getItem('token')
}


export function saveAuth(authResponse) {
  localStorage.setItem('token', authResponse.token)

  localStorage.setItem(
    'userId',
    authResponse.userId,
  )

  localStorage.setItem(
    'email',
    authResponse.email,
  )

  localStorage.setItem(
    'role',
    authResponse.role,
  )
}


export function clearAuth() {
  localStorage.removeItem('token')
  localStorage.removeItem('userId')
  localStorage.removeItem('email')
  localStorage.removeItem('role')
}


export function isAuthenticated() {
  return Boolean(getToken())
}


/*
 * Generic request for CustomerBookingService
 */
async function request(path, options = {}) {
  const token = getToken()

  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(
    `${CUSTOMER_BOOKING_API}${path}`,
    {
      ...options,
      headers,
    },
  )

  const data = await response
    .json()
    .catch(() => null)

  if (!response.ok) {
    const error = new Error(
      data?.message ||
      'Something went wrong while contacting the server.',
    )

    error.status = response.status
    error.data = data

    throw error
  }

  return data
}


/*
 * Generic request for JobMaintenanceService
 */
async function jobMaintenanceRequest(path, options = {}) {
  const token = getToken()

  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(
    `${JOB_MAINTENANCE_API}${path}`,
    {
      ...options,
      headers,
    },
  )

  const data = await response
    .json()
    .catch(() => null)

  if (!response.ok) {
    const error = new Error(
      data?.message ||
      'Something went wrong while contacting the server.',
    )

    error.status = response.status
    error.data = data

    throw error
  }

  return data
}


/* =========================================================
   AUTH API
   CustomerBookingService
   ========================================================= */

export const authApi = {
  register(email, password) {
    return request('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({
        email,
        password,
      }),
    })
  },

  login(email, password) {
    return request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({
        email,
        password,
      }),
    })
  },

  logout() {
    return request('/api/auth/logout', {
      method: 'POST',
    })
  },

  me() {
    return request('/api/auth/me')
  },
}


/* =========================================================
   CUSTOMER API
   CustomerBookingService
   ========================================================= */

export const customerApi = {
  getMyProfile() {
    return request('/api/customers/me')
  },

  createMyProfile(profile) {
    return request('/api/customers/me', {
      method: 'POST',
      body: JSON.stringify(profile),
    })
  },

  updateMyProfile(profile) {
    return request('/api/customers/me', {
      method: 'PUT',
      body: JSON.stringify(profile),
    })
  },
}


/* =========================================================
   VEHICLE API
   CustomerBookingService
   ========================================================= */

export const vehicleApi = {
  getMyVehicles() {
    return request('/api/vehicles/me')
  },

  getMyVehicle(id) {
    return request(`/api/vehicles/me/${id}`)
  },

  createMyVehicle(vehicle) {
    return request('/api/vehicles/me', {
      method: 'POST',
      body: JSON.stringify(vehicle),
    })
  },

  updateMyVehicle(id, vehicle) {
    return request(`/api/vehicles/me/${id}`, {
      method: 'PUT',
      body: JSON.stringify(vehicle),
    })
  },
}


/* =========================================================
   BOOKING API
   CustomerBookingService
   ========================================================= */

export const bookingApi = {
  getMyBookings() {
    return request('/api/bookings/me')
  },

  getMyBooking(id) {
    return request(`/api/bookings/me/${id}`)
  },

  createMyBooking(booking) {
    return request('/api/bookings/me', {
      method: 'POST',
      body: JSON.stringify(booking),
    })
  },

  updateMyBooking(id, booking) {
    return request(`/api/bookings/me/${id}`, {
      method: 'PUT',
      body: JSON.stringify(booking),
    })
  },

  cancelMyBooking(id) {
    return request(`/api/bookings/me/${id}/cancel`, {
      method: 'PATCH',
    })
  },

  getStaffCheckInReady() {
    return request('/api/bookings/staff/check-in-ready')
  },
}


/* =========================================================
   CHECK-IN API
   CustomerBookingService
   ========================================================= */

export const checkInApi = {
  checkInBooking(bookingId, data) {
    return request(
      `/api/check-ins/booking/${bookingId}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      },
    )
  },

  checkInWalkIn(data) {
    return request(
      '/api/check-ins/walk-in',
      {
        method: 'POST',
        body: JSON.stringify(data),
      },
    )
  },
}


/* =========================================================
   JOB CARD API
   JobMaintenanceService
   ========================================================= */

export const jobCardApi = {
  create(data) {
    return jobMaintenanceRequest(
      '/api/jobs',
      {
        method: 'POST',
        body: JSON.stringify(data),
      },
    )
  },

  getAll() {
    return jobMaintenanceRequest('/api/jobs')
  },

  getById(id) {
    return jobMaintenanceRequest(
      `/api/jobs/${id}`,
    )
  },

  getByCheckIn(checkInId) {
    return jobMaintenanceRequest(
      `/api/jobs/check-in/${checkInId}`,
    )
  },
}


/* =========================================================
   MECHANIC API
   CustomerBookingService
   ========================================================= */

export const mechanicApi = {
  getActiveMechanics() {
    return request('/api/auth/mechanics')
  },
}


/* =========================================================
   MECHANIC ASSIGNMENT API
   JobMaintenanceService
   ========================================================= */

export const mechanicAssignmentApi = {
  assign(data) {
    return jobMaintenanceRequest(
      '/api/mechanic-assignments',
      {
        method: 'POST',
        body: JSON.stringify(data),
      },
    )
  },

  getByJob(jobCardId) {
    return jobMaintenanceRequest(
      `/api/mechanic-assignments/job/${jobCardId}`,
    )
  },

  getMyJobs() {
    return jobMaintenanceRequest(
      '/api/mechanic-assignments/my-jobs',
    )
  },
}

/* =========================================================
   INSPECTION API
   JobMaintenanceService
   ========================================================= */

export const inspectionApi = {
  save(data) {
    return jobMaintenanceRequest('/api/inspections', { method: 'POST', body: JSON.stringify(data) })
  },
  getMy() {
    return jobMaintenanceRequest('/api/inspections/my')
  },
  getByJob(jobCardId) {
    return jobMaintenanceRequest(`/api/inspections/job/${jobCardId}`)
  },
  getCompleted() {
    return jobMaintenanceRequest('/api/inspections/completed')
  },
  complete(id) {
    return jobMaintenanceRequest(`/api/inspections/${id}/complete`, { method: 'POST' })
  },
}
