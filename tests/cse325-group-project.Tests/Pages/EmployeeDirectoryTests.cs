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
    public void CreateButtonLinksToAddPage()
    {
        var page = Render<EmployeeDirectory>();

        var create = page.Find("a.btn-primary");
        Assert.Equal("Create New Record", create.TextContent.Trim());
        Assert.Equal("directory/add", create.GetAttribute("href"));
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

    [Fact]
    public void DirectoryPageRequiresLogin()
    {
        Assert.NotNull(typeof(EmployeeDirectory).GetCustomAttribute<AuthorizeAttribute>());
    }

    [Theory]
    [InlineData(typeof(EmployeeDirectory), "/directory")]
    [InlineData(typeof(AddEmployee), "/directory/add")]
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
