// In Models/Attendance.cs
using AttendanceSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Attendance
{
    [Key]
    public int AttendanceId { get; set; }
    public string StudentId { get; set; }
    public ApplicationUser Student { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Accuracy { get; set; } // This is the missing column
    public DateTime? MarkedAt { get; set; }
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    Excused
}
