using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class MarkAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public double Accuracy { get; set; }
    }
}