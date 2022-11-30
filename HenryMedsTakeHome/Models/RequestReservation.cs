namespace HenryMedsTakeHome.Models
{
    public class RequestReservation
    {
        public long Id { get; set; }

        public long ReservationId { get; set; }

        public long ClientId { get; set; }

        public DateTime TimeRequested { get; set; }
    }
}
