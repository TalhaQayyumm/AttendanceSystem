// Should be in: ViewModels/Attendance/AttendanceLocationViewModel.cs
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class AttendanceLocationViewModel
    {
        public int AttendanceId { get; set; }
        public string StudentName { get; set; }
        public DateTime MarkedAt { get; set; }
        public AttendanceStatus Status { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double AllowedLatitude { get; set; }
        public double AllowedLongitude { get; set; }
        public double AllowedRadiusMeters { get; set; }
        public string CourseName { get; internal set; }
    }
}