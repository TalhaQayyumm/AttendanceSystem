
namespace AttendanceSystem.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public DateTime Date { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime? MarkedAt { get; set; }
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused
    }
}
