using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels.Attendance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels.Attendance;
using AttendanceSystem.ViewModels.Attendance;


namespace YourProjectName.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance(MarkAttendanceViewModel model)
        {
            // Get current user (adjust based on your authentication)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            // Validate
            if (user == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var today = DateTime.Today;
            var currentTime = DateTime.Now.TimeOfDay;

            // Determine status based on your business logic
            var course = await _context.Courses.FindAsync(model.CourseId);
            var isLate = course.ClassStartTime.HasValue && currentTime > course.ClassStartTime.Value.Add(TimeSpan.FromMinutes(15));

            var attendance = new Attendance
            {
                StudentId = user.Id,
                CourseId = model.CourseId,
                Date = today,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present,
                MarkedAt = DateTime.Now
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // Or wherever appropriate
        }

        [HttpGet]
        public async Task<IActionResult> ViewAttendanceLocations(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null || !course.AllowedLatitude.HasValue || !course.AllowedLongitude.HasValue)
            {
                TempData["ErrorMessage"] = "Course location is not set up properly.";
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId && a.Latitude.HasValue && a.Longitude.HasValue)
                .ToListAsync();

            var model = attendances.Select(a => new AttendanceLocationViewModel
            {
                StudentName = a.Student.FullName,
                Latitude = a.Latitude.Value,
                Longitude = a.Longitude.Value,
                Status = a.Status, // Use the AttendanceStatus enum directly
                AllowedLatitude = course.AllowedLatitude.Value,
                AllowedLongitude = course.AllowedLongitude.Value,
                AllowedRadiusMeters = course.AllowedRadiusMeters ?? 100
            }).ToList();



            ViewData["CourseName"] = course.Name;
            return View(model);
        }
    }
}