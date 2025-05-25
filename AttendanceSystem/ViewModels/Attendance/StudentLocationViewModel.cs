using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class StudentLocationViewModel
    {
        public string StudentName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime MarkedAt { get; set; }
        public string CourseName { get; set; }
        public AttendanceStatus Status { get; internal set; }
        public double AllowedLongitude { get; internal set; }
        public double AllowedLatitude { get; internal set; }
        public int? AllowedRadius { get; internal set; }
    }
}
