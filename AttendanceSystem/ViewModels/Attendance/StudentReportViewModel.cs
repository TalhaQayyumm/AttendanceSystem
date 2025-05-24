using AttendanceSystem.Models;
using System.Collections.Generic;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class StudentReportViewModel
    {
        public ApplicationUser Student { get; set; }
        public List<MarkAttendanceViewModel> Attendances { get; set; } = new List<MarkAttendanceViewModel>();
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
    }
}