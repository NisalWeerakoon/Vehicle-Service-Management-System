import BrandLogo from './BrandLogo'

export default function BrandIdentity({ tone = 'dark', className = '' }) {
  const classes = ['brand-identity', `brand-identity--${tone}`, className]
    .filter(Boolean)
    .join(' ')

  return (
    <div className={classes} aria-label="Vehicle Service Center">
      <BrandLogo label="" />
      <span className="brand-identity__name" aria-hidden="true">
        <strong>VEHICLE</strong>
        <span>SERVICE CENTER</span>
      </span>
    </div>
  )
}