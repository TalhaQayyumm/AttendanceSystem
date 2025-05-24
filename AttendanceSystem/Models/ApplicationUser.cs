using Microsoft.AspNetCore.Identity;
using AttendanceSystem.Models;
using System.Collections.Generic;

namespace AttendanceSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public virtual ICollection<UserCourse> UserCourses { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}