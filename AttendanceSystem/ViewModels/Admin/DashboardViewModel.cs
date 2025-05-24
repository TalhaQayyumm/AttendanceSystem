using System;
using System.Collections.Generic;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels.Attendance;

namespace AttendanceSystem.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public List<ApplicationUser> Students { get; set; } = new();
        public List<ApplicationUser> Teachers { get; set; } = new();
        public int CourseCount { get; set; }
        public List<AttendanceRecordViewModel> RecentAttendances { get; set; } = new();

        public int TotalStudents => Students?.Count ?? 0;
        public int TotalTeachers => Teachers?.Count ?? 0;
        public int TotalCourses => CourseCount;
    }
}
