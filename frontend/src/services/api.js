const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ||
  'http://localhost:5000'

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
    `${API_BASE_URL}${path}`,
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