using System.Reflection;
using Bunit;
using cse325_group_project.Components.Pages;
using cse325_group_project.Components.Pages.Employees;
using cse325_group_project.Models;
using cse325_group_project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace cse325_group_project.Tests.Pages;

// Tests for the Employee Directory page, using a fake employee service instead of the database.
public class EmployeeDirectoryTests : BunitContext
{
    // Fake service: returns whatever task the test gives it
    private class FakeEmployeeService : IEmployeeService
    {
        public Func<Task<List<Employee>>> Result { get; set; } =
            () => Task.FromResult(new List<Employee>());

        public Task<List<Employee>> GetAllEmployeesAsync() => Result();

        // Employees passed to CreateEmployeeAsync
        public List<Employee> Created { get; } = [];

        // Set this to make CreateEmployeeAsync fail
        public Exception? CreateError { get; set; }

        public Task CreateEmployeeAsync(Employee employee)
        {
            if (CreateError != null)
            {
                throw CreateError;
            }

            Created.Add(employee);
            return Task.CompletedTask;
        }
    }

    private readonly FakeEmployeeService _fakeService = new();

    public EmployeeDirectoryTests()
    {
        Services.AddLogging();
        Services.AddSingleton<IEmployeeService>(_fakeService);
    }

    private static List<Employee> SampleEmployees() =>
    [
        new() { EmployeeId = 1, FirstName = "Ada", LastName = "Lovelace", Email = "ada@ems.com",
                Position = "Engineer", Department = "IT", Status = "Active",
                StartDate = new DateTime(2024, 3, 15), Responsibilities = "Builds the analytical engine" },
        new() { EmployeeId = 2, FirstName = "Alan", LastName = "Turing", Email = "alan@ems.com",
                Position = "Analyst", Department = "Research", Status = "Active",
                StartDate = new DateTime(2023, 6, 1), Responsibilities = "Breaks codes" }
    ];

    private void ReturnSampleEmployees() =>
        _fakeService.Result = () => Task.FromResult(SampleEmployees());

    [Fact]
    public void ShowsEveryEmployeeFromTheService()
    {
        ReturnSampleEmployees();

        var page = Render<EmployeeDirectory>();

        var rows = page.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        Assert.Contains("Ada", rows[0].TextContent);
        Assert.Contains("Lovelace", rows[0].TextContent);
        Assert.Contains("ada@ems.com", rows[0].TextContent);
        Assert.Contains("Engineer", rows[0].TextContent);
        Assert.Contains("Turing", rows[1].TextContent);
    }

    [Fact]
    public void ShowsLoadingMessageWhileWaitingForTheDatabase()
    {
        var pending = new TaskCompletionSource<List<Employee>>();
        _fakeService.Result = () => pending.Task;

        var page = Render<EmployeeDirectory>();
        Assert.Contains("Loading employees", page.Markup);

        pending.SetResult(SampleEmployees());
        page.WaitForAssertion(() => Assert.Equal(2, page.FindAll("tbody tr").Count));
    }

    [Fact]
    public void ShowsEmptyMessageWhenThereAreNoEmployees()
    {
        var page = Render<EmployeeDirectory>();

        Assert.Contains("No employees found.", page.Markup);
        Assert.Empty(page.FindAll("table"));
    }

    [Fact]
    public void ShowsErrorMessageWhenTheDatabaseFails()
    {
        _fakeService.Result = () => throw new InvalidOperationException("database is down");

        var page = Render<EmployeeDirectory>();

        Assert.Contains("could not be loaded", page.Find(".alert-danger").TextContent);
        Assert.Empty(page.FindAll("table"));
    }


