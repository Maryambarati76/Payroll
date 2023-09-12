using System;
using System.Collections.Generic;

namespace Payroll.Models
{
    public partial class Salary
    {
        public int SalaryId { get; set; }
        public decimal? Amount { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public decimal? Transportation { get; set; }
        public decimal? Allowance { get; set; }
        public decimal BasicSalary { get; set; }

        public Employee Employee { get; set; }
    }
}
