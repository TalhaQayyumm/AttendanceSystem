namespace AttendanceSystem.ViewModels.Courses
{
    public class EnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public List<EnrollmentRecordViewModel> Students { get; set; } = new List<EnrollmentRecordViewModel>();
    }

    public class EnrollmentRecordViewModel
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsEnrolled { get; set; }
    }
}