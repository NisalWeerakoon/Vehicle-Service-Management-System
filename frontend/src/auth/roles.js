export const ROLES = Object.freeze({
  CUSTOMER: 'Customer',
  SERVICE_ADVISOR: 'ServiceAdvisor',
  MECHANIC: 'Mechanic',
  INVENTORY_OFFICER: 'InventoryOfficer',
  ACCOUNTS: 'Accounts',
  ADMINISTRATOR: 'Administrator',
})

// These values mirror CustomerBookingService.Models.UserRole and support
// legacy numeric auth data without changing backend authorization.
const LEGACY_NUMERIC_ROLES = Object.freeze({
  '0': ROLES.CUSTOMER,
  '1': ROLES.SERVICE_ADVISOR,
  '2': ROLES.MECHANIC,
  '3': ROLES.INVENTORY_OFFICER,
  '4': ROLES.ACCOUNTS,
  '5': ROLES.ADMINISTRATOR,
})

const ROLE_NAMES = new Map(
  Object.values(ROLES).map((role) => [role.toLowerCase(), role]),
)

export function normalizeRole(value) {
  if (value === null || value === undefined) return ''
  const candidate = String(value).trim()
  if (!candidate) return ''
  if (candidate.toLowerCase() === 'staff') return ROLES.SERVICE_ADVISOR
  return LEGACY_NUMERIC_ROLES[candidate] || ROLE_NAMES.get(candidate.toLowerCase()) || ''
}

export function getRoleHomeRoute(value) {
  switch (normalizeRole(value)) {
    case ROLES.CUSTOMER: return '/profile'
    case ROLES.SERVICE_ADVISOR: return '/service-advisor'
    case ROLES.MECHANIC: return '/mechanic'
    case ROLES.INVENTORY_OFFICER: return '/inventory/dashboard'
    case ROLES.ACCOUNTS: return '/billing/invoices'
    case ROLES.ADMINISTRATOR: return '/admin'
    default: return null
  }
}

export function roleIsAllowed(value, allowedRoles = []) {
  const role = normalizeRole(value)
  return Boolean(role) && allowedRoles.some((allowed) => normalizeRole(allowed) === role)
}