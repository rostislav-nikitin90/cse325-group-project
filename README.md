# CSE 325 Group Project

## Team Members
- Christian Ifeanyichukwu Ahanonu — [christian-ahanonu](https://github.com/christian-ahanonu)
- Francis Happy — [francis-2008-happy](https://github.com/francis-2008-happy)
- Praise Enato — [Praise-Enato](https://github.com/Praise-Enato)
- Rostislav Mikhaylovich Nikitin — [rostislav-nikitin90](https://github.com/rostislav-nikitin90)

## Project Overview
This group project is a **.NET Blazor web application** designed and developed collaboratively as part of the CSE 325 course.  
The application will include:
- User authentication  
- CRUD functionality
- Azure SQL Database integration  
- Responsive and accessible design following WCAG 2.1 standards  
- Deployment to a cloud service  

The project emphasizes teamwork, professional collaboration, and applying the **.NET development ecosystem** to create a complete, functional, and usable application.

## Tech Stack
- Front-End: Blazor (C# with Razor components)  
- Back-End: .NET Core with Entity Framework  
- Project Management: Trello Board  
- Code Management: Git & GitHub  
- Deployment: Cloud service  

## Getting Started
```bash
git clone https://github.com/rostislav-nikitin90/cse325-group-project.git
cd [folder]
dotnet restore
dotnet run
```

## User Guide

### Home Page
The Home page allows users to access the system.

- Click **Login** to sign in with an existing HR account.
- Click **Signup** to create a new HR account.
- After successful login, the Login and Signup buttons are replaced with a Logout button.

### HR Profile Page
The HR Profile page allows HR users to manage their account.

- Click **Create Account** to register a new HR account.
- Fill in all required fields and click **Submit**.
- Click **Cancel** to close the form without saving.
- Logged-in users can click **Edit Account** to update their account information.

### Employee Directory Page
The Employee Directory page allows HR users to manage employee records.

- View all employee records stored in the database.
- Click **Create New Record** to add a new employee.
- Click **Edit** to update an employee's information.
- Click **Delete** to remove an employee from the database.
- Click **Details** to view complete employee information, including:
  - Employee ID
  - First Name
  - Last Name
  - Email
  - Job Position
  - Department
  - Employment Status
  - Employment Start Date
  - Responsibilities Description

### Authentication
Only authenticated HR users can access employee management features.

### Database
Employee and HR account information is stored in an Azure SQL Database.