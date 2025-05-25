using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using AttendanceSystem.ViewModels.Attendance;

namespace AttendanceSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public virtual ICollection<UserCourse> UserCourses { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}