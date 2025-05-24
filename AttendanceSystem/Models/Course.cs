using System;
using System.Collections.Generic;

namespace AttendanceSystem.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? AllowedLatitude { get; set; }
        public double? AllowedLongitude { get; set; }
        public double? AllowedRadiusMeters { get; set; }
        public TimeSpan? ClassStartTime { get; set; }
        public TimeSpan? ClassEndTime { get; set; }
        public DayOfWeek? ClassDay { get; set; }
        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }
        public virtual ICollection<UserCourse> UserCourses { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}
