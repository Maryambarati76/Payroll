using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Payroll.Controllers;
using Payroll.Data;
using Payroll.Helper;
using Payroll.Models;
using System.Data.SqlClient;
using System.Reflection;
using System.Xml.Serialization;

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
        public EmployeeSalary ExtractDataFromBody(string datatype, RequestBodyModel requestBody)
        {
            var data = requestBody.Data;
            datatype = datatype.ToLower();
            EmployeeSalary model = new EmployeeSalary();

            if (datatype == "json")
                model = JsonConvert.DeserializeObject<EmployeeSalary>(data);
            else if (datatype == "xml")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EmployeeSalary));
                using (TextReader reader = new StringReader(data))
                {
                    model = (EmployeeSalary)serializer.Deserialize(reader);
                }
            }
            else if (datatype == "custom")
            {

                data = data.Replace("\n", "/").Replace("\r", "/").Replace("//", "/").Trim();
                var allFields = data.Split('/');
                List<string> fields = new List<string>();
                List<string> values = new List<string>();
                foreach (var item in allFields)
                {
                    if (item.Contains("\\"))
                        continue;
                    if (item == "BasicSalary" || item == "Allowance" || item == "Transportation" || item == "Date" || item == "LastName" ||
                        item == "FirstName")
                    {
                        fields.Add(item);
                    }
                    else
                    {
                        values.Add(item);
                    }
                }
                for (int i = 0; i < fields.Count; i++)
                {
                    model.SetFieldValue(fields[i], values[i]);
                }

            }
            else if (datatype == "cs" || datatype == "csv")
            {

                var allFields = data.Split('/');
                List<string> fields = new List<string>() { "FirstName", "LastName", "BasicSalary", "Allowance", "Transportation", "Date" };
                List<string> values = new List<string>();

                for (int i = 0; i < fields.Count; i++)
                {
                    model.SetFieldValue(fields[i], values[i]);
                }
            }

            return model;

            //var employee = _context.Employee.SingleOrDefault(x => x.FirstName == model.FirstName && x.LastName == model.LastName);
            //if (employee == null)
            //{
            //    employee = new Employee { FirstName = model.FirstName, LastName = model.LastName };
            //    _context.Employee.Add(employee);
            //    _context.SaveChanges();
            //}

            //var salary = _context.Salary.SingleOrDefault(x => x.Date == model.Date && x.Employee.EmployeeId == employee.EmployeeId);
            //if (salary != null)
            //    return BadRequest("An employee can not have more than 1 salary per date");
            //string className = "OvetimePolicies.OvetimePolicies";


            //var classInstance = new OvetimePolicies.OvetimePolicies();

            //object[] methodParams = new object[] { model.BasicSalary, model.Allowance };
            //MethodInfo method = classInstance.GetType().GetMethod(requestBody.OverTimeCalculator);

            //if (method == null) return BadRequest("Calc method not found");
            //var overTimeResult = (decimal)method.Invoke(classInstance, methodParams);
            //salary = new Salary
            //{
            //    Employee = employee,
            //    Allowance = model.Allowance,
            //    Transportation = model.Transportation,
            //    BasicSalary = model.BasicSalary,
            //    Date = model.Date,
            //    Amount = model.BasicSalary + model.Allowance + model.Transportation + overTimeResult
            //};
            //_context.Salary.Add(salary);
            //_context.SaveChanges();

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
            await  _context.SaveChangesAsync();
            return salary;

        }

        public Salary GetSalaryByEmployeeAndDate(int employeeId, DateTime date)
        {
            return _context.Salary.SingleOrDefault(x => x.Date == date && x.Employee.EmployeeId == employeeId);

        }

        public Salary CreatSalary(Employee employee, string overTimeCalculator, EmployeeSalary model, MethodInfo methodInfo)
        {
            var classInstance = new OvetimePolicies.OvetimePolicies();

            object[] methodParams = new object[] { model.BasicSalary, model.Allowance };

            var overTimeResult = (decimal)methodInfo.Invoke(classInstance, methodParams);
            var salary = new Salary
            {
                Employee = employee,
                Allowance = model.Allowance,
                Transportation = model.Transportation,
                BasicSalary = model.BasicSalary,
                Date = model.Date,
                Amount = model.BasicSalary + model.Allowance + model.Transportation + overTimeResult
            };
            _context.Salary.Add(salary);
            _context.SaveChanges();
            return salary;
        }

        public Employee GetOrCreateEmployee(string firstName, string lastName)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var salary = _context.Salary.Find(id);
            if (salary == null)
                return ;
            _context.Salary.Remove(salary);
            _context.SaveChanges();
        }

        public string GetConnectionString()
        {
            return _config.GetConnectionString("SqlServer");
        }
    }
}
