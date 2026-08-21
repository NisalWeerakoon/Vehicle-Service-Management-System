import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  authApi,
  customerApi,
  saveAuth,
} from '../services/api'

function RegisterPage() {
  const navigate = useNavigate()

  const [form, setForm] = useState({
    fullName: '',
    email: '',
    phone: '',
    address: '',
    password: '',
    confirmPassword: '',
  })

  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  function handleChange(event) {
    const { name, value } = event.target

    setForm((current) => ({
      ...current,
      [name]: value,
    }))
  }

  async function handleSubmit(event) {
    event.preventDefault()

    setError('')

    if (form.password.length < 8) {
      setError(
        'Password must contain at least 8 characters.',
      )
      return
    }

    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setLoading(true)

    try {
      // Step 1 - Authentication account
      const authResponse =
        await authApi.register(
          form.email,
          form.password,
        )

      saveAuth(authResponse)

      // Step 2 - Customer profile
      await customerApi.createMyProfile({
        fullName: form.fullName,
        email: form.email,
        phone: form.phone,
        address:
          form.address.trim() === ''
            ? null
            : form.address,
      })

      navigate('/profile')
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page-container">
      <div className="auth-card large-card">
        <div className="brand-section">
          <h1>Vehicle Service Center</h1>
          <p>Create your customer account.</p>
        </div>

        <h2>Customer Registration</h2>

        {error && (
          <div className="alert error-alert">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="fullName">
              Full Name
            </label>

            <input
              id="fullName"
              name="fullName"
              type="text"
              value={form.fullName}
              onChange={handleChange}
              required
              maxLength="120"
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">
              Email
            </label>

            <input
              id="email"
              name="email"
              type="email"
              value={form.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="phone">
              Phone Number
            </label>

            <input
              id="phone"
              name="phone"
              type="tel"
              value={form.phone}
              onChange={handleChange}
              required
              maxLength="20"
            />
          </div>

          <div className="form-group">
            <label htmlFor="address">
              Address
            </label>

            <textarea
              id="address"
              name="address"
              value={form.address}
              onChange={handleChange}
              maxLength="250"
              rows="3"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">
              Password
            </label>

            <input
              id="password"
              name="password"
              type="password"
              value={form.password}
              onChange={handleChange}
              minLength="8"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">
              Confirm Password
            </label>

            <input
              id="confirmPassword"
              name="confirmPassword"
              type="password"
              value={form.confirmPassword}
              onChange={handleChange}
              minLength="8"
              required
            />
          </div>

          <button
            className="primary-button"
            type="submit"
            disabled={loading}
          >
            {loading
              ? 'Creating account...'
              : 'Create Account'}
          </button>
        </form>

        <p className="switch-text">
          Already registered?{' '}
          <Link to="/login">Login here</Link>
        </p>
      </div>
    </div>
  )
}

export default RegisterPage