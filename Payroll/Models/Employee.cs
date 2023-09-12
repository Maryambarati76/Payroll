using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Payroll.Models
{
    public partial class Employee
    {
        public Employee()
        {
            Salary = new HashSet<Salary>();
        }

        public int EmployeeId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public ICollection<Salary> Salary { get; set; }
    }
}
