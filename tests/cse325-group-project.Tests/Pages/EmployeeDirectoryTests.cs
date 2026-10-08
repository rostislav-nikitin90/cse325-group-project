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
        public Func<Task<IReadOnlyList<Employee>>> Result { get; set; } =
            () => Task.FromResult<IReadOnlyList<Employee>>([]);

        public Task<IReadOnlyList<Employee>> GetAllEmployeesAsync() => Result();
    }

    private readonly FakeEmployeeService _fakeService = new();

    public EmployeeDirectoryTests()
    {
        // No authentication services are registered: the page must work for anonymous visitors
        Services.AddLogging();
        Services.AddSingleton<IEmployeeService>(_fakeService);
    }

    private static List<Employee> SampleEmployees() =>
    [
        new() { EmployeeId = 1, FirstName = "Ada", LastName = "Lovelace", Email = "ada@ems.com", Department = "IT" },
        new() { EmployeeId = 2, FirstName = "Alan", LastName = "Turing", Email = "alan@ems.com" }
    ];

    [Fact]
    public void ShowsEveryEmployeeFromTheService()
    {
        _fakeService.Result = () => Task.FromResult<IReadOnlyList<Employee>>(SampleEmployees());

        var page = Render<EmployeeDirectory>();

        var rows = page.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        Assert.Contains("Ada Lovelace", rows[0].TextContent);
        Assert.Contains("IT", rows[0].TextContent);
        Assert.Contains("ada@ems.com", rows[0].TextContent);
        Assert.Contains("Alan Turing", rows[1].TextContent);
        Assert.Contains("2 employee(s)", page.Find("caption").TextContent);
    }

    [Fact]
    public void ShowsLoadingMessageWhileWaitingForTheDatabase()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<Employee>>();
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
    public void AddButtonLinksToAddPage()
    {
        var page = Render<EmployeeDirectory>();

        var add = page.Find("a.btn-primary");
        Assert.Equal("Add Employee", add.TextContent.Trim());
        Assert.Equal("directory/add", add.GetAttribute("href"));
    }

    [Fact]
    public void EachRowHasEditAndDeleteLinksForThatEmployee()
    {
        _fakeService.Result = () => Task.FromResult<IReadOnlyList<Employee>>(SampleEmployees());

        var page = Render<EmployeeDirectory>();

        var links = page.FindAll("tbody tr")[1].QuerySelectorAll("a.btn");
        Assert.Equal("directory/edit/2", links[0].GetAttribute("href"));
        Assert.Equal("directory/delete/2", links[1].GetAttribute("href"));
        Assert.Equal("Delete Alan Turing", links[1].GetAttribute("aria-label"));
    }

    [Fact]
    public void DirectoryPageDoesNotRequireLogin()
    {
        Assert.Null(typeof(EmployeeDirectory).GetCustomAttribute<AuthorizeAttribute>());
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
