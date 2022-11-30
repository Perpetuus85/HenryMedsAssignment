using HenryMedsTakeHome.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HenryMedsTakeHome.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly ScheduleContext _context;

        public ScheduleController(ScheduleContext context)
        {
            _context = context;
            var client = new Client();
            _context.Clients.Add(client);
            var provider = new Provider();
            _context.Providers.Add(provider);
            _context.SaveChanges();
        }

        [HttpGet("GetAllProviders")]
        public async Task<ActionResult<List<Provider>>> GetAllProviders()
        {
            return await _context.Providers.ToListAsync();
        }

        [HttpGet("GetAllConfirmedReservationsForProvider/{providerId}")]
        public async Task<ActionResult<List<Reservation>>> GetAllConfirmedReservationsForProvider(long providerId)
        {
            return await _context.Reservations.Where(x => x.ProviderId == providerId && x.IsConfirmed == true).ToListAsync();
        }

        // PUT: api/Schedule/ConfirmReservation/5
        // this allows clients to confirm their reservation
        [HttpPut("ConfirmReservation/{requestReservationId}")]
        public async Task<IActionResult> ConfirmReservation(long requestReservationId)
        {
            var requestReservation = await _context.RequestReservations.FindAsync(requestReservationId);
            if (requestReservation == null)
            {
                return NotFound();
            }

            // if reservation request was made over 30 minutes ago, do not confirm
            if (DateTime.Now.AddMinutes(-30) > requestReservation.TimeRequested)
            {
                return BadRequest("Confirmation no longer allowed after 30 minutes");
            }

            var reservation = await _context.Reservations.FindAsync(requestReservation.ReservationId);
            if (reservation == null)
            {
                return NotFound();
            }
            reservation.ClientId = requestReservation.ClientId;
            reservation.IsConfirmed = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!ReservationExists(reservation.Id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Schedule/ReserveTimeSlot/5/1
        // this allows clients to reserve an available slot
        [HttpPost("ReserveTimeSlot/{reservationId}/{clientId}")]
        public async Task<ActionResult<RequestReservation>> ReserveTimeSlot(long reservationId, long clientId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            var requestReservation = new RequestReservation
            {
                ReservationId = reservationId,
                ClientId = clientId,
                TimeRequested = DateTime.Now
            };

            _context.RequestReservations.Add(requestReservation);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(requestReservation);
            }
            catch (DbUpdateConcurrencyException) when (!ReservationExists(reservationId))
            {
                return NotFound();
            }
        }

        // GET: api/Schedule/GetAvailabilityByProvider/1
        // this allows clients to get availability by provider
        [HttpGet("GetAvailabilityByProvider/{providerId}")]
        public async Task<ActionResult<List<Reservation>>> GetAvailabilityByProvider(long providerId)
        {
            // get future reservations for provider with no client and not confirmed
            DateTime future = DateTime.Now.AddDays(1);
            return await _context.Reservations
                .Where(x => x.ProviderId == providerId && x.IsConfirmed == false && x.ClientId == null && x.TimeSlot > future)
                .Take(10)
                .ToListAsync();
        }

        // POST: api/Schedule/SubmitWorkDay
        // this allows providers to submit times they would like to work on the schedule
        [HttpPost("SubmitWorkDay")]
        public async Task<ActionResult<List<Reservation>>> SubmitWorkDay(WorkDayRequest workDayRequest)
        {
            // create new reservations every 15 minutes within that workday with no clientId and IsConfirmed = false
            DateTime startTime = workDayRequest.StartTime;
            while (startTime < workDayRequest.EndTime)
            {
                var reservation = new Reservation
                {
                    ClientId = null,
                    ProviderId = workDayRequest.ProviderId,
                    TimeSlot = startTime,
                    IsConfirmed = false
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                startTime = startTime.AddMinutes(15);
            }

            return await _context.Reservations
                .Where(x => x.ProviderId == workDayRequest.ProviderId)
                .ToListAsync();
        }

        private bool ReservationExists(long id)
        {
            return _context.Reservations.Any(x => x.Id == id);
        }
    }
}
