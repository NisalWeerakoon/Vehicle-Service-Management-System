import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { AnimatePresence, motion, useReducedMotion } from 'motion/react'
import { ArrowLeft, ArrowRight, Lock, Mail, MapPin, Phone, User } from 'lucide-react'
import BrandIdentity from '../components/BrandIdentity'
import { authApi, customerApi, saveAuth } from '../services/api'

function RegisterPage() {
  const navigate = useNavigate()
  const reduceMotion = useReducedMotion()
  const [form, setForm] = useState({
    fullName: '',
    email: '',
    phone: '',
    address: '',
    password: '',
    confirmPassword: '',
    role: 'Customer',
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')

    if (form.password.length < 8) {
      setError('Password must contain at least 8 characters.')
      return
    }

    if (form.password !== form.confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setLoading(true)

    try {
      const authResponse = await authApi.register(form.email, form.password, form.role)
      saveAuth(authResponse)

      try {
        await customerApi.createMyProfile({
          fullName: form.fullName,
          email: form.email,
          phone: form.phone,
          address: form.address.trim() === '' ? null : form.address,
        })
      } catch {
        // Profile creation is optional if the user profile already exists.
      }

      if (authResponse.role === 'ServiceAdvisor' || authResponse.role === 'Staff') {
        navigate('/service-advisor')
      } else if (authResponse.role === 'Mechanic') {
        navigate('/mechanic')
      } else {
        navigate('/profile')
      }
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setLoading(false)
    }
  }

  const entrance = reduceMotion ? false : { opacity: 0, x: -18 }

  return (
    <div className="register-experience">
      <header className="register-header">
        <div className="register-brand"><BrandIdentity tone="light" /></div>
        <div className="register-support">
          <Phone size={17} aria-hidden="true" />
          <div><span>Need help?</span><strong>Customer Support</strong></div>
        </div>
      </header>

      <main className="register-split">
        <motion.section
          className="register-workspace"
          aria-labelledby="register-title"
          initial={entrance}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: .58, ease: [0.22, 1, 0.36, 1] }}
        >
          <div className="register-workspace-inner">
            <div className="register-intro">
              <span className="register-eyebrow">CREATE YOUR ACCOUNT</span>
              <h1 id="register-title">Your journey starts here.</h1>
              <p>Create your customer account to manage vehicles, arrange service bookings and follow every update in one clear place.</p>
            </div>

            <AnimatePresence initial={false}>
              {error && (
                <motion.div
                  className="register-error"
                  role="alert"
                  initial={reduceMotion ? false : { opacity: 0, y: -6 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0 }}
                  transition={{ duration: .2 }}
                >
                  <span aria-hidden="true">!</span>
                  <p>{error}</p>
                </motion.div>
              )}
            </AnimatePresence>

            <form className="register-premium-form" onSubmit={handleSubmit}>
              <div className="register-field">
                <label htmlFor="fullName">Full name</label>
                <div className="register-control">
                  <User size={17} aria-hidden="true" />
                  <input id="fullName" name="fullName" type="text" value={form.fullName} onChange={handleChange} placeholder="Enter your full name" autoComplete="name" maxLength="120" required />
                </div>
              </div>

              <div className="register-field">
                <label htmlFor="email">Email address</label>
                <div className="register-control">
                  <Mail size={17} aria-hidden="true" />
                  <input id="email" name="email" type="email" value={form.email} onChange={handleChange} placeholder="you@example.com" autoComplete="email" required />
                </div>
              </div>

              <div className="register-field">
                <label htmlFor="phone">Phone number</label>
                <div className="register-control">
                  <Phone size={17} aria-hidden="true" />
                  <input id="phone" name="phone" type="tel" value={form.phone} onChange={handleChange} placeholder="Enter your phone number" autoComplete="tel" maxLength="20" required />
                </div>
              </div>

              <div className="register-field">
                <label htmlFor="address">Address</label>
                <div className="register-control">
                  <MapPin size={17} aria-hidden="true" />
                  <input id="address" name="address" type="text" value={form.address} onChange={handleChange} placeholder="Enter your address" autoComplete="street-address" maxLength="250" />
                </div>
              </div>

              <div className="register-field">
                <label htmlFor="password">Password</label>
                <div className="register-control">
                  <Lock size={17} aria-hidden="true" />
                  <input id="password" name="password" type="password" value={form.password} onChange={handleChange} placeholder="Minimum 8 characters" autoComplete="new-password" minLength="8" required />
                </div>
              </div>

              <div className="register-field">
                <label htmlFor="confirmPassword">Confirm password</label>
                <div className="register-control">
                  <Lock size={17} aria-hidden="true" />
                  <input id="confirmPassword" name="confirmPassword" type="password" value={form.confirmPassword} onChange={handleChange} placeholder="Re-enter your password" autoComplete="new-password" minLength="8" required />
                </div>
              </div>

              <motion.button
                className="register-submit"
                type="submit"
                disabled={loading}
                whileHover={reduceMotion || loading ? undefined : { y: -1 }}
                whileTap={reduceMotion || loading ? undefined : { scale: .99 }}
              >
                <span>{loading ? 'Creating account…' : 'Create account'}</span>
                {!loading && <ArrowRight size={18} aria-hidden="true" />}
              </motion.button>
            </form>

            <div className="register-login-row">
              <span>Already have an account?</span>
              <Link to="/login"><ArrowLeft size={15} aria-hidden="true" /> Back to login</Link>
            </div>
          </div>
        </motion.section>

        <motion.aside
          className="register-visual"
          aria-label="Premium automotive service"
          initial={reduceMotion ? false : { opacity: 0, scale: 1.012 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: .8, ease: [0.22, 1, 0.36, 1] }}
        >
          <div className="register-visual-shade" />
          <div className="register-visual-copy">
            <span>PREMIUM VEHICLE CARE</span>
            <h2>DRIVE FORWARD.<strong>WE’LL HANDLE THE REST.</strong></h2>
            <p>One account for every booking, update and service milestone.</p>
            <ol className="register-journey">
              <li><b>01</b><span>BOOK</span></li>
              <li><b>02</b><span>TRACK</span></li>
              <li><b>03</b><span>HISTORY</span></li>
            </ol>
          </div>
        </motion.aside>
      </main>

      <footer className="register-footer">
        <span>© 2026 Vehicle Service Center</span>
        <div><span>Privacy Policy</span><span>Terms of Service</span></div>
      </footer>
    </div>
  )
}

export default RegisterPage