using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class MarkAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public bool RequiresLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class AttendanceReportViewModel
    {
        public string CourseName { get; set; }
        public List<AttendanceRecord> Records { get; set; } = new();
        public double AttendancePercentage { get; set; }
        public bool BelowMinimum { get; set; }
    }

    public class AttendanceRecord
    {
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class StudentReportViewModel
    {
        public ApplicationUser Student { get; set; }
        public List<AttendanceRecord> Attendances { get; set; } = new();
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
    }
}