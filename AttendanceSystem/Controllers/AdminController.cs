using AttendanceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.ViewModels;
using AttendanceSystem.ViewModels.Courses;

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

        // Dashboard actions
        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = new AdminDashboardViewModel
                {
                    TotalStudents = (await _userManager.GetUsersInRoleAsync("Student")).ToList(),
                    TotalTeachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList(),
                    TotalCourses = await _context.Courses.CountAsync(),
                    RecentAttendances = await _context.Attendances
                        .OrderByDescending(a => a.Date)
                        .Take(10)
                        .Include(a => a.Student)
                        .Include(a => a.Course)
                        .ToListAsync()
                };

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Error", "Home");
            }
        }

        // User Management actions
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
                    AllRoles = allRoles.Select(r => r.Name).ToList()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FullName = model.FullName;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    AddErrors(updateResult);
                    model.AllRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                    return View(model);
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    AddErrors(removeResult);
                    model.AllRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                    return View(model);
                }

                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    if (!addResult.Succeeded)
                    {
                        AddErrors(addResult);
                        model.AllRoles = (await _roleManager.Roles.ToListAsync()).Select(r => r.Name).ToList();
                        return View(model);
                    }
                }

                TempData["SuccessMessage"] = "User updated successfully";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID: {model.UserId}");
                TempData["ErrorMessage"] = "An error occurred while updating the user.";
                return RedirectToAction("EditUser", new { id = model.UserId });
            }
        }

        // Course Management actions
        public async Task<IActionResult> ManageCourses()
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Teacher)
                    .ToListAsync();

                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading course management");
                TempData["ErrorMessage"] = "An error occurred while loading courses.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            try
            {
                var model = new CourseViewModel
                {
                    AvailableTeachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList()
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
        public async Task<IActionResult> CreateCourse(ViewModels.CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableTeachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList();
                return View(model);
            }

            try
            {
                var course = new Course
                {
                    Name = model.Name,
                    Description = model.Description,
                    TeacherId = model.TeacherId,
                    AllowedLatitude = model.AllowedLatitude,
                    AllowedLongitude = model.AllowedLongitude,
                    AllowedRadiusMeters = model.AllowedRadiusMeters,
                    ClassStartTime = model.ClassStartTime,
                    ClassEndTime = model.ClassEndTime,
                    ClassDay = model.ClassDay
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
                model.AvailableTeachers = (await _userManager.GetUsersInRoleAsync("Teacher")).ToList();
                return View(model);
            }
        }

        // Student Enrollment actions
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

                var model = new EnrollStudentsViewModel
                {
                    CourseId = courseId,
                    CourseName = course.Name,
                    Students = students.Select(s => new StudentEnrollmentViewModel
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
        public async Task<IActionResult> EnrollStudents(EnrollStudentsViewModel model)
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