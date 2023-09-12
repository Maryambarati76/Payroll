using Payroll.Controllers;
using Payroll.Models;
using System.Reflection;

namespace Payroll.Services
{
    public interface IEmployeeService
    {
        Task<List<Salary>> GetSalaryInfoForMonth(int id, string date);
        EmployeeSalary ExtractDataFromBody(string datatype, RequestBodyModel requestBody);
        Employee GetOrCreateEmployee(string firstName, string lastName);
        Salary GetSalaryByEmployeeAndDate(int employeeId,DateTime date);
        Salary CreatSalary(Employee employee,string overTimeCalculator, EmployeeSalary model,MethodInfo methodInfo);

        Task<Salary> CreateData(RequestBodyModel requestBody, EmployeeSalary model);
        void Delete(int id);
        string GetConnectionString();
    }
}
