using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystem.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        //public List<string> AttendanceStatuses { get; set; }

        public List<string> AttendanceStatuses { get; set; } = new List<string>
        {
            "Present",
            "Absent",
            "Late",
            "Excused"
        };

        public List<StudentAttendanceReport> StudentReports { get; set; }
    }

    [NotMapped] // Add this attribute
    public class StudentAttendanceReport
    {
        public Student Student { get; set; }
        public int PresentClasses { get; set; }
        public int TotalClasses { get; set; }
        public List<AttendanceRecord> Attendances { get; set; }
    }

    [NotMapped] // Add this attribute
    public class AttendanceRecord
    {
        public int AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }

    [NotMapped]
    public class Course
    {
        public int CourseId { get; set; }
        public string Name { get; set; }

    }
    [NotMapped]
    public class Student
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
    }
}
