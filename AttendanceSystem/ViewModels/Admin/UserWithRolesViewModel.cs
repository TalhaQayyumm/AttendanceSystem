using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels.Admin
{
    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }
}