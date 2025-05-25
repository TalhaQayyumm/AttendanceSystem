using AttendanceSystem.Models;
using System.Collections.Generic;

namespace AttendanceSystem.ViewModels.Teacher
{
    public class TeacherReportsViewModel
    {
        public List<Course> Courses { get; set; }
        public List<AttendanceStatViewModel> AttendanceStats { get; set; }

        //public static implicit operator TeacherReportsViewModel(TeacherReportsViewModel v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}