import { useEffect, useState } from 'react'
import { Boxes, Edit3, PackagePlus, Search, Trash2 } from 'lucide-react'
import InventorySidebar from '../components/InventorySidebar'
import { clearAuth, sparePartApi } from '../services/api'
import { useNavigate } from 'react-router-dom'

const blankForm = { name: '', description: '', quantity: '', lowStockThreshold: '', unitPrice: '' }

function SparePartsPage() {
  const navigate = useNavigate()
  const [parts, setParts] = useState([])
  const [search, setSearch] = useState('')
  const [form, setForm] = useState(blankForm)
  const [editingId, setEditingId] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const loadParts = async (term = search) => {
    try {
      setLoading(true)
      setError('')
      setParts(await sparePartApi.getAll(term))
    } catch (err) {
      if (err.status === 401 || err.status === 403) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(err.message || 'Unable to load spare parts.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { loadParts('') }, [])

  const validate = () => {
    const quantity = Number(form.quantity)
    const lowStockThreshold = Number(form.lowStockThreshold)
    const unitPrice = Number(form.unitPrice)
    if (form.name.trim().length < 2) return 'Name must contain at least 2 characters.'
    if (form.description.trim().length < 2) return 'Description must contain at least 2 characters.'
    if (!Number.isInteger(quantity) || quantity < 0) return 'Quantity must be a whole number of zero or more.'
    if (!Number.isInteger(lowStockThreshold) || lowStockThreshold < 0) return 'Low-stock threshold must be a whole number of zero or more.'
    if (!Number.isFinite(unitPrice) || unitPrice < 0) return 'Unit price must be zero or more.'
    return ''
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const validationError = validate()
    if (validationError) { setError(validationError); return }

    try {
      setSaving(true); setError(''); setSuccess('')
      const payload = { ...form, name: form.name.trim(), description: form.description.trim(), quantity: Number(form.quantity), lowStockThreshold: Number(form.lowStockThreshold), unitPrice: Number(form.unitPrice) }
      if (editingId) {
        await sparePartApi.update(editingId, payload)
        setSuccess('Spare part updated successfully.')
      } else {
        await sparePartApi.create(payload)
        setSuccess('Spare part added successfully.')
      }
      setForm(blankForm); setEditingId(null)
      await loadParts()
      window.dispatchEvent(new Event('inventory-stock-changed'))
    } catch (err) {
      setError(err.message || 'Unable to save the spare part.')
    } finally { setSaving(false) }
  }

  const editPart = (part) => {
    setEditingId(part.id)
    setForm({ name: part.name, description: part.description, quantity: String(part.quantity), lowStockThreshold: String(part.lowStockThreshold), unitPrice: String(part.unitPrice) })
    setError(''); setSuccess('')
  }

  const adjustStock = async (part) => {
    const rawValue = window.prompt(`Adjust stock for ${part.name}. Enter a positive or negative whole number. Current quantity: ${part.quantity}`, '0')
    if (rawValue === null) return
    const adjustment = Number(rawValue)
    if (!Number.isInteger(adjustment)) { setError('Stock adjustment must be a whole number.'); return }
    if (part.quantity + adjustment < 0) { setError('This adjustment would result in negative stock.'); return }
    try {
      setError(''); setSuccess('')
      await sparePartApi.adjustStock(part.id, adjustment)
      setSuccess('Stock adjusted successfully.')
      await loadParts()
      window.dispatchEvent(new Event('inventory-stock-changed'))
    } catch (err) { setError(err.message || 'Unable to adjust stock.') }
  }

  const deletePart = async (part) => {
    if (!window.confirm(`Delete ${part.name}? This cannot be undone.`)) return
    try {
      setError(''); setSuccess('')
      await sparePartApi.remove(part.id)
      setSuccess('Spare part deleted successfully.')
      await loadParts()
    } catch (err) { setError(err.message || 'Unable to delete the spare part.') }
  }

  return (
    <div className="portal-layout">
      <InventorySidebar />
      <main className="portal-main">
        <header className="portal-topbar">
          <div><span className="portal-eyebrow">INVENTORY MANAGEMENT</span><h1>Spare Parts Catalog</h1></div>
          <button className="portal-primary-button" onClick={() => { setEditingId(null); setForm(blankForm); setError('') }}><PackagePlus size={16} /> Add Spare Part</button>
        </header>
        <div className="portal-content inventory-content">
          {error && <div className="portal-error">{error}</div>}
          {success && <div className="checkin-alert success">{success}</div>}
          <section className="portal-card inventory-form-card">
            <h2>{editingId ? `Edit Spare Part #${editingId}` : 'Add Spare Part'}</h2>
            <form className="inventory-form" onSubmit={handleSubmit}>
              <label>Name<input value={form.name} maxLength="200" onChange={(e) => setForm({ ...form, name: e.target.value })} /></label>
              <label>Description<input value={form.description} maxLength="1000" onChange={(e) => setForm({ ...form, description: e.target.value })} /></label>
              <label>Quantity<input type="number" min="0" step="1" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: e.target.value })} /></label>
              <label>Low-stock threshold<input type="number" min="0" step="1" value={form.lowStockThreshold} onChange={(e) => setForm({ ...form, lowStockThreshold: e.target.value })} /></label>
              <label>Unit price<input type="number" min="0" step="0.01" value={form.unitPrice} onChange={(e) => setForm({ ...form, unitPrice: e.target.value })} /></label>
              <div className="inventory-form-actions"><button className="portal-primary-button" disabled={saving}>{saving ? 'Saving...' : editingId ? 'Update Part' : 'Create Part'}</button>{editingId && <button type="button" className="portal-secondary-button" onClick={() => { setEditingId(null); setForm(blankForm) }}>Cancel</button>}</div>
            </form>
          </section>
          <section className="portal-card">
            <div className="inventory-list-header"><div><h2>Catalog</h2><p>{parts.length} part{parts.length === 1 ? '' : 's'} found</p></div><form className="inventory-search" onSubmit={(e) => { e.preventDefault(); loadParts() }}><Search size={17} /><input placeholder="Search name or ID" value={search} onChange={(e) => setSearch(e.target.value)} /><button className="portal-secondary-button">Search</button></form></div>
            {loading ? <div className="portal-loading-card"><div className="loading-spinner" /><p>Loading spare parts...</p></div> : parts.length === 0 ? <div className="modern-empty-state"><div className="modern-empty-icon"><Boxes size={32} /></div><h2>No spare parts found</h2><p>Add a spare part or change your search term.</p></div> : <div className="portal-table-wrapper"><table className="portal-table"><thead><tr><th>Part ID</th><th>Name</th><th>Description</th><th>Quantity</th><th>Threshold</th><th>Status</th><th>Unit Price</th><th>Actions</th></tr></thead><tbody>{parts.map((part) => <tr key={part.id}><td>#{part.id}</td><td><strong>{part.name}</strong></td><td>{part.description}</td><td>{part.quantity}</td><td>{part.lowStockThreshold}</td><td><strong className={part.isLowStock ? 'portal-error-text' : ''}>{part.isLowStock ? 'LOW STOCK' : 'NORMAL'}</strong></td><td>{Number(part.unitPrice).toFixed(2)}</td><td><div className="inventory-actions"><button className="portal-secondary-button" onClick={() => editPart(part)}><Edit3 size={14} /> Edit</button><button className="portal-secondary-button" onClick={() => adjustStock(part)}>Adjust stock</button><button className="inventory-delete-button" onClick={() => deletePart(part)} title="Delete part"><Trash2 size={15} /></button></div></td></tr>)}</tbody></table></div>}
          </section>
        </div>
      </main>
    </div>
  )
}

export default SparePartsPage
