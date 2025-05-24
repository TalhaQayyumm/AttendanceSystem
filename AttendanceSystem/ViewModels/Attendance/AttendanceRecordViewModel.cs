using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class AttendanceRecordViewModel
    {
        public DateTime Date { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
    }
}
