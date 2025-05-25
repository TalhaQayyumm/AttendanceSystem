// File: ViewModels/StudentLocationViewModel.cs
namespace AttendanceSystem.ViewModels
{
    public class StudentLocationViewModel
    {
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public DateTime MarkedAt { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } // Or use your AttendanceStatus enum
        public double? AllowedLatitude { get; set; }
        public double? AllowedLongitude { get; set; }
        public int? AllowedRadiusMeters { get; set; }
        public int AttendanceId { get; set; }

        // Calculated property (optional)
        public double? DistanceFromAllowedLocation { get; set; }
    }
}