namespace AttendanceSystem.ViewModels
{
    public class AttendanceViewModel
    {
        public int AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime? MarkedAt { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
