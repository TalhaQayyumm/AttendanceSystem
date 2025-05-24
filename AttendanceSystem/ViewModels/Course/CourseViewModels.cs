using AttendanceSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels.Courses
{
    public class CourseViewModel : CourseFormViewModel
    {
        public int CourseId { get; set; }
    }
}