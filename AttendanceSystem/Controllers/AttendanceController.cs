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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace YourProjectName.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


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
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ViewAttendanceLocations(int courseId, DateTime? date = null)
        {
            try
            {
                // Validate courseId
                if (courseId <= 0)
                {
                    TempData["ErrorMessage"] = "Invalid course ID";
                    return RedirectToAction(nameof(Index));
                }

                var selectedDate = date ?? DateTime.Today;
                var teacherId = _userManager.GetUserId(User);

                // Verify teacher is authenticated
                if (string.IsNullOrEmpty(teacherId))
                {
                    return Unauthorized();
                }

                // Get course with location requirements and verify teacher owns it
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);

                if (course == null)
                {
                    TempData["ErrorMessage"] = "Course not found or you don't have permission to view it";
                    return RedirectToAction(nameof(Index));
                }

                // Check if location settings are configured
                if (!course.AllowedLatitude.HasValue || !course.AllowedLongitude.HasValue)
                {
                    TempData["ErrorMessage"] = "Location settings not configured for this course";
                    return RedirectToAction("CourseDetails", new { id = courseId });
                }

                // Get attendances with location data
                var attendances = await _context.Attendances
                    .Include(a => a.Student)
                    .Where(a => a.CourseId == courseId &&
                               a.Date == selectedDate &&
                               a.Latitude.HasValue &&
                               a.Longitude.HasValue)
                    .ToListAsync();

                // Prepare view model with distance calculation
                var model = attendances.Select(a => new AttendanceLocationViewModel
                {
                    AttendanceId = a.AttendanceId,
                    StudentName = a.Student.FullName,
                    Latitude = a.Latitude ?? 0,
                    Longitude = a.Longitude ?? 0,
                    Status = a.Status,
                    MarkedAt = a.MarkedAt ?? DateTime.MinValue,
                    AllowedLatitude = course.AllowedLatitude.Value,
                    AllowedLongitude = course.AllowedLongitude.Value,
                    AllowedRadiusMeters = course.AllowedRadiusMeters ?? 100,
                    DistanceFromAllowedLocation = CalculateDistance(
                        a.Latitude ?? 0,
                        a.Longitude ?? 0,
                        course.AllowedLatitude.Value,
                        course.AllowedLongitude.Value)
                }).ToList();

                ViewBag.CourseName = course.Name;
                ViewBag.CourseId = courseId;
                ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
                ViewBag.AllowLocationSettings = true;

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error (implementation depends on your logging setup)
                // _logger.LogError(ex, "Error loading attendance locations");

                TempData["ErrorMessage"] = "An error occurred while loading attendance locations";
                return RedirectToAction(nameof(Index));
            }
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