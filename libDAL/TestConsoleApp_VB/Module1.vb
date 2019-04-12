Imports Models
Imports DataRepositories

Module Module1

    Sub Main()

        ' Instantiate a new SQL Server connection
        Dim conn = "Data Source=SQLSERVER;Initial Catalog=db;Persist Security Info=True;User ID=user;Password=pass"
        Dim db As New MSSQLRepository(conn)

        ' Create a new 'Employee' with auto generated id
        Dim newEmp As New Employee() With {.Name = "Jeremy Johnson", .Email = "jmjohnson@ci.davenport.ia.us"}
        newEmp.ID = db.[New]("INSERT INTO [Employees] ([Name], [Email]) VALUES (@Name, @Email)", record:=newEmp)

        ' Get all 'Employees'
        Dim emps = db.Get(Of Employee)("SELECT [ID], [Name], [Email] FROM [Employees]")

        ' Get 'Employee' with id 123
        Dim empID = (NameOf(Employee.ID), 123)
        Dim emp123 = db.Get(Of Employee)("SELECT [ID], [Name], [Email] FROM [Employees] WHERE [ID] = @ID", params:={empID}).First()

        ' Update email address for 'Employee' 123
        emp123.Email = "new@email.com"
        db.Edit("UPDATE [Employees] SET [Email] = '@Email' WHERE [ID] = @ID", record:=emp123)

        ' Delete 'Employee' 123
        db.Remove("DELETE FROM [Employees] WHERE [ID] = @ID", record:=emp123)

    End Sub

End Module
