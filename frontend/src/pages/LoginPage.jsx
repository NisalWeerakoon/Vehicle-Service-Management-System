import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi, saveAuth } from '../services/api'

import loginBg from '../assets/login-bg.png'

function LoginPage() {
  const navigate = useNavigate()

  const [form, setForm] = useState({
    email: '',
    password: '',
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
    setLoading(true)

    try {
      const response = await authApi.login(
        form.email,
        form.password,
      )

      saveAuth(response)

      navigate('/profile')
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div
      className="login-page"
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

        <main className="login-content">
          <section className="login-left">
            <div className="login-card">
              <div className="login-card-heading">
                <span className="login-eyebrow">
                  CUSTOMER PORTAL
                </span>

                <h1>Welcome</h1>

                <p>
                  Login to your account to manage
                  your vehicles and service bookings.
                </p>
              </div>

              {error && (
                <div className="login-error">
                  <span>!</span>
                  {error}
                </div>
              )}

              <form
                className="login-form"
                onSubmit={handleSubmit}
              >
                <div className="login-form-group">
                  <label htmlFor="email">
                    Email Address
                  </label>

                  <div className="login-input-wrapper">
                    <span className="login-input-icon">
                      ♙
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
                      placeholder="Enter your password"
                      autoComplete="current-password"
                      required
                    />
                  </div>
                </div>

                <button
                  className="login-submit"
                  type="submit"
                  disabled={loading}
                >
                  {loading
                    ? 'Signing in...'
                    : 'Login'}
                </button>
              </form>

              <div className="login-divider">
                <span />
                <p>or</p>
                <span />
              </div>

              <Link
                className="login-register-button"
                to="/register"
              >
                <span>＋</span>
                Create an account
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
              Reliable service and maintenance
              for a smoother, safer drive. Keep
              your vehicle running at its best.
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
                    Reliable vehicle maintenance
                    and professional service.
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
                    Your account and vehicle
                    information stay protected.
                  </p>
                </div>
              </div>

              <div className="service-benefit">
                <div className="benefit-icon">
                  ◷
                </div>

                <div>
                  <strong>
                    Save Time
                  </strong>

                  <p>
                    Manage vehicles and bookings
                    conveniently in one place.
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

export default LoginPage