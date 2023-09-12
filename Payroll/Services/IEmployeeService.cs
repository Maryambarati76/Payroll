using Payroll.Controllers;
using Payroll.Models;
using System.Reflection;

namespace Payroll.Services
{
    public interface IEmployeeService
    {
        Task<List<Salary>> GetSalaryInfoForMonth(int id, string date);
        Task<Salary> CreateData(RequestBodyModel requestBody, EmployeeSalary model);
        Task<Salary> UpdateData(int salaryId, RequestBodyModel requestBody, EmployeeSalary model);
        void Delete(int id);
        string GetConnectionString();
    }
}
