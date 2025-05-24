namespace AttendanceSystem.ViewModels.Attendance
{
    public class MarkAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public bool RequiresLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // If you need full Course object (not recommended for view models)
        // public Course Course { get; set; }
    }
}