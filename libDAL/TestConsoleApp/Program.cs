using System.Linq;
using Models;
using DataRepositories;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate a new SQL Server connection
            var conn = "Data Source=SQL01;Initial Catalog=db;Persist Security Info=True;User ID=user;Password=pass";
            var db = new MSSQLRepository(conn);

            // Create a new 'Employee' with auto generated id
            var newEmp = new Employee() { Name = "Jeremy Johnson", Email = "jmjohnson@ci.davenport.ia.us" };
            newEmp.ID = db.New("INSERT INTO [Employees] ([Name], [Email]) VALUES (@Name, @Email)", record: newEmp);

            // Get all 'Employees'
            var emps = db.Get<Employee>("SELECT [ID],[Name],[Email] FROM [Employees]");

            // Bulk insert all 'Employees' into the 'PR_Employees' table
            db.New(emps, "PR_Employees");

            // Get 'Employee' with id 123
            (string, object)empID = (nameof(Employee.ID), 123);
            var emp123 = db.Get<Employee>("SELECT [ID],[Name],[Email] FROM [Employees] WHERE [ID] = @ID", @params: new[] {empID}).First();

            // Update email address for 'Employee' 123
            emp123.Email = "new@email.com";
            db.Edit("UPDATE [Employees] SET [Email] = '@Email' WHERE [ID] = @ID", record: emp123);

            // Delete 'Employee' 123
            db.Remove("DELETE FROM [Employees] WHERE [ID] = @ID", record: emp123);
        }
    }
}
