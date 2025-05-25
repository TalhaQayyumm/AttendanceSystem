using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class TeacherReportsViewModel
    {
        public List<Course> Courses { get; set; }
        public List<AttendanceStatViewModel> AttendanceStats { get; set; }
    }
}
