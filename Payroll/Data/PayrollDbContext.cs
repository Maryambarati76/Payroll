using Microsoft.EntityFrameworkCore;
using Payroll.Models;

namespace Payroll.Data
{
    public partial class PayrollDbContext : DbContext
    {
        public PayrollDbContext()
        {
        }

        public PayrollDbContext(DbContextOptions<PayrollDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<Salary> Salary { get; set; }

       
    }
}
