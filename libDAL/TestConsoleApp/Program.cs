using System.Linq;
using Models;
using DataRepositories;

namespace TestConsoleApp
{
    class Program
    {
        static async void Main(string[] args)
        {
            // Instantiate a new SQL Server connection
            var conn = "Data Source=SQL01;Initial Catalog=db;Persist Security Info=True;User ID=user;Password=pass";
            var db = new MSSQLRepository(conn);

            // Create a new 'Employee' with auto generated id
            var newEmp = new Employee() { Name = "Jeremy Johnson", Email = "jmjohnson@ci.davenport.ia.us" };
            var newTask = db.NewAsync("INSERT INTO [Employees] ([Name], [Email]) VALUES (@Name, @Email)", record: newEmp);
            newEmp.ID = newTask.Result;

            // Get all 'Employees'
            var getTask = db.GetAsync<Employee>("SELECT [ID],[Name],[Email] FROM [Employees]");
            var emps = getTask.Result;

            // Bulk insert all 'Employees' into the 'PR_Employees' table
            await db.NewAsync(emps, "PR_Employees");

            // Get 'Employee' with id 123
            (string, object)empID = (nameof(Employee.ID), 123);
            getTask = db.GetAsync<Employee>("SELECT [ID],[Name],[Email] FROM [Employees] WHERE [ID] = @ID", @params: new[] { empID });
            var emp123 = getTask.Result.First();

            // Update email address for 'Employee' 123
            emp123.Email = "new@email.com";
            await db.EditAsync("UPDATE [Employees] SET [Email] = '@Email' WHERE [ID] = @ID", record: emp123);

            // Delete 'Employee' 123
            await db.RemoveAsync("DELETE FROM [Employees] WHERE [ID] = @ID", record: emp123);
        }
    }
}
