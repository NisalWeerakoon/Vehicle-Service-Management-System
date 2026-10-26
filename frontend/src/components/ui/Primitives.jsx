import { motion, useReducedMotion } from 'motion/react'
import { Check, CircleAlert, Inbox, LoaderCircle } from 'lucide-react'

export function AnimatedButton({ children, className = 'portal-primary-button', loading = false, ...props }) {
  const reduceMotion = useReducedMotion()
  return (
    <motion.button className={className} whileTap={reduceMotion || props.disabled ? undefined : { scale: 0.98 }} disabled={loading || props.disabled} {...props}>
      {loading && <LoaderCircle className="button-spinner" size={17} aria-hidden="true" />}
      <span>{children}</span>
    </motion.button>
  )
}

export function StatusBadge({ children, tone = 'neutral' }) {
  const Icon = tone === 'success' ? Check : tone === 'danger' || tone === 'warning' ? CircleAlert : null
  return <span className={`status-badge status-${tone}`}>{Icon && <Icon size={13} aria-hidden="true" />}{children}</span>
}

export function EmptyState({ title, description, action }) {
  return <div className="empty-state premium-empty-state"><Inbox size={28} aria-hidden="true" /><h2>{title}</h2>{description && <p>{description}</p>}{action}</div>
}

export function LoadingSkeleton({ rows = 3, label = 'Loading content' }) {
  return <div className="skeleton-stack" role="status" aria-label={label}>{Array.from({ length: rows }, (_, index) => <span className="skeleton-line" key={index} />)}</div>
}

export function FormField({ id, label, hint, error, required, children }) {
  return <div className={`form-group premium-form-field${error ? ' has-error' : ''}`}><label htmlFor={id}>{label}{required && <span aria-hidden="true"> *</span>}</label>{children}{error ? <small id={`${id}-error`} className="field-error" role="alert">{error}</small> : hint && <small id={`${id}-hint`} className="field-hint">{hint}</small>}</div>
}

export function ProgressTimeline({ steps = [], activeIndex = 0 }) {
  return <ol className="progress-timeline" aria-label="Progress">{steps.map((step, index) => <li className={index < activeIndex ? 'complete' : index === activeIndex ? 'active' : ''} key={step}><span aria-hidden="true">{index < activeIndex ? <Check size={14} /> : index + 1}</span><strong>{step}</strong></li>)}</ol>
}
