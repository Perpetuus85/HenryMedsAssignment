namespace HenryMedsTakeHome.Models
{
    public class WorkDayRequest
    {
        public long ProviderId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
