export default function BrandLogo({ className = '', label = 'Vehicle Service Center' }) {
  const classes = ['brand-logo', className].filter(Boolean).join(' ')

  return (
    <span className={classes} role={label ? 'img' : undefined} aria-label={label || undefined} aria-hidden={label ? undefined : true}>
      <svg
        className="brand-logo__symbol"
        viewBox="1.5 3.5 29 17"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        preserveAspectRatio="xMidYMid meet"
        aria-hidden="true"
      >
        <path d="M3.5 8.6h6M2.5 12h6.6M3.5 15.4h6M22.5 8.6h6M22.9 12h6.6M22.5 15.4h6" stroke="currentColor" strokeWidth="1.55" strokeLinecap="round" />
        <circle cx="16" cy="12" r="7.4" stroke="currentColor" strokeWidth="1.65" />
        <path d="m11.8 8.8 4.2 7.4 4.2-7.4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
        <path d="M16 4.6v3.2M12.9 6l1.55 2.35M19.1 6l-1.55 2.35" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" />
        <circle cx="16" cy="12" r=".95" fill="currentColor" />
      </svg>
    </span>
  )
}