import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import MechanicSidebar from '../components/MechanicSidebar'
import { clearAuth, jobCardApi, repairNoteApi, repairTaskApi } from '../services/api'

const emptyTask = { taskTitle: '', taskDescription: '' }

export default function RepairTasksPage() {
  const { jobCardId } = useParams()
  const navigate = useNavigate()
  const [job, setJob] = useState(null)
  const [tasks, setTasks] = useState([])
  const [notes, setNotes] = useState([])
  const [taskForm, setTaskForm] = useState(emptyTask)
  const [editingId, setEditingId] = useState(null)
  const [note, setNote] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const load = async () => {
    try {
      setLoading(true); setError('')
      const [jobData, taskData, noteData] = await Promise.all([
        jobCardApi.getById(jobCardId),
        repairTaskApi.getByJob(jobCardId),
        repairNoteApi.getByJob(jobCardId),
      ])
      setJob(jobData); setTasks(taskData); setNotes(noteData)
    } catch (err) {
      if (err.status === 401 || err.status === 403) { clearAuth(); navigate('/login'); return }
      setError(err.message)
    } finally { setLoading(false) }
  }

  useEffect(() => { load() }, [jobCardId])

  const saveTask = async (e) => {
    e.preventDefault(); setError(''); setMessage('')
    if (taskForm.taskTitle.trim().length < 3 || taskForm.taskDescription.trim().length < 3) {
      setError('Task title and description are required and must contain at least 3 characters.'); return
    }
    try {
      setSaving(true)
      if (editingId) await repairTaskApi.update(editingId, taskForm)
      else await repairTaskApi.create({ jobCardId: Number(jobCardId), ...taskForm })
      setTaskForm(emptyTask); setEditingId(null); setMessage(editingId ? 'Repair task updated successfully.' : 'Repair task added successfully.')
      await load()
    } catch (err) { setError(err.message) } finally { setSaving(false) }
  }

  const editTask = (task) => {
    setEditingId(task.id); setTaskForm({ taskTitle: task.taskTitle, taskDescription: task.taskDescription }); setMessage(''); setError('')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const completeTask = async (id) => {
    if (!window.confirm('Mark this repair task as completed?')) return
    try { setSaving(true); setError(''); await repairTaskApi.complete(id); setMessage('Repair task marked as completed.'); await load() }
    catch (err) { setError(err.message) } finally { setSaving(false) }
  }

  const addNote = async (e) => {
    e.preventDefault(); setError(''); setMessage('')
    if (note.trim().length < 3) { setError('Repair note is required and must contain at least 3 characters.'); return }
    try { setSaving(true); await repairNoteApi.create({ jobCardId: Number(jobCardId), note }); setNote(''); setMessage('Repair note recorded successfully.'); await load() }
    catch (err) { setError(err.message) } finally { setSaving(false) }
  }

  if (loading) return <div className="portal-layout"><MechanicSidebar /><main className="portal-main"><div className="portal-content"><div className="portal-loading-card"><div className="loading-spinner" /><p>Loading repair workspace...</p></div></div></main></div>

  return <div className="portal-layout"><MechanicSidebar /><main className="portal-main">
    <header className="portal-topbar"><div><span className="portal-eyebrow">MECHANIC INTERFACE</span><h1>Repair Tasks & Notes</h1></div><button className="portal-secondary-button" onClick={() => navigate('/mechanic/my-jobs')}>← My Jobs</button></header>
    <div className="portal-content">
      {error && <div className="portal-error"><span>!</span>{error}</div>}
      {message && <div className="portal-success">✓ {message}</div>}
      {job && <section className="checkin-card"><span className="profile-welcome-label">JOB CARD</span><h2>{job.jobCardNumber}</h2><p><strong>Vehicle:</strong> {job.vehicleRegistrationNumber}</p><p><strong>Reported problem:</strong> {job.reportedProblems}</p><p><strong>Job Status:</strong> {job.status}</p></section>}

      <section className="checkin-card" style={{ marginTop: '24px' }}>
        <h2>{editingId ? 'Update Repair Task' : 'Add Repair Task'}</h2>
        <p>Create and maintain the repair or service work required for this job.</p>
        <form onSubmit={saveTask}>
          <div className="form-group"><label htmlFor="taskTitle">Task Title *</label><input id="taskTitle" value={taskForm.taskTitle} onChange={e => setTaskForm({ ...taskForm, taskTitle: e.target.value })} disabled={saving} placeholder="e.g. Replace front brake pads" /></div>
          <div className="form-group"><label htmlFor="taskDescription">Task Description *</label><textarea id="taskDescription" rows="4" value={taskForm.taskDescription} onChange={e => setTaskForm({ ...taskForm, taskDescription: e.target.value })} disabled={saving} placeholder="Describe the repair or service work required..." /></div>
          <div className="button-row"><button className="portal-primary-button" disabled={saving}>{saving ? 'Saving...' : editingId ? 'Update Task' : 'Add Task'}</button>{editingId && <button className="portal-secondary-button" type="button" onClick={() => { setEditingId(null); setTaskForm(emptyTask) }}>Cancel Edit</button>}</div>
        </form>
      </section>

      <section className="checkin-card" style={{ marginTop: '24px' }}>
        <h2>Repair Tasks</h2>
        <p><strong>{tasks.filter(x => !x.isCompleted).length}</strong> outstanding · <strong>{tasks.filter(x => x.isCompleted).length}</strong> completed</p>
        {tasks.length === 0 ? <p>No repair tasks have been added yet.</p> : <div className="job-card-list">{tasks.map(task => <div className="job-card-row" key={task.id}>
          <span><strong>{task.taskTitle}</strong><small>{task.taskDescription}</small></span>
          <span><strong>{task.isCompleted ? 'Completed' : 'Outstanding'}</strong><small>{task.isCompleted && task.completedAt ? new Date(task.completedAt).toLocaleString() : 'Work pending'}</small></span>
          <span>{!task.isCompleted && <><button className="portal-secondary-button" type="button" onClick={() => editTask(task)} disabled={saving}>Edit</button><button className="portal-primary-button" type="button" onClick={() => completeTask(task.id)} disabled={saving}>Mark Complete</button></>}</span>
        </div>)}</div>}
      </section>

      <section className="checkin-card" style={{ marginTop: '24px' }}>
        <h2>Repair Notes</h2>
        <p>Record observations, work details, or other maintenance notes against this job.</p>
        <form onSubmit={addNote}><div className="form-group"><label htmlFor="repairNote">Note *</label><textarea id="repairNote" rows="4" value={note} onChange={e => setNote(e.target.value)} disabled={saving} placeholder="Enter a repair note..." /></div><button className="portal-primary-button" disabled={saving}>{saving ? 'Saving...' : 'Add Note'}</button></form>
        {notes.length > 0 && <div className="job-card-list" style={{ marginTop: '20px' }}>{notes.map(item => <div className="job-card-row" key={item.id}><span><strong>{item.mechanicName}</strong><small>{new Date(item.createdAt).toLocaleString()}</small></span><span style={{ flex: 2 }}><small>{item.note}</small></span></div>)}</div>}
      </section>
    </div>
  </main></div>
}
