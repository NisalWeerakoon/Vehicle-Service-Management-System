import assert from 'node:assert/strict'
import test from 'node:test'
import { getRoleHomeRoute, normalizeRole, roleIsAllowed, ROLES } from '../src/auth/roles.js'

const homes = [
  ['Customer', '/profile'],
  ['ServiceAdvisor', '/service-advisor'],
  ['Mechanic', '/mechanic'],
  ['InventoryOfficer', '/inventory/dashboard'],
  ['Accounts', '/billing/invoices'],
  ['Administrator', '/admin'],
]

test('every supported role has the correct portal home', () => {
  for (const [role, route] of homes) assert.equal(getRoleHomeRoute(role), route)
})

test('legacy numeric enum values are mapped explicitly', () => {
  homes.forEach(([role], index) => assert.equal(normalizeRole(String(index)), role))
})

test('role normalization handles casing and the legacy Staff alias', () => {
  assert.equal(normalizeRole(' accounts '), ROLES.ACCOUNTS)
  assert.equal(normalizeRole('Staff'), ROLES.SERVICE_ADVISOR)
  assert.equal(normalizeRole('unknown'), '')
})

test('customer and accounts permissions remain isolated', () => {
  assert.equal(roleIsAllowed('Customer', ['Customer']), true)
  assert.equal(roleIsAllowed('Accounts', ['Customer']), false)
  assert.equal(roleIsAllowed('Customer', ['Accounts', 'Administrator']), false)
  assert.equal(roleIsAllowed('Accounts', ['Accounts', 'Administrator']), true)
})