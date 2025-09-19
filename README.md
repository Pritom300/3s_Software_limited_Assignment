Assignment (3s_Software_limited) <br />

**Project Overview** <br />

This is a RESTful API (.NET 9) with Domain-Driven Design (DDD) layered, Unity of work, Repository pattern and Generic repository architecture style application.

**Demo Video:** [Project Demo Video]  (https://drive.google.com/file/d/12O-9HaP72bgLEL3O4HnNn8KkOgekAMoy/view?usp=sharing)

**Feature Added** <br />
-> Authentication and Authrization with refresh token mechanism. <br />
-> CRUD operations for Products and Categories, including image upload (file path based) and search functionality. <br />
-> Swagger API documentation for testing. <br />
-> Domain-Driven Design (DDD) layered architecture. <br />
-> Repository Pattern + Generic Repository + Unit of Work implementation. <br />
-> Entity Framework Core ORM <br />
-> Used MSSQL for store data <br />




1. Setup Instructions: <br />
  ```git clone <your-repo-url>``` <br />

2. Update connection string in ```appsettings.json``` to connecting the database. <br />
   -> Open ```appsettings.json``` in the ```API``` project. and here update the server name and database name to point your local server.

3. ```Add-Migration InitialCreate3 -StartupProject API -Project Infrastructure```  (For Visual Studio) <br />
   ```dotnet ef migrations add InitialCreate -p Infrastructure -s API```           (For Visual Studio Code) <br />
             

3. ```Update-Database -StartupProject API -Project Infrastructure```                (For Visual Studio) <br />
   ```dotnet ef database update -p Infrastructure -s API ```                        (For Visual Studio Code) <br />
   

Restore the project```dotnet restore``` then build the project ```dotnet build``` and run.   <br />



Technologies Used: <br />
ASP.NET Core Web API (.NET 9) <br />
Entity Framework Core <br />
SQL Server <br />
Swagger <br/>


