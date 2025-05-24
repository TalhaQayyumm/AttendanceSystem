using System;
using AttendanceSystem.Models;

namespace AttendanceSystem.Models
{
    public class UserCourse
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public DateTime EnrollmentDate { get; set; }
    }
}