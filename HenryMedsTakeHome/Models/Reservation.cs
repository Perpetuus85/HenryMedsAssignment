namespace HenryMedsTakeHome.Models
{
    public class Reservation
    {
        public long Id { get; set; }

        public long? ClientId { get; set; }

        public long ProviderId { get; set; }

        public DateTime TimeSlot { get; set; }

        public Boolean IsConfirmed { get; set; }
    }
}
