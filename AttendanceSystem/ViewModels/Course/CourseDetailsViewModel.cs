using AttendanceSystem.Models;
using System.Collections.Generic;

namespace AttendanceSystem.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Models.Course Course { get; set; } = null!;
        public List<StudentAttendanceReportViewModel> StudentReports { get; set; } = new();
    }

    public class StudentAttendanceReportViewModel
    {
        public Models.ApplicationUser Student { get; set; } = null!;
        public int PresentClasses { get; set; }
        public int TotalClasses { get; set; }
        public List<Models.Attendance> Attendances { get; set; } = new(); // Fully qualified
    }
}