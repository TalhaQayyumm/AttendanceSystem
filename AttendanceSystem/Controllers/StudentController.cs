using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Services;
using AttendanceSystem.ViewModels.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Services;
using AttendanceSystem.ViewModels;
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
        private readonly LocationService _locationService;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            LocationService locationService)
        {
            _context = context;
            _userManager = userManager;
            _locationService = locationService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var courses = await _context.UserCourses
                .Include(uc => uc.Course)
                .ThenInclude(c => c.Teacher)
                .Where(uc => uc.UserId == user.Id)
                .ToListAsync();

            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> MarkAttendance(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                return NotFound();
            }

            var isEnrolled = await _context.UserCourses
                .AnyAsync(uc => uc.UserId == user.Id && uc.CourseId == courseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var model = new MarkAttendanceViewModel
            {
                CourseId = courseId,
                CourseName = course.Name,
                RequiresLocation = course.AllowedLatitude.HasValue && course.AllowedLongitude.HasValue
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance(MarkAttendanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Courses.FindAsync(model.CourseId);

            if (course == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == user.Id &&
                                        a.CourseId == model.CourseId &&
                                        a.Date == today);

            if (existingAttendance != null)
            {
                ModelState.AddModelError("", "Attendance already marked for today.");
                return View(model);
            }

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var isWithinTimeWindow = true;

            if (course.ClassDay.HasValue && course.ClassStartTime.HasValue && course.ClassEndTime.HasValue)
            {
                var isCorrectDay = now.DayOfWeek == course.ClassDay.Value;
                var isWithinTime = currentTime >= course.ClassStartTime.Value &&
                                  currentTime <= course.ClassEndTime.Value;

                isWithinTimeWindow = isCorrectDay && isWithinTime;
            }

            var isWithinLocation = true;
            if (course.AllowedLatitude.HasValue && course.AllowedLongitude.HasValue && course.AllowedRadiusMeters.HasValue)
            {
                if (model.Latitude == null || model.Longitude == null)
                {
                    ModelState.AddModelError("", "Location is required for this course.");
                    return View(model);
                }

                isWithinLocation = _locationService.IsWithinAllowedDistance(
                    model.Latitude.Value, model.Longitude.Value,
                    course.AllowedLatitude.Value, course.AllowedLongitude.Value,
                    course.AllowedRadiusMeters.Value);
            }

            var status = AttendanceStatus.Present;
            if (!isWithinTimeWindow)
            {
                status = AttendanceStatus.Late;
            }
            else if (!isWithinLocation)
            {
                status = AttendanceStatus.Absent;
            }

            var attendance = new Attendance
            {
                StudentId = user.Id,
                CourseId = model.CourseId,
                Date = today,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Status = status,
                MarkedAt = now
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return RedirectToAction("AttendanceReport", new { courseId = model.CourseId });
        }

        public async Task<IActionResult> AttendanceReport(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);

            var isEnrolled = await _context.UserCourses
                .AnyAsync(uc => uc.UserId == user.Id && uc.CourseId == courseId);

            if (!isEnrolled)
            {
                return Forbid();
            }

            var course = await _context.Courses.FindAsync(courseId);
            var attendances = await _context.Attendances
                .Where(a => a.StudentId == user.Id && a.CourseId == courseId)
                .OrderBy(a => a.Date)
                .ToListAsync();

            var totalClasses = attendances.Count;
            var presentClasses = attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
            var percentage = totalClasses > 0 ? (double)presentClasses / totalClasses * 100 : 0;

            var model = new AttendanceReportViewModel
            {
                CourseName = course.Name,
                Records = attendances.Select(a => new AttendanceRecordViewModel
                {
                    Date = a.Date,
                    Status = a.Status
                }).ToList(),
                AttendancePercentage = percentage,
                IsBelowMinimum = percentage < 75
            };

            return View(model);
        }

    }
}