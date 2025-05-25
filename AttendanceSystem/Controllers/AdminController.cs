using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels.Admin;
using AttendanceSystem.ViewModels.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.ViewModels.Admin;
using AttendanceSystem.ViewModels.Courses;
using AttendanceSystem.ViewModels.Attendance;
using AttendanceSystem.ViewModels.Admin;
using AttendanceSystem.ViewModels.Attendance;
using Course = AttendanceSystem.Models.Course;


namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new DashboardViewModel
            {
                Students = (await _userManager.GetUsersInRoleAsync("Student")).ToList(),
                Teachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList(),
                CourseCount = await _context.Courses.CountAsync(),
                RecentAttendances = await _context.Attendances
                    .OrderByDescending(a => a.Date)
                    .Take(10)
                    .Include(a => a.Student)
                    .Include(a => a.Course)
                    .Select(a => new AttendanceRecordViewModel
                    {
                        Date = a.Date,
                        Status = a.Status,
                        StudentFullName = a.Student.FullName,
                        CourseName = a.Course.Name
                    })
                    .ToListAsync()
            };
            return View(stats);
        }

        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userRoles = new List<UserWithRolesViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRoles.Add(new UserWithRolesViewModel
                    {
                        User = user,
                        Roles = roles
                    });
                }

                return View(userRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management");
                TempData["ErrorMessage"] = "An error occurred while loading users.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("User ID is required");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var allRoles = await _roleManager.Roles.ToListAsync();

                var model = new EditUserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    CurrentRoles = roles.ToList(),
                    AvailableRoles = allRoles.Select(r => r.Name).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading user edit page for ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading user details.";
                return RedirectToAction("ManageUsers");
            }
        }


        [Authorize(Roles = "Admin")]


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties
                user.Email = model.Email;
                user.UserName = model.Email;
                user.FullName = model.FullName;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    AddErrors(updateResult);
                    model.AvailableRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                    return View(model);
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    AddErrors(removeResult);
                    model.AvailableRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                    return View(model);
                }

                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    if (!addResult.Succeeded)
                    {
                        AddErrors(addResult);
                        model.AvailableRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = "User updated successfully";
                return RedirectToAction(nameof(ManageUsers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID: {model.UserId}");
                TempData["ErrorMessage"] = "An error occurred while updating the user.";
                return RedirectToAction(nameof(EditUser), new { id = model.UserId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // First delete related records if needed (e.g., attendances, enrollments)
                var userCourses = await _context.UserCourses
                    .Where(uc => uc.UserId == id)
                    .ToListAsync();
                _context.UserCourses.RemoveRange(userCourses);

                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == id)
                    .ToListAsync();
                _context.Attendances.RemoveRange(attendances);

                // Then delete the user
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                TempData["SuccessMessage"] = "User deleted successfully";
                return RedirectToAction(nameof(ManageUsers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while deleting the user.";
                return RedirectToAction(nameof(ManageUsers));
            }
        }

        public async Task<IActionResult> ManageCourses()
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Teacher)  // Include the Teacher information
                    .ToListAsync();

                return View(courses);  // Pass the courses list to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading course management");
                TempData["ErrorMessage"] = "An error occurred while loading courses.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.UserCourses)
                    .Include(c => c.Attendances)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                {
                    return NotFound();
                }

                // First remove all related records
                _context.UserCourses.RemoveRange(entities: course.UserCourses);
                _context.Attendances.RemoveRange((Attendance)course.Attendances);

                // Then remove the course
                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course deleted successfully";
                return RedirectToAction(nameof(ManageCourses));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting course with ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while deleting the course.";
                return RedirectToAction(nameof(ManageCourses));
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            try
            {
                var model = new CourseFormViewModel
                {
                    Teachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading course creation form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("ManageCourses");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CourseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Teachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList();
                return View(model);
            }

            try
            {
                var course = new Course
                {
                    Name = model.Name,
                    Description = model.Description,
                    TeacherId = model.TeacherId,
                    AllowedLatitude = model.Latitude,
                    AllowedLongitude = model.Longitude,
                    AllowedRadiusMeters = model.RadiusMeters,
                    ClassStartTime = model.StartTime,
                    ClassEndTime = model.EndTime,
                    ClassDay = model.DayOfWeek
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course created successfully";
                return RedirectToAction("ManageCourses");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                TempData["ErrorMessage"] = "An error occurred while creating the course.";
                model.Teachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EnrollStudents(int courseId)
        {
            try
            {
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    return NotFound();
                }

                var students = (await _userManager.GetUsersInRoleAsync("Student")).ToList();
                var enrolledStudentIds = await _context.UserCourses
                    .Where(uc => uc.CourseId == courseId)
                    .Select(uc => uc.UserId)
                    .ToListAsync();

                var model = new EnrollmentViewModel
                {
                    CourseId = courseId,
                    CourseName = course.Name,
                    Students = students.Select(s => new EnrollmentRecordViewModel
                    {
                        StudentId = s.Id,
                        StudentName = s.FullName,
                        IsEnrolled = enrolledStudentIds.Contains(s.Id)
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading enrollment page for course ID: {courseId}");
                TempData["ErrorMessage"] = "An error occurred while loading enrollment data.";
                return RedirectToAction("ManageCourses");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudents(EnrollmentViewModel model)
        {
            try
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course == null)
                {
                    return NotFound();
                }

                var currentEnrollments = await _context.UserCourses
                    .Where(uc => uc.CourseId == model.CourseId)
                    .ToListAsync();

                foreach (var student in model.Students)
                {
                    var existing = currentEnrollments.FirstOrDefault(uc => uc.UserId == student.StudentId);

                    if (student.IsEnrolled && existing == null)
                    {
                        _context.UserCourses.Add(new UserCourse
                        {
                            UserId = student.StudentId,
                            CourseId = model.CourseId,
                            EnrollmentDate = DateTime.Now
                        });
                    }
                    else if (!student.IsEnrolled && existing != null)
                    {
                        _context.UserCourses.Remove(existing);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student enrollments updated successfully";
                return RedirectToAction("ManageCourses");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating enrollments for course ID: {model.CourseId}");
                TempData["ErrorMessage"] = "An error occurred while updating enrollments.";
                return RedirectToAction("EnrollStudents", new { courseId = model.CourseId });
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }

}