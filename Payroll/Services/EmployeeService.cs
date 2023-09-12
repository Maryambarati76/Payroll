using Dapper;
using Payroll.Controllers;
using Payroll.Data;
using Payroll.Helper;
using Payroll.Models;
using System.Data.SqlClient;
using System.Reflection;

namespace Payroll.Services
{
    public class EmployeeService : IEmployeeService
    {

        private readonly PayrollDbContext _context;
        private readonly IConfiguration _config;
        public EmployeeService(PayrollDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<List<Salary>> GetSalaryInfoForMonth(int id, string date)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("SqlServer"));
            string sql = @"SELECT * 
               FROM Salary 
               INNER JOIN Employee ON Employee.EmployeeId=Salary.EmployeeId 
               WHERE Employee.EmployeeId = @EmployeeId AND Salary.Date = @Date";



            DateTime dateTime = DateTimeHelper.ConverPersianDateToMiladi(date);

            var parameters = new { EmployeeId = id, Date = dateTime };

            IEnumerable<Salary> result = await connection.QueryAsync<Salary, Employee, Salary>(sql, (salary, employee) =>
            {
                salary.Employee = employee;
                return salary;
            }, parameters, splitOn: "EmployeeId");

            var salaries = result.ToList();


            return salaries;
        }
        public async Task<Salary> CreateData(RequestBodyModel requestBody, EmployeeSalary model)
        {
            var employee = _context.Employee.SingleOrDefault(x => x.FirstName == model.FirstName && x.LastName == model.LastName);
            if (employee == null)
            {
                employee = new Employee { FirstName = model.FirstName, LastName = model.LastName };
                _context.Employee.Add(employee);
                await _context.SaveChangesAsync();
            }
            var salary = _context.Salary.SingleOrDefault(x => x.Date == model.Date && x.Employee.EmployeeId == employee.EmployeeId);
            if (salary != null)
                throw new Exception("An employee can not have more than 1 salary per date");


            var classInstance = new OvetimePolicies.OvetimePolicies();

            MethodInfo method = classInstance.GetType().GetMethod(requestBody.OverTimeCalculator);

            if (method == null) throw new Exception("Calc method not found");


            object[] methodParams = new object[] { model.BasicSalary, model.Allowance };

            var overTimeResult = (decimal)method.Invoke(classInstance, methodParams);
            salary = new Salary
            {
                Employee = employee,
                Allowance = model.Allowance,
                Transportation = model.Transportation,
                BasicSalary = model.BasicSalary,
                Date = model.Date,
                Amount = model.BasicSalary + model.Allowance + model.Transportation + overTimeResult
            };
            _context.Salary.Add(salary);
            await _context.SaveChangesAsync();
            return salary;

        }

        public async Task<Salary> UpdateData(int salaryId, RequestBodyModel requestBody, EmployeeSalary model)
        {
            var salary = _context.Salary.SingleOrDefault(x => x.SalaryId == salaryId);
            if (salary == null)
                throw new Exception("Salary Not Found");


            var employee = _context.Employee.SingleOrDefault(x => x.EmployeeId == salary.EmployeeId);
            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            _context.Employee.Update(employee);
            await _context.SaveChangesAsync();


            var classInstance = new OvetimePolicies.OvetimePolicies();

            MethodInfo method = classInstance.GetType().GetMethod(requestBody.OverTimeCalculator);

            if (method == null) throw new Exception("Calc method not found");


            object[] methodParams = new object[] { model.BasicSalary, model.Allowance };

            var overTimeResult = (decimal)method.Invoke(classInstance, methodParams);

            salary.Allowance = model.Allowance;
            salary.Transportation = model.Transportation;
            salary.BasicSalary = model.BasicSalary;
            salary.Date = model.Date;
            salary.Amount = model.BasicSalary + model.Allowance + model.Transportation + overTimeResult;
         
            _context.Salary.Update(salary);
            await _context.SaveChangesAsync();
            return salary;

        }

        public void Delete(int id)
        {
            var salary = _context.Salary.Find(id);
            if (salary == null)
                return;
            _context.Salary.Remove(salary);
            _context.SaveChanges();
        }

        public string GetConnectionString()
        {
            return _config.GetConnectionString("SqlServer");
        }
    }
}
