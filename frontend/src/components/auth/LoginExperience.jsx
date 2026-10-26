import BrandIdentity from '../BrandIdentity'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { motion, useReducedMotion } from 'motion/react'
import { ArrowRight, Eye, EyeOff, Lock, Mail, Phone } from 'lucide-react'
import { authApi, saveAuth } from '../../services/api'
import { getRoleHomeRoute } from '../../auth/roles'

const heroVariants = {
  hidden: {},
  show: { transition: { staggerChildren: 0.09, delayChildren: 0.18 } },
}

const lineVariants = {
  hidden: { opacity: 0, y: 18 },
  show: { opacity: 1, y: 0, transition: { duration: 0.52, ease: [0.22, 1, 0.36, 1] } },
}

export default function LoginExperience() {
  const navigate = useNavigate()
  const reduceMotion = useReducedMotion()
  const [form, setForm] = useState({ email: '', password: '' })
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setLoading(true)
    try {
      const response = await authApi.login(form.email, form.password)
      saveAuth(response)
      const destination = getRoleHomeRoute(response.role)
      if (!destination) throw new Error('The account has an unsupported role. Contact an administrator.')
      navigate(destination, { replace: true })
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="cinematic-login">
      <section className="cinematic-auth" aria-labelledby="login-title">
        <header className="cinematic-brand"><BrandIdentity tone="light" /></header>

        <motion.div className="cinematic-form-wrap" initial={reduceMotion ? false : { opacity: 0, x: -14 }} animate={{ opacity: 1, x: 0 }} transition={{ duration: 0.46, ease: [0.22, 1, 0.36, 1] }}>
          <span className="cinematic-eyebrow">CUSTOMER PORTAL</span>
          <h1 id="login-title">Welcome back.</h1>
          <p>Sign in to manage your vehicles, bookings and service updates.</p>

          {error && <motion.div className="cinematic-error" role="alert" initial={reduceMotion ? false : { opacity: 0, x: -6 }} animate={{ opacity: 1, x: 0 }}><span aria-hidden="true">!</span>{error}</motion.div>}

          <form className="cinematic-form" onSubmit={handleSubmit}>
            <label htmlFor="email">Email address</label>
            <div className="cinematic-field">
              <Mail size={17} aria-hidden="true" />
              <input id="email" name="email" type="email" value={form.email} onChange={handleChange} placeholder="you@example.com" autoComplete="email" required />
            </div>

            <label htmlFor="password">Password</label>
            <div className="cinematic-field">
              <Lock size={17} aria-hidden="true" />
              <input id="password" name="password" type={showPassword ? 'text' : 'password'} value={form.password} onChange={handleChange} placeholder="Enter your password" autoComplete="current-password" required />
              <button type="button" className="password-visibility" onClick={() => setShowPassword((visible) => !visible)} aria-label={showPassword ? 'Hide password' : 'Show password'} aria-pressed={showPassword}>
                {showPassword ? <EyeOff size={17} /> : <Eye size={17} />}
              </button>
            </div>

            <motion.button className="cinematic-submit" type="submit" disabled={loading} whileTap={reduceMotion || loading ? undefined : { scale: .985 }}>
              <span>{loading ? 'Signing in…' : 'Sign in'}</span>
              {!loading && <ArrowRight size={17} aria-hidden="true" />}
            </motion.button>
          </form>

          <div className="cinematic-register"><span>New to the service center?</span><Link to="/register">Create an account <ArrowRight size={14} aria-hidden="true" /></Link></div>
        </motion.div>

        <footer className="cinematic-auth-footer"><span>© 2026 Vehicle Service Center</span><span><Phone size={13} aria-hidden="true" /> Customer support</span></footer>
      </section>

      <motion.section className="cinematic-visual" aria-label="Premium vehicle service" initial={reduceMotion ? false : { opacity: 0, scale: 1.012 }} animate={{ opacity: 1, scale: 1 }} transition={{ duration: .8, ease: [0.22, 1, 0.36, 1] }}>
        <div className="cinematic-visual-shade" />
        <motion.div className="cinematic-story" variants={reduceMotion ? undefined : heroVariants} initial={reduceMotion ? false : 'hidden'} animate="show">
          <motion.span className="cinematic-story-label" variants={lineVariants}>A CLEARER WAY TO CARE FOR YOUR VEHICLE</motion.span>
          <h2 aria-label="Every service. One clear view.">
            <motion.span variants={lineVariants}>EVERY SERVICE.</motion.span>
            <motion.span className="accent" variants={lineVariants}>ONE CLEAR VIEW.</motion.span>
          </h2>
          <motion.i className="cinematic-line" variants={{ hidden: { scaleX: 0 }, show: { scaleX: 1, transition: { duration: .56, ease: [0.22, 1, 0.36, 1] } } }} />
          <motion.p variants={lineVariants}>Book maintenance, track repairs and stay informed about your vehicle—all in one place.</motion.p>
          <motion.ol className="service-journey" variants={heroVariants}>
            {['BOOK', 'TRACK', 'HISTORY'].map((item, index) => <motion.li key={item} variants={lineVariants}><span>0{index + 1}</span><strong>{item}</strong></motion.li>)}
          </motion.ol>
        </motion.div>
      </motion.section>
    </div>
  )
}
