function PageHeader({ eyebrow, title, description, actions }) {
  return (
    <header className="portal-topbar">
      <div>
        {eyebrow && <span className="portal-eyebrow">{eyebrow}</span>}
        <h1>{title}</h1>
        {description && <p className="portal-page-description">{description}</p>}
      </div>
      {actions && <div className="page-header-actions">{actions}</div>}
    </header>
  )
}

export default PageHeader