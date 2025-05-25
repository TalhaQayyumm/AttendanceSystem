using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class AttendanceRecordViewModel
    {
        [Key] // Add this using System.ComponentModel.DataAnnotations
        public int AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
    }
}
