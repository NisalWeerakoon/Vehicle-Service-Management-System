import { useEffect, useState } from 'react'
import CustomerSidebar from '../components/CustomerSidebar'
import { clearAuth, inspectionApi } from '../services/api'
import { useNavigate } from 'react-router-dom'

export default function CompletedInspectionsPage() {
  const navigate = useNavigate(); const [items,setItems]=useState([]); const [error,setError]=useState(''); const [loading,setLoading]=useState(true)
  const load=async()=>{try{setLoading(true);setError('');setItems(await inspectionApi.getCompleted())}catch(err){if(err.status===401||err.status===403){clearAuth();navigate('/login');return}setError(err.message)}finally{setLoading(false)}}
  useEffect(()=>{load()},[])
  return <div className="portal-layout"><CustomerSidebar/><main className="portal-main"><header className="portal-topbar"><div><span className="portal-eyebrow">SERVICE MANAGEMENT</span><h1>Completed Inspections</h1></div><button className="portal-primary-button" onClick={load}>↻ Refresh</button></header><div className="portal-content">{error&&<div className="portal-error"><span>!</span>{error}</div>}{loading?<div className="portal-loading-card"><div className="loading-spinner"/><p>Loading completed inspections...</p></div>:items.length===0?<div className="empty-state"><h2>No completed inspections</h2><p>Completed mechanic inspections will appear here.</p></div>:<div className="job-card-list">{items.map(x=><div className="job-card-row" key={x.id}><span><strong>{x.jobCardNumber}</strong><small>{x.vehicleRegistrationNumber}</small></span><span><strong>{x.mechanicName}</strong><small>{new Date(x.completedAt).toLocaleString()}</small></span><span><strong>Problems Found</strong><small>{x.identifiedProblems}</small></span></div>)}</div>}</div></main></div>
}
