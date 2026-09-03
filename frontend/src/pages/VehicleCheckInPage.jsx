import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import {
  bookingApi,
  checkInApi,
  clearAuth,
} from '../services/api'

function VehicleCheckInPage() {
  const navigate = useNavigate()
  const [mode, setMode] = useState('booking')
  const [bookings, setBookings] = useState([])
  const [selectedBookingId, setSelectedBookingId] = useState('')
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(null)

  const [bookingForm, setBookingForm] = useState({
    mileage: '',
    reportedProblems: '',
  })

  const [walkInForm, setWalkInForm] = useState({
    fullName: '',
    email: '',
    phone: '',
    address: '',
    registrationNumber: '',
    make: '',
    model: '',
    year: '',
    fuelType: '',
    mileage: '',
    reportedProblems: '',
  })

  useEffect(() => {
    async function loadBookings() {
      try {
        const data = await requestReadyBookings()
        setBookings(data)
      } catch (err) {
        if (err.status === 401 || err.status === 403) {
          clearAuth()
          navigate('/login')
          return
        }
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }

    loadBookings()
  }, [navigate])

  async function requestReadyBookings() {
    return bookingApi.getStaffCheckInReady()
  }

  function updateBookingField(event) {
    const { name, value } = event.target
    setBookingForm((current) => ({
      ...current,
      [name]: value,
    }))
  }

  function updateWalkInField(event) {
    const { name, value } = event.target
    setWalkInForm((current) => ({
      ...current,
      [name]: value,
    }))
  }

  function validateMileage(value) {
    return Number.isInteger(Number(value)) && Number(value) >= 0
  }

  async function handleBookingSubmit(event) {
    event.preventDefault()
    setError('')
    setSuccess(null)

    if (!selectedBookingId) {
      setError('Please select a booking.')
      return
    }

    if (!validateMileage(bookingForm.mileage)) {
      setError('Mileage must be a valid non-negative number.')
      return
    }

    if (!bookingForm.reportedProblems.trim()) {
      setError('Please enter the reported problems.')
      return
    }

    setSubmitting(true)

    try {
      const result = await checkInApi.checkInBooking(
        selectedBookingId,
        {
          mileage: Number(bookingForm.mileage),
          reportedProblems:
            bookingForm.reportedProblems.trim(),
        },
      )

      setSuccess(result)
      setSelectedBookingId('')
      setBookingForm({
        mileage: '',
        reportedProblems: '',
      })

      const refreshed = await requestReadyBookings()
      setBookings(refreshed)
    } catch (err) {
      if (err.status === 401 || err.status === 403) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  async function handleWalkInSubmit(event) {
    event.preventDefault()
    setError('')
    setSuccess(null)

    const requiredFields = [
      'fullName',
      'email',
      'phone',
      'registrationNumber',
      'make',
      'model',
      'year',
      'fuelType',
      'mileage',
      'reportedProblems',
    ]

    const missing = requiredFields.find(
      (field) => !String(walkInForm[field]).trim(),
    )

    if (missing) {
      setError('Please complete all required walk-in fields.')
      return
    }

    if (!validateMileage(walkInForm.mileage)) {
      setError('Mileage must be a valid non-negative number.')
      return
    }

    setSubmitting(true)

    try {
      const result = await checkInApi.checkInWalkIn({
        ...walkInForm,
        year: Number(walkInForm.year),
        mileage: Number(walkInForm.mileage),
        reportedProblems:
          walkInForm.reportedProblems.trim(),
      })

      setSuccess(result)

      setWalkInForm({
        fullName: '',
        email: '',
        phone: '',
        address: '',
        registrationNumber: '',
        make: '',
        model: '',
        year: '',
        fuelType: '',
        mileage: '',
        reportedProblems: '',
      })
    } catch (err) {
      if (err.status === 401 || err.status === 403) {
        clearAuth()
        navigate('/login')
        return
      }
      setError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="checkin-page">
      <header className="checkin-header">
        <div>
          <span className="checkin-eyebrow">
            SERVICE ADVISOR
          </span>
          <h1>Vehicle Check-In</h1>
          <p>
            Record arriving vehicles so inspection and
            maintenance work can begin.
          </p>
        </div>

        <button
          className="checkin-back-button"
          onClick={() => navigate('/bookings')}
        >
          ← Bookings
        </button>
      </header>

      <main className="checkin-content">
        <div className="checkin-tabs">
          <button
            className={mode === 'booking' ? 'active' : ''}
            onClick={() => {
              setMode('booking')
              setError('')
              setSuccess(null)
            }}
          >
            Existing Booking
          </button>

          <button
            className={mode === 'walkin' ? 'active' : ''}
            onClick={() => {
              setMode('walkin')
              setError('')
              setSuccess(null)
            }}
          >
            Walk-In Customer
          </button>
        </div>

        {error && (
          <div className="checkin-alert error">
            <strong>Check-in rejected</strong>
            <span>{error}</span>
          </div>
        )}

        {success && (
          <div className="checkin-alert success">
            <strong>Vehicle checked in successfully</strong>
            <span>
              {success.vehicleRegistrationNumber} ·{' '}
              {success.serviceStatus}
            </span>
            <small>
              VehicleCheckedIn event published to Kafka.
            </small>
          </div>
        )}

        {mode === 'booking' ? (
          <form
            className="checkin-card"
            onSubmit={handleBookingSubmit}
          >
            <div className="checkin-card-heading">
              <div>
                <span>FR-CHK-01</span>
                <h2>Check in an existing booking</h2>
                <p>
                  Select a pending or confirmed booking and
                  record the vehicle's arrival information.
                </p>
              </div>
            </div>

            <label>
              Booking
              <select
                value={selectedBookingId}
                onChange={(event) =>
                  setSelectedBookingId(event.target.value)
                }
                disabled={loading || submitting}
              >
                <option value="">
                  {loading
                    ? 'Loading bookings...'
                    : 'Select a booking'}
                </option>

                {bookings.map((booking) => (
                  <option
                    key={booking.id}
                    value={booking.id}
                  >
                    {booking.bookingReference} —{' '}
                    {booking.vehicleRegistrationNumber} —{' '}
                    {booking.vehicleName}
                  </option>
                ))}
              </select>
            </label>

            {selectedBookingId && (
              <div className="checkin-booking-preview">
                {(() => {
                  const booking = bookings.find(
                    (item) =>
                      String(item.id) ===
                      String(selectedBookingId),
                  )

                  if (!booking) return null

                  return (
                    <>
                      <div>
                        <span>Vehicle</span>
                        <strong>
                          {booking.vehicleName}
                        </strong>
                      </div>
                      <div>
                        <span>Registration</span>
                        <strong>
                          {booking.vehicleRegistrationNumber}
                        </strong>
                      </div>
                      <div>
                        <span>Preferred Date</span>
                        <strong>
                          {new Date(
                            booking.preferredDate,
                          ).toLocaleDateString()}
                        </strong>
                      </div>
                    </>
                  )
                })()}
              </div>
            )}

            <div className="checkin-form-grid">
              <label>
                Current Mileage
                <input
                  name="mileage"
                  type="number"
                  min="0"
                  value={bookingForm.mileage}
                  onChange={updateBookingField}
                  placeholder="e.g. 45230"
                  disabled={submitting}
                />
              </label>

              <label className="full-width">
                Reported Problems
                <textarea
                  name="reportedProblems"
                  rows="5"
                  maxLength="500"
                  value={bookingForm.reportedProblems}
                  onChange={updateBookingField}
                  placeholder="Describe the customer's reported problem..."
                  disabled={submitting}
                />
              </label>
            </div>

            <button
              className="checkin-submit-button"
              type="submit"
              disabled={submitting || loading}
            >
              {submitting
                ? 'Checking in...'
                : 'Check In Vehicle'}
            </button>
          </form>
        ) : (
          <form
            className="checkin-card"
            onSubmit={handleWalkInSubmit}
          >
            <div className="checkin-card-heading">
              <div>
                <span>FR-CHK-02</span>
                <h2>Process a walk-in customer</h2>
                <p>
                  Capture the customer, vehicle and arrival
                  information. A service booking is created
                  automatically in CheckedIn status.
                </p>
              </div>
            </div>

            <h3 className="checkin-section-title">
              Customer Information
            </h3>

            <div className="checkin-form-grid">
              <label>
                Full Name *
                <input
                  name="fullName"
                  value={walkInForm.fullName}
                  onChange={updateWalkInField}
                  placeholder="Customer name"
                  disabled={submitting}
                />
              </label>

              <label>
                Email *
                <input
                  name="email"
                  type="email"
                  value={walkInForm.email}
                  onChange={updateWalkInField}
                  placeholder="customer@example.com"
                  disabled={submitting}
                />
              </label>

              <label>
                Phone *
                <input
                  name="phone"
                  value={walkInForm.phone}
                  onChange={updateWalkInField}
                  placeholder="Phone number"
                  disabled={submitting}
                />
              </label>

              <label>
                Address
                <input
                  name="address"
                  value={walkInForm.address}
                  onChange={updateWalkInField}
                  placeholder="Customer address"
                  disabled={submitting}
                />
              </label>
            </div>

            <h3 className="checkin-section-title">
              Vehicle Information
            </h3>

            <div className="checkin-form-grid">
              <label>
                Registration Number *
                <input
                  name="registrationNumber"
                  value={walkInForm.registrationNumber}
                  onChange={updateWalkInField}
                  placeholder="WP ABC-1234"
                  disabled={submitting}
                />
              </label>

              <label>
                Make *
                <input
                  name="make"
                  value={walkInForm.make}
                  onChange={updateWalkInField}
                  placeholder="Toyota"
                  disabled={submitting}
                />
              </label>

              <label>
                Model *
                <input
                  name="model"
                  value={walkInForm.model}
                  onChange={updateWalkInField}
                  placeholder="Corolla"
                  disabled={submitting}
                />
              </label>

              <label>
                Year *
                <input
                  name="year"
                  type="number"
                  min="1900"
                  max="2100"
                  value={walkInForm.year}
                  onChange={updateWalkInField}
                  placeholder="2022"
                  disabled={submitting}
                />
              </label>

              <label>
                Fuel Type *
                <input
                  name="fuelType"
                  value={walkInForm.fuelType}
                  onChange={updateWalkInField}
                  placeholder="Petrol"
                  disabled={submitting}
                />
              </label>

              <label>
                Current Mileage *
                <input
                  name="mileage"
                  type="number"
                  min="0"
                  value={walkInForm.mileage}
                  onChange={updateWalkInField}
                  placeholder="e.g. 45230"
                  disabled={submitting}
                />
              </label>

              <label className="full-width">
                Reported Problems *
                <textarea
                  name="reportedProblems"
                  rows="5"
                  maxLength="500"
                  value={walkInForm.reportedProblems}
                  onChange={updateWalkInField}
                  placeholder="Describe the customer's reported problem..."
                  disabled={submitting}
                />
              </label>
            </div>

            <button
              className="checkin-submit-button"
              type="submit"
              disabled={submitting}
            >
              {submitting
                ? 'Processing walk-in...'
                : 'Check In Walk-In Vehicle'}
            </button>
          </form>
        )}
      </main>
    </div>
  )
}

export default VehicleCheckInPage
