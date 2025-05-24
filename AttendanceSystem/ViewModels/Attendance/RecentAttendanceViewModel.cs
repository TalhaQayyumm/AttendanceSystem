using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class RecentAttendanceViewModel
    {
        public DateTime Date { get; set; }
        public string StudentFullName { get; set; }
        public string CourseName { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