    [Fact]
    public void EachRowHasEditAndDeleteLinksForThatEmployee()
    {
        ReturnSampleEmployees();

        var page = Render<EmployeeDirectory>();

        var links = page.FindAll("tbody tr")[1].QuerySelectorAll("a.employee-action-button");
        Assert.Equal("directory/edit/2", links[0].GetAttribute("href"));
        Assert.Equal("Edit Alan Turing", links[0].GetAttribute("aria-label"));
        Assert.Equal("directory/delete/2", links[1].GetAttribute("href"));
        Assert.Equal("Delete Alan Turing", links[1].GetAttribute("aria-label"));
    }

    [Fact]
    public void DetailsButtonOpensAndClosesTheDetailsPopup()
    {
        ReturnSampleEmployees();
        var page = Render<EmployeeDirectory>();
        Assert.Empty(page.FindAll(".employee-modal"));

        page.Find("button[aria-label='View details for Ada Lovelace']").Click();

        var modal = page.Find(".employee-modal");
        Assert.Contains("IT", modal.TextContent);
        Assert.Contains("Active", modal.TextContent);
        Assert.Contains("Builds the analytical engine", modal.TextContent);

        page.Find(".employee-modal-footer button").Click();
        Assert.Empty(page.FindAll(".employee-modal"));
    }

    // Fills in every field of the Create New Record form with valid values
    private static void FillCreateForm(IRenderedComponent<EmployeeDirectory> page, int id = 3)
    {
        page.Find("#create-employee-id").Change(id.ToString());
        page.Find("#create-first-name").Change("Grace");
        page.Find("#create-last-name").Change("Hopper");
        page.Find("#create-email").Change("grace@ems.com");
        page.Find("#create-position").Change("Frontend Engineer");
        page.Find("#create-department").Change("Engineering");
        page.Find("#create-status").Change("Active");
        page.Find("#create-start-date").Change("2026-09-01");
        page.Find("#create-responsibilities").Change("Writes compilers");
    }

    private IRenderedComponent<EmployeeDirectory> RenderAndOpenCreateForm()
    {
        ReturnSampleEmployees();
        var page = Render<EmployeeDirectory>();
        page.Find("button.btn-primary").Click();
        return page;
    }

    [Fact]
    public void CreateNewRecordButtonOpensTheForm()
    {
        var page = Render<EmployeeDirectory>();
        Assert.Empty(page.FindAll("#create-employee-title"));

        page.Find("button.btn-primary").Click();

        Assert.Equal("Create New Record", page.Find("#create-employee-title").TextContent);
        Assert.NotNull(page.Find("#create-responsibilities"));
    }

    [Fact]
    public void CancelClosesTheFormWithoutSaving()
    {
        var page = RenderAndOpenCreateForm();
        FillCreateForm(page);

        page.Find(".employee-modal-footer button.btn-secondary").Click();

        Assert.Empty(page.FindAll("#create-employee-title"));
        Assert.Empty(_fakeService.Created);
        Assert.Equal(2, page.FindAll("tbody tr").Count);
    }

    [Fact]
    public void SubmittingAnEmptyFormShowsErrorsAndDoesNotSave()
    {
        var page = RenderAndOpenCreateForm();
        page.Find("#create-employee-id").Change("");
        page.Find("#create-start-date").Change("");

        page.Find("form").Submit();

        var errors = page.FindAll(".validation-message").Select(e => e.TextContent).ToList();
        Assert.Contains("The Employee ID field is required.", errors);
        Assert.Contains("The First Name field is required.", errors);
        Assert.Contains("The Start Date field is required.", errors);
        Assert.Contains("The Responsibilities field is required.", errors);
        Assert.Empty(_fakeService.Created);
        Assert.NotNull(page.Find("#create-employee-title"));
    }

    [Fact]
    public void InvalidEmailShowsAnErrorAndDoesNotSave()
    {
        var page = RenderAndOpenCreateForm();
        FillCreateForm(page);
        page.Find("#create-email").Change("not-an-email");

        page.Find("form").Submit();

        Assert.Contains("The Email field is not a valid email address.", page.Markup);
        Assert.Empty(_fakeService.Created);
    }

