using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using AttendanceSystem.ViewModels.Attendance;
using AttendanceSystem.ViewModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var courses = await _context.Courses
                .Where(c => c.TeacherId == user.Id)
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> CourseDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Courses
                .Include(c => c.UserCourses)
                .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.TeacherId == user.Id);

            if (course == null)
            {
                return NotFound();
            }

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == id)
                .ToListAsync();

            var studentReports = course.UserCourses
                .Select(uc => uc.User)
                .Select(s => new StudentAttendanceReportViewModel
                {
                    Student = s,
                    Attendances = attendances
                        .Where(a => a.StudentId == s.Id)
                        .OrderBy(a => a.Date)
                        .ToList(),
                    TotalClasses = attendances.Select(a => a.Date).Distinct().Count(),
                    PresentClasses = attendances.Count(a => a.StudentId == s.Id &&
                        (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
                })
                .ToList();

            var model = new CourseDetailsViewModel
            {
                Course = course,
                StudentReports = studentReports
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAttendanceStatus(int attendanceId, AttendanceStatus status)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

            if (attendance == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (attendance.Course.TeacherId != user.Id)
            {
                return Forbid();
            }

            attendance.Status = status;
            _context.Update(attendance);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseDetails", new { id = attendance.CourseId });
        }
    }
}