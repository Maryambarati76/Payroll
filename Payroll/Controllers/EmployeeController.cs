using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Xml.Serialization;
using Payroll.Models;
using System.Data.SqlClient;
using Dapper;
using Payroll.Helper;
using Payroll.Services;

namespace Payroll.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        [HttpPost("{datatype}")]
        public async Task<IActionResult> AddRecord([FromRoute] string datatype, [FromBody] RequestBodyModel requestBody)
        {
            var data = requestBody.Data;
            if (string.IsNullOrEmpty(data))
                return BadRequest(ModelState);

            datatype = datatype.ToLower();
            EmployeeSalary? model;
            try
            {
                model = ExtractDataFromBody(datatype, requestBody);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (model == null) model = new EmployeeSalary();
            try
            {
                var employee = await _employeeService.CreateData(requestBody, model);
                // employee.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpDelete("salaryId")]
        public IActionResult Delete(int id)
        {
            _employeeService.Delete(id);
            return Ok();

        }
        [HttpPut("{datatype}/{salaryId}")]
        public async Task<IActionResult> Update(int salaryId, string datatype, [FromBody] RequestBodyModel requestBody)
        {
            var data = requestBody.Data;
            if (string.IsNullOrEmpty(data))
                return BadRequest(ModelState);

            datatype = datatype.ToLower();
            EmployeeSalary? model;
            try
            {
                model = ExtractDataFromBody(datatype, requestBody);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (model == null) model = new EmployeeSalary();
            try
            {
                var employee = await _employeeService.UpdateData(salaryId, requestBody, model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();

        }
        [HttpGet("{employeeId}/{date}")]
        public async Task<ActionResult<List<EmployeeSalary>>> GetSalaryInfoForMonth(int employeeId, string date)
        {

            var s = await _employeeService.GetSalaryInfoForMonth(employeeId, date);
            return Ok(s);
        }


        [HttpGet("{employeeId}/{startDate}/{endDate}")]
        public async Task<ActionResult<List<EmployeeSalary>>> GetRange(int employeeId, string startDate, string endDate)
        {
           
            using var connection = new SqlConnection(_employeeService.GetConnectionString());
            string sql = @"SELECT * 
               FROM Salary 
               INNER JOIN Employee ON Employee.EmployeeId=Salary.EmployeeId 
               WHERE Employee.EmployeeId = @EmployeeId AND Salary.Date >= @StartDate and Salary.Date <= @EndDate ";


            DateTime start = DateTimeHelper.ConverPersianDateToMiladi(startDate);
            DateTime end = DateTimeHelper.ConverPersianDateToMiladi(endDate);

            var parameters = new { EmployeeId = employeeId, StartDate = start, EndDate = end };

            IEnumerable<Salary> result = await connection.QueryAsync<Salary, Employee, Salary>(sql, (salary, employee) =>
            {
                salary.Employee = employee;
                return salary;
            }, parameters, splitOn: "EmployeeId");

            var salaries = result.ToList();


            return Ok(salaries);
        }

        private EmployeeSalary ExtractDataFromBody(string datatype, RequestBodyModel requestBody)
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

                data = data.Replace("\\n", "").Replace("\\r", "").Replace("\n", "/").Replace("\r", "").Trim();
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
        }
    }

    [XmlRoot("EmployeeSalary")]
    public class EmployeeSalary
    {
        [XmlElement("FirstName")]
        public string FirstName { get; set; }
        [XmlElement("LastName")]
        public string LastName { get; set; }
        [XmlElement("Date")]
        public DateTime Date { get; set; }
        [XmlElement("Transportation")]
        public decimal? Transportation { get; set; }
        [XmlElement("Allowance")]
        public decimal? Allowance { get; set; }
        [XmlElement("BasicSalary")]
        public decimal BasicSalary { get; set; }
        public void SetFieldValue(string fieldName, string value)
        {
            switch (fieldName)
            {
                case "FirstName":
                    FirstName = value;
                    break;
                case "LastName":
                    LastName = value;
                    break;
                case "Date":
                    DateTime dateTime = DateTimeHelper.ConverPersianDateToMiladi(value);
                    Date = dateTime;
                    break;
                case "Transportation":
                    Transportation = decimal.Parse(value);
                    break;
                case "Allowance":
                    Allowance = decimal.Parse(value);
                    break;
                case "BasicSalary":
                    BasicSalary = decimal.Parse(value);
                    break;
            }
        }
    }
    public class RequestBodyModel
    {
        public string Data { get; set; }
        public string OverTimeCalculator { get; set; }
    }
}
