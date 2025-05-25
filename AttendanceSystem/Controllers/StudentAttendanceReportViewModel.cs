using AttendanceSystem.Models;

namespace AttendanceSystem.Controllers
{
    internal class StudentAttendanceReportViewModel
    {
        public ApplicationUser Student { get; set; }
        public List<AttendanceStatus> Attendances { get; set; }
        public int TotalClasses { get; set; }
        public int PresentClasses { get; set; }
    }
}