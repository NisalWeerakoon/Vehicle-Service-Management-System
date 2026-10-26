import { motion, useReducedMotion } from 'motion/react'

export default function SectionCard({ className = '', children, ...props }) {
  const reduceMotion = useReducedMotion()
  return (
    <motion.section className={`portal-card premium-section-card ${className}`.trim()} initial={reduceMotion ? false : { opacity: 0, scale: 0.985 }} whileInView={{ opacity: 1, scale: 1 }} viewport={{ once: true, amount: 0.12 }} transition={{ duration: reduceMotion ? 0 : 0.4, ease: [0.22, 1, 0.36, 1] }} {...props}>
      {children}
    </motion.section>
  )
}
