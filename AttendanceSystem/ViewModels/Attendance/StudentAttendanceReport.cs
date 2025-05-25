using System.ComponentModel.DataAnnotations.Schema;
using AttendanceSystem.ViewModels;

[NotMapped]
public class StudentAttendanceReport
{
    public Student Student { get; set; }

    [NotMapped]
    public List<AttendanceRecord> Attendances { get; set; }

    public int TotalClasses { get; set; }
    public int PresentClasses { get; set; }
}
