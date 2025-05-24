// In ViewModels/Admin/EditUserViewModel.cs
using System.Collections.Generic;

namespace AttendanceSystem.ViewModels.Admin
{
    public class EditUserViewModel
    {
        public EditUserViewModel()  // Add constructor
        {
            CurrentRoles = new List<string>();
            AvailableRoles = new List<string>();
            SelectedRoles = new List<string>();
        }

        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> CurrentRoles { get; set; }
        public List<string> AvailableRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}