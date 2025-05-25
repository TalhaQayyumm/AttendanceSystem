using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using AttendanceSystem.ViewModels.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Student/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var courses = await _context.UserCourses
                .Include(uc => uc.Course)
                .Where(uc => uc.UserId == user.Id)
                .Select(uc => uc.Course)
                .ToListAsync();

            return View(courses);
        }

        // GET: Student/MarkAttendance/{courseId}
        public async Task<IActionResult> MarkAttendance(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var model = new MarkAttendanceViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.Name
            };

            return View(model);
        }


        [HttpGet] // Explicitly declare as GET
        public async Task<IActionResult> ViewLocation(int attendanceId)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

            if (attendance == null || !attendance.Latitude.HasValue)
            {
                return NotFound();
            }

            return View(new AttendanceSystem.ViewModels.Attendance.StudentLocationViewModel
            {
                StudentName = attendance.Student.FullName,
                Latitude = attendance.Latitude.Value,
                Longitude = attendance.Longitude.Value,
                MarkedAt = attendance.MarkedAt.Value,
                CourseName = attendance.Course.Name
            });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(MarkAttendanceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Courses.FindAsync(model.CourseId);

            if (!ModelState.IsValid || course == null)
            {
                return View(model);
            }

            var existing = await _context.Attendances
                .AnyAsync(a => a.StudentId == user.Id &&
                              a.CourseId == model.CourseId &&
                              a.Date.Date == DateTime.Today);

            if (existing)
            {
                TempData["ErrorMessage"] = "You've already marked attendance today";
                return RedirectToAction("Index");
            }

            var attendance = new Attendance
            {
                StudentId = user.Id,
                CourseId = model.CourseId,
                Date = DateTime.Today,
                Status = AttendanceStatus.Present,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Accuracy = model.Accuracy, // Make sure this is included
                MarkedAt = DateTime.Now
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance marked successfully!";
            return RedirectToAction("Index");
        }

        // GET: Student/AttendanceHistory
        public async Task<IActionResult> AttendanceHistory(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            var attendances = await _context.Attendances
                .Where(a => a.CourseId == courseId && a.StudentId == userId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(attendances);
        }


        // Helper method to calculate distance between coordinates
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // meters
            var φ1 = lat1 * Math.PI / 180;
            var φ2 = lat2 * Math.PI / 180;
            var Δφ = (lat2 - lat1) * Math.PI / 180;
            var Δλ = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}