import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  authApi,
  customerApi,
  saveAuth,
} from '../services/api'

import loginBg from '../assets/login-bg.png'

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
      const authResponse =
        await authApi.register(
          form.email,
          form.password,
        )

      saveAuth(authResponse)

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
    <div
      className="login-page register-page"
      style={{
        backgroundImage: `url(${loginBg})`,
      }}
    >
      <div className="login-overlay" />

      <div className="login-shell">
        <header className="login-header">
          <div className="login-brand">
            <div className="login-brand-icon">
              ⚙
            </div>

            <div>
              <strong>
                VEHICLE SERVICE CENTER
              </strong>

              <span>
                Service • Care • Reliability
              </span>
            </div>
          </div>

          <div className="login-support">
            <span className="support-icon">
              ☎
            </span>

            <div>
              <small>Need help?</small>
              <strong>Customer Support</strong>
            </div>
          </div>
        </header>

        <main className="login-content register-content">
          <section className="login-left">
            <div className="login-card register-card">
              <div className="login-card-heading">
                <span className="login-eyebrow">
                  CREATE YOUR ACCOUNT
                </span>

                <h1>Welcome</h1>

                <p>
                  Register to manage your profile,
                  vehicles and service bookings.
                </p>
              </div>

              {error && (
                <div className="login-error">
                  <span>!</span>
                  {error}
                </div>
              )}

              <form
                className="login-form register-form"
                onSubmit={handleSubmit}
              >
                <div className="register-grid">
                  <div className="login-form-group">
                    <label htmlFor="fullName">
                      Full Name
                    </label>

                    <div className="login-input-wrapper">
                      <span className="login-input-icon">
                        ♙
                      </span>

                      <input
                        id="fullName"
                        name="fullName"
                        type="text"
                        value={form.fullName}
                        onChange={handleChange}
                        placeholder="Enter your full name"
                        required
                        maxLength="120"
                      />
                    </div>
                  </div>

                  <div className="login-form-group">
                    <label htmlFor="email">
                      Email Address
                    </label>

                    <div className="login-input-wrapper">
                      <span className="login-input-icon">
                        ✉
                      </span>

                      <input
                        id="email"
                        name="email"
                        type="email"
                        value={form.email}
                        onChange={handleChange}
                        placeholder="Enter your email"
                        autoComplete="email"
                        required
                      />
                    </div>
                  </div>

                  <div className="login-form-group">
                    <label htmlFor="phone">
                      Phone Number
                    </label>

                    <div className="login-input-wrapper">
                      <span className="login-input-icon">
                        ☎
                      </span>

                      <input
                        id="phone"
                        name="phone"
                        type="tel"
                        value={form.phone}
                        onChange={handleChange}
                        placeholder="Enter your phone number"
                        required
                        maxLength="20"
                      />
                    </div>
                  </div>

                  <div className="login-form-group">
                    <label htmlFor="address">
                      Address
                    </label>

                    <div className="login-input-wrapper register-textarea-wrapper">
                      <span className="login-input-icon textarea-icon">
                        ⌂
                      </span>

                      <textarea
                        id="address"
                        name="address"
                        value={form.address}
                        onChange={handleChange}
                        placeholder="Enter your address"
                        maxLength="250"
                        rows="3"
                      />
                    </div>
                  </div>

                  <div className="login-form-group">
                    <label htmlFor="password">
                      Password
                    </label>

                    <div className="login-input-wrapper">
                      <span className="login-input-icon">
                        ◈
                      </span>

                      <input
                        id="password"
                        name="password"
                        type="password"
                        value={form.password}
                        onChange={handleChange}
                        placeholder="Minimum 8 characters"
                        autoComplete="new-password"
                        minLength="8"
                        required
                      />
                    </div>
                  </div>

                  <div className="login-form-group">
                    <label htmlFor="confirmPassword">
                      Confirm Password
                    </label>

                    <div className="login-input-wrapper">
                      <span className="login-input-icon">
                        ◈
                      </span>

                      <input
                        id="confirmPassword"
                        name="confirmPassword"
                        type="password"
                        value={form.confirmPassword}
                        onChange={handleChange}
                        placeholder="Re-enter your password"
                        autoComplete="new-password"
                        minLength="8"
                        required
                      />
                    </div>
                  </div>
                </div>

                <button
                  className="login-submit"
                  type="submit"
                  disabled={loading}
                >
                  {loading
                    ? 'Creating account...'
                    : 'Create Account'}
                </button>
              </form>

              <div className="login-divider">
                <span />
                <p>or</p>
                <span />
              </div>

              <Link
                className="login-register-button"
                to="/login"
              >
                ← Back to Login
              </Link>
            </div>
          </section>

          <section className="login-hero-content">
            <span className="hero-label">
              PREMIUM VEHICLE CARE
            </span>

            <h2>
              Your Vehicle,
              <br />
              <span>Our Priority.</span>
            </h2>

            <div className="hero-accent-line" />

            <p className="hero-description">
              Create your customer account and
              access one convenient place for your
              profile, vehicles and service bookings.
            </p>

            <div className="service-benefits">
              <div className="service-benefit">
                <div className="benefit-icon">
                  ⚒
                </div>

                <div>
                  <strong>
                    Quality Service
                  </strong>

                  <p>
                    Reliable maintenance and
                    professional vehicle care.
                  </p>
                </div>
              </div>

              <div className="service-benefit">
                <div className="benefit-icon">
                  ◇
                </div>

                <div>
                  <strong>
                    Trusted &amp; Secure
                  </strong>

                  <p>
                    Your customer and vehicle
                    information stays protected.
                  </p>
                </div>
              </div>

              <div className="service-benefit">
                <div className="benefit-icon">
                  ◷
                </div>

                <div>
                  <strong>
                    Easy Booking
                  </strong>

                  <p>
                    Register once and manage your
                    service requests easily.
                  </p>
                </div>
              </div>
            </div>
          </section>
        </main>

        <footer className="login-footer">
          <p>
            © 2026 Vehicle Service Center.
            All rights reserved.
          </p>

          <div>
            <span>Privacy Policy</span>
            <span className="footer-divider">
              |
            </span>
            <span>Terms of Service</span>
          </div>
        </footer>
      </div>
    </div>
  )
}

export default RegisterPage