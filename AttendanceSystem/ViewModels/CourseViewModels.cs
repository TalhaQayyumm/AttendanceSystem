using AttendanceSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels.Courses
{
    public class CourseViewModel
    {
        [Required] public string Name { get; set; }
        public string Description { get; set; }

        [Required]
        [Display(Name = "Teacher")]
        public string TeacherId { get; set; }

        public List<ApplicationUser> Teachers { get; set; } = new();

        [Range(-90, 90)] public double? Latitude { get; set; }
        [Range(-180, 180)] public double? Longitude { get; set; }
        [Range(1, 1000)] public int? RadiusMeters { get; set; }

        [DataType(DataType.Time)] public TimeSpan? StartTime { get; set; }
        [DataType(DataType.Time)] public TimeSpan? EndTime { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
    }

    public class EnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public List<EnrollmentRecordViewModel> Students { get; set; } = new();
    }

    public class EnrollmentRecordViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsEnrolled { get; set; }
    }
}