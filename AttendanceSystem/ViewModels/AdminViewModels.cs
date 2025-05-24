// ViewModels/AdminViewModels.cs
using AttendanceSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<ApplicationUser> TotalStudents { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> TotalTeachers { get; set; } = new List<ApplicationUser>();
        public int TotalCourses { get; set; }
        public List<Attendance> RecentAttendances { get; set; } = new List<Attendance>();
    }

    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> CurrentRoles { get; set; } = new List<string>();
        public List<string> AllRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class CourseViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Display(Name = "Teacher")]
        public string TeacherId { get; set; }

        public List<ApplicationUser> AvailableTeachers { get; set; } = new List<ApplicationUser>();

        [Range(-90, 90)]
        public double? AllowedLatitude { get; set; }

        [Range(-180, 180)]
        public double? AllowedLongitude { get; set; }

        [Range(1, 1000)]
        public int? AllowedRadiusMeters { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? ClassStartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? ClassEndTime { get; set; }

        public DayOfWeek? ClassDay { get; set; }
    }

    public class EnrollStudentsViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public List<StudentEnrollmentViewModel> Students { get; set; } = new List<StudentEnrollmentViewModel>();
    }

    public class StudentEnrollmentViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsEnrolled { get; set; }
    }

    public class StudentAttendanceReport
    {
        public ApplicationUser Student { get; set; }
        public List<Attendance> Attendances { get; set; } = new List<Attendance>();
        public int TotalClasses { get; set; }
        public int PresentClasses { get; set; }
    }
}