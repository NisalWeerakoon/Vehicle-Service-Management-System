import { motion, useReducedMotion } from 'motion/react'

export default function MetricCard({ label, value, detail, icon: Icon, tone = 'neutral', index = 0 }) {
  const reduceMotion = useReducedMotion()
  const entrances = [{ opacity: 0, x: -14 }, { opacity: 0, scale: 0.94 }, { opacity: 0, x: 14 }, { opacity: 0, y: 14 }]
  const entrance = entrances[index % entrances.length]
  return (
    <motion.article className={`metric-tile premium-metric tone-${tone}`} initial={reduceMotion ? false : entrance} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.38, delay: reduceMotion ? 0 : index * 0.045, ease: [0.22, 1, 0.36, 1] }} whileHover={reduceMotion ? undefined : { y: index % 2 ? 4 : -4 }}>
      <div className="metric-tile-top">
        <span>{label}</span>
        {Icon && <span className="metric-icon" aria-hidden="true"><Icon size={18} /></span>}
      </div>
      <strong>{value}</strong>
      <small>{detail}</small>
    </motion.article>
  )
}