    [Fact]
    public void ValidFormSavesTheEmployeeAndAddsItToTheList()
    {
        var page = RenderAndOpenCreateForm();
        FillCreateForm(page);

        page.Find("form").Submit();

        var saved = Assert.Single(_fakeService.Created);
        Assert.Equal(3, saved.EmployeeId);
        Assert.Equal("Grace", saved.FirstName);
        Assert.Equal("grace@ems.com", saved.Email);
        Assert.Equal(new DateTime(2026, 9, 1), saved.StartDate);
        Assert.Equal("Writes compilers", saved.Responsibilities);

        Assert.Empty(page.FindAll("#create-employee-title"));
        var rows = page.FindAll("tbody tr");
        Assert.Equal(3, rows.Count);
        Assert.Contains("Hopper", rows[2].TextContent);
        Assert.Contains("Grace Hopper has been added.", page.Find(".alert-success").TextContent);
    }

    [Fact]
    public void DuplicateIdKeepsTheFormOpenWithAMessage()
    {
        var page = RenderAndOpenCreateForm();
        _fakeService.CreateError = new DuplicateEmployeeException(new Exception());
        FillCreateForm(page, id: 1);

        page.Find("form").Submit();

        Assert.Contains("already exists", page.Find(".employee-modal .alert-danger").TextContent);
        Assert.NotNull(page.Find("#create-employee-title"));
        Assert.Equal(2, page.FindAll("tbody tr").Count);
    }

    [Fact]
    public void DropdownsOfferOnlyTheValuesTheDatabaseAllows()
    {
        var page = RenderAndOpenCreateForm();

        string[] OptionsOf(string id) => page.FindAll($"{id} option")
            .Select(o => o.GetAttribute("value")!)
            .Where(v => v != "")
            .ToArray();

        Assert.Equal(EmployeeOptions.Positions, OptionsOf("#create-position"));
        Assert.Equal(EmployeeOptions.Departments, OptionsOf("#create-department"));
        Assert.Equal(EmployeeOptions.Statuses, OptionsOf("#create-status"));
    }

    [Fact]
    public void ValueRejectedByTheDatabaseKeepsTheFormOpenWithAMessage()
    {
        var page = RenderAndOpenCreateForm();
        _fakeService.CreateError = new InvalidEmployeeValueException(new Exception());
        FillCreateForm(page);

        page.Find("form").Submit();

        Assert.Contains("does not accept the selected Position, Department or Status",
            page.Find(".employee-modal .alert-danger").TextContent);
        Assert.NotNull(page.Find("#create-employee-title"));
    }

    [Fact]
    public void DatabaseErrorKeepsTheFormOpenWithAMessage()
    {
        var page = RenderAndOpenCreateForm();
        _fakeService.CreateError = new InvalidOperationException("database is down");
        FillCreateForm(page);

        page.Find("form").Submit();

        Assert.Contains("Unable to create the employee", page.Find(".employee-modal .alert-danger").TextContent);
        Assert.NotNull(page.Find("#create-employee-title"));
    }

    [Fact]
    public void DirectoryPageRequiresLogin()
    {
        Assert.NotNull(typeof(EmployeeDirectory).GetCustomAttribute<AuthorizeAttribute>());
    }

    [Theory]
    [InlineData(typeof(EmployeeDirectory), "/directory")]
    [InlineData(typeof(EditEmployee), "/directory/edit/{Id:int}")]
    [InlineData(typeof(DeleteEmployee), "/directory/delete/{Id:int}")]
    public void PagesUseTheExpectedRoutes(Type page, string route)
    {
        Assert.Equal(route, page.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void EditAndDeletePagesShowTheEmployeeId()
    {
        var edit = Render<EditEmployee>(p => p.Add(x => x.Id, 5));
        var delete = Render<DeleteEmployee>(p => p.Add(x => x.Id, 5));

        Assert.Contains("employee #5", edit.Markup);
        Assert.Contains("employee #5", delete.Markup);
    }
}
