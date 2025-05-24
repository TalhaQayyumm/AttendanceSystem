using AttendanceSystem.Models;
using System.Collections.Generic;

namespace AttendanceSystem.ViewModels.Attendance
{
    public class AttendanceReportViewModel
    {
        public string CourseName { get; set; }
        public List<AttendanceRecordViewModel> Records { get; set; } = new List<AttendanceRecordViewModel>();
        public double AttendancePercentage { get; set; }
        public bool IsBelowMinimum { get; set; }

        // Add this computed property if you need attendance count
        public int AttendancesCount => Records.Count;
    }

}