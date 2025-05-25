using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using AttendanceSystem.ViewModels.Attendance;
using AttendanceSystem.ViewModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Controllers/TeacherController.cs
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ViewAttendanceLocations(int courseId, DateTime? date = null)
        {
            var selectedDate = date ?? DateTime.Today;
            var teacherId = _userManager.GetUserId(User);

            // Get course with location requirements and verify teacher owns it
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);

            if (course == null)
            {
                return NotFound("Course not found or you don't have permission to view it");
            }

            if (!course.AllowedLatitude.HasValue || !course.AllowedLongitude.HasValue)
            {
                // Redirect to a page to set up location settings
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

            // Prepare view model
            var model = attendances.Select(a => new AttendanceLocationViewModel
            {
                AttendanceId = a.AttendanceId,
                StudentName = a.Student.FullName,
                Latitude = a.Latitude ?? 0, // Safe fallback
                Longitude = a.Longitude ?? 0, // Safe fallback
                Status = a.Status,
                MarkedAt = a.MarkedAt ?? DateTime.MinValue, // Safe fallback
                AllowedLatitude = course.AllowedLatitude.Value,
                AllowedLongitude = course.AllowedLongitude.Value,
                AllowedRadiusMeters = course.AllowedRadiusMeters ?? 100
            }).ToList();

            ViewBag.CourseName = course.Name;
            ViewBag.CourseId = courseId;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.AllowLocationSettings = true;

            return View(model);
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var courses = await _context.Courses
                .Where(c => c.TeacherId == user.Id)
                .ToListAsync();

            return View(courses);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Reports()
        {
            var teacherId = _userManager.GetUserId(User);

            var model = new AttendanceSystem.ViewModels.Teacher.TeacherReportsViewModel
            {
                Courses = await _context.Courses
                    .Where(c => c.TeacherId == teacherId)
                    .Select(c => new AttendanceSystem.ViewModels.Course
                    {
                        CourseId = c.CourseId,
                        Name = c.Name
                    })
                    .ToListAsync(),

                AttendanceStats = await _context.Attendances
                    .Include(a => a.Course)
                    .Where(a => a.Course != null && a.Course.TeacherId == teacherId)
                    .GroupBy(a => a.Status)
                    .Select(g => new AttendanceSystem.ViewModels.Teacher.AttendanceStatViewModel
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync()
            };

            return View(model); // <-- This line returns the model to the view
        }

        //    [Authorize(Roles = "Teacher")]
        //    public async Task<IActionResult> Reports()
        //    {
        //        var teacherId = _userManager.GetUserId(User);

        //        // Get attendance stats grouped by status
        //        var attendanceStats = await _context.Attendances
        //            .Where(a => a.Course.TeacherId == teacherId)
        //            .GroupBy(a => a.Status)
        //            .Select(g => new AttendanceStatViewModel
        //            {
        //                Status = g.Key,
        //                Count = g.Count()
        //            })
        //            .ToListAsync();

        //        // Ensure all statuses are represented (even with 0 counts)
        //        var allStatuses = Enum.GetValues(typeof(AttendanceStatus)).Cast<AttendanceStatus>();
        //        foreach (var status in allStatuses)
        //        {
        //            if (!attendanceStats.Any(s => s.Status == status))
        //            {
        //                attendanceStats.Add(new AttendanceStatViewModel
        //                {
        //                    Status = status,
        //                    Count = 0
        //                });
        //            }
        //        }

        //        var model = new TeacherReportsViewModel
        //        {
        //            Courses = await _context.Courses
        //.Where(c => c.TeacherId == teacherId)
        //.Select(c => new AttendanceSystem.ViewModels.Course
        //{
        //    CourseId = c.CourseId,
        //    Name = c.Name
        //})
        //.ToListAsync(),

        //            AttendanceStats = attendanceStats
        //        };

        //        return View(model);
        //    }

        //[Authorize(Roles = "Teacher")]
        //public async Task<IActionResult> Reports()
        //{
        //    var teacherId = _userManager.GetUserId(User);
        //    if (string.IsNullOrEmpty(teacherId))
        //    {
        //        TempData["ErrorMessage"] = "Unable to determine teacher identity.";
        //        return RedirectToAction("Index");
        //    }

        //    var model = new ViewModels.Teacher.TeacherReportsViewModel
        //    {
        //        Courses = await _context.Courses
        //            .Where(c => c.TeacherId == teacherId)
        //            .Include(c => c.Attendances)
        //            .Select(c => new ViewModels.Course
        //            {
        //                CourseId = c.CourseId,
        //                Name = c.Name
        //            })
        //            .ToListAsync(),

        //        AttendanceStats = await _context.Attendances
        //            .Where(a => a.Course != null && a.Course.TeacherId == teacherId)
        //            .GroupBy(a => a.Status)
        //            .Select(g => new ViewModels.Teacher.AttendanceStatViewModel
        //            {
        //                Status = g.Key,
        //                Count = g.Count()
        //            })
        //            .ToListAsync()
        //    };

        //    return View(model);
        //}



        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CourseDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.TeacherId == user.Id);

            if (course == null) return NotFound();

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == id)
                .ToListAsync();

            var students = await _context.UserCourses
                .Where(uc => uc.CourseId == id)
                .Select(uc => uc.User)
                .ToListAsync();

            var model = new CourseDetailsViewModel
            {
                Course = new ViewModels.Course
                {
                    CourseId = course.CourseId,
                    Name = course.Name
                },
                StudentReports = students.Select(s => new AttendanceSystem.ViewModels.StudentAttendanceReport
                {
                    Student = new Student
                    {
                        StudentId = int.TryParse(s.Id, out var sid) ? sid : 0,
                        FullName = s.FullName
                    },
                    Attendances = attendances
                        .Where(a => a.StudentId == s.Id)
                        .Select(a => new AttendanceRecord
                        {
                            AttendanceId = a.AttendanceId,
                            Date = a.Date,
                            Status = a.Status.ToString()
                        })
                        .ToList(),
                    TotalClasses = attendances.Select(a => a.Date).Distinct().Count(),
                    PresentClasses = attendances.Count(a => a.StudentId == s.Id &&
                        (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
                }).ToList()
            };


            return View(model);
        }





        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ViewStudentLocation(int? attendanceId, string searchString)
        {
            var teacherId = _userManager.GetUserId(User);

            if (attendanceId.HasValue && attendanceId > 0)
            {
                // Existing code for viewing specific attendance
                var attendance = await _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Course)
                    .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId &&
                                           a.Course.TeacherId == teacherId);

                if (attendance == null) return NotFound();

                var model = new AttendanceLocationViewModel
                {
                    AttendanceId = attendance.AttendanceId,
                    StudentName = attendance.Student.FullName,
                    CourseName = attendance.Course.Name,
                    MarkedAt = attendance.MarkedAt ?? DateTime.Now,
                    Latitude = attendance.Latitude.Value,
                    Longitude = attendance.Longitude.Value,
                    Status = attendance.Status,
                    AllowedLatitude = attendance.Course.AllowedLatitude.Value,
                    AllowedLongitude = attendance.Course.AllowedLongitude.Value,
                    AllowedRadiusMeters = attendance.Course.AllowedRadiusMeters ?? 100
                };

                return View(model);
            }

            // Handle search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                var attendances = await _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Course)
                    .Where(a => a.Course.TeacherId == teacherId &&
                               (a.Student.FullName.Contains(searchString) ||
                                a.Course.Name.Contains(searchString)) &&
                               a.Latitude.HasValue &&
                               a.Longitude.HasValue)
                    .OrderByDescending(a => a.Date)
                    .Take(20)
                    .ToListAsync();

                return View("SearchResults", attendances.Select(a => new AttendanceLocationViewModel
                {
                    AttendanceId = a.AttendanceId,
                    StudentName = a.Student.FullName,
                    CourseName = a.Course.Name,
                    MarkedAt = a.MarkedAt ?? DateTime.Now,
                    Latitude = a.Latitude.Value,
                    Longitude = a.Longitude.Value,
                    Status = a.Status,
                    AllowedLatitude = a.Course.AllowedLatitude.Value,
                    AllowedLongitude = a.Course.AllowedLongitude.Value,
                    AllowedRadiusMeters = a.Course.AllowedRadiusMeters ?? 100
                }).ToList());
            }

            // Show search page if no parameters
            return View("Search");
        }





        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateAttendanceStatus(int attendanceId, string status)
        {
            if (!Enum.TryParse<AttendanceStatus>(status, out var statusEnum))
            {
                TempData["ErrorMessage"] = "Invalid attendance status";
                return RedirectToAction("Index"); // Redirect to a safe fallback action
            }

            var attendance = await _context.Attendances
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

            if (attendance == null)
            {
                TempData["ErrorMessage"] = "Attendance record not found";
                return RedirectToAction("Index"); // Redirect to a safe fallback action
            }

            // Verify teacher owns the course
            var teacherId = _userManager.GetUserId(User);
            if (attendance.Course.TeacherId != teacherId)
                return Forbid();

            attendance.Status = statusEnum;

            if ((statusEnum == AttendanceStatus.Present || statusEnum == AttendanceStatus.Late) &&
                !attendance.MarkedAt.HasValue)
            {
                attendance.MarkedAt = DateTime.Now;
            }

            _context.Update(attendance);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Attendance status updated to {statusEnum}";
            return RedirectToAction("CourseDetails", new { id = attendance.CourseId });
        }

    }
}