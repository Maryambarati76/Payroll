using Payroll.Controllers;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Payroll.Services;

namespace Payroll.Test
{
    public class PayrollControllerTests
    {


        private readonly EmployeeController _controller;
        private readonly Mock<IEmployeeService> _mockDbContext;

        public PayrollControllerTests()
        {
            _mockDbContext = new Mock<IEmployeeService>();
            _controller = new EmployeeController(_mockDbContext.Object);
        }
        [Fact]
        public void GetRecord_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var requestBody = new RequestBodyModel
            {
                Data = "FirstName/LastName/BasicSalary/Allowance/Transportation/Date\\r\\n\r\nAli/Ahmadi/1200000/400000/350000/14010801",
                OverTimeCalculator = "CalcurlatorB"
            };

             _controller.AddRecord("custom", requestBody);


            // Act
            var result = _controller.GetSalaryInfoForMonth(1, "14010801");

            // Assert
            Assert.IsType<OkObjectResult>(result.Result.Result);
        }
        [Fact]
        public void AddRecord_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var requestBody = new RequestBodyModel
            {
                Data = "FirstName/LastName/BasicSalary/Allowance/Transportation/Date\\r\\n\r\nAli/Ahmadi/1200000/400000/350000/14010801",
                OverTimeCalculator = "CalcurlatorB"
            };

            // Act
            var result = _controller.AddRecord("custom", requestBody);

            // Assert
            Assert.IsType<OkResult>(result);
        }

    }
}