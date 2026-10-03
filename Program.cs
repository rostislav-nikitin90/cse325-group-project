using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using cse325_group_project.Components;
using cse325_group_project.Data;
using cse325_group_project.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Data and Authentication services
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Login cookie: remembers who is logged in
// If a logged out user opens a page that needs login, send them to the home page
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// The login form on the home page sends the email and password here
app.MapPost("/login", async ([FromForm] string email, [FromForm] string password, IAuthService authService, HttpContext context) =>
{
    var user = await authService.AuthenticateAsync(email, password);

    // Wrong email or password: go back to the home page and show the error
    if (user == null)
    {
        return Results.Redirect("/?loginError=true");
    }

    // Save the user's name and email in the login cookie
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
        new Claim(ClaimTypes.Email, user.Email)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(new ClaimsPrincipal(identity));

    return Results.Redirect("/directory");
});

// The Logout button sends the user here, and the login cookie is removed
app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync();
    return Results.Redirect("/");
});


// --- Quick Database & Auth Connection Test ---
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

    Console.WriteLine("\n================ TESTING HR USER CREATION & AUTH ================");
    try
    {
        // 1. Check if the test HR user already exists
        var existingUser = await authService.GetHrByEmailAsync("james.friday@ems.com");
        if (existingUser == null)
        {
            Console.WriteLine("[INFO] Creating HR user: James Friday (ID: 5)...");
            bool created = await authService.CreateHrUserAsync(
                hrId: 5,
                firstName: "James",
                lastName: "Friday",
                email: "james.friday@ems.com",
                plaintextPassword: "Jerobin$12"
            );

            if (created)
            {
                Console.WriteLine("[SUCCESS] HR user created and password hashed with BCrypt!");
            }
        }
        else
        {
            Console.WriteLine($"[INFO] HR user already exists in database (HR ID: {existingUser.HrId}). Skipping creation.");
        }

        // 2. Test authenticating with the new HR user's credentials
        Console.WriteLine("[INFO] Verifying authentication with plain password 'Jerobin$12'...");
        var authenticatedUser = await authService.AuthenticateAsync("james.friday@ems.com", "Jerobin$12");

        if (authenticatedUser != null)
        {
            Console.WriteLine($"[SUCCESS] Authentication PASSED!");
            Console.WriteLine($"Logged in as: {authenticatedUser.FirstName} {authenticatedUser.LastName} ({authenticatedUser.Email}), HR ID: {authenticatedUser.HrId}");
        }
        else
        {
            Console.WriteLine("[FAILED] Authentication returned null.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Database operation failed: {ex.Message}");
    }
    Console.WriteLine("=================================================================\n");
}


app.Run();
