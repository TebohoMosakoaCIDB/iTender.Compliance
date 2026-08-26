using iTender.Compliance.Application.Common;
using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Client.Components;
using iTender.Compliance.Client.Components.Account;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Extensions;
using iTender.Compliance.Infrastructure.Hubs;
using iTender.Compliance.Infrastructure.Identity;
using iTender.Compliance.Infrastructure.Persistence.Seeders;
using iTender.Compliance.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorization(options =>
{
    // =========================================================
    // ADMINISTRATION
    // =========================================================
    // System administration, configuration and administrative
    // functions.
    options.AddPolicy("Administration", policy =>
        policy.RequireRole(
            Roles.ComplianceAdministrator,
            Roles.ComplianceManager));

    // =========================================================
    // CASE ASSIGNMENT
    // =========================================================
    // The Compliance Manager leads the Compliance Unit and
    // therefore controls case assignment.
    options.AddPolicy("CaseAssignment", policy =>
        policy.RequireRole(
            Roles.ComplianceManager));

    // =========================================================
    // CASE MANAGEMENT
    // =========================================================
    // Officers work on cases assigned to them.
    // Managers oversee all cases.
    // Director has overall oversight.
    options.AddPolicy("CaseManagement", policy =>
        policy.RequireRole(
            Roles.Director,
            Roles.ComplianceManager,
            Roles.ComplianceOfficer));

    // =========================================================
    // COMPLIANCE OVERSIGHT
    // =========================================================
    // Director and Manager have management/oversight
    // responsibilities.
    options.AddPolicy("ComplianceOversight", policy =>
        policy.RequireRole(
            Roles.Director,
            Roles.ComplianceManager));

    // =========================================================
    // REPORTING
    // =========================================================
    // Officers prepare reports, while management and the
    // administrative function need access to reporting.
    options.AddPolicy("Reporting", policy =>
        policy.RequireRole(
            Roles.Director,
            Roles.ComplianceManager,
            Roles.ComplianceOfficer,
            Roles.ComplianceAdministrator));

    // =========================================================
    // ACCOUNT MANAGEMENT
    // =========================================================
    // Administrative function manages user accounts.
    options.AddPolicy("AccountManagement", policy =>
        policy.RequireRole(
            Roles.ComplianceAdministrator,
            Roles.ComplianceManager));

    // =========================================================
    // CORRESPONDENCE
    // =========================================================
    // Compliance Officers draft compliance correspondence and
    // Contravention Notices.
    // Compliance Managers provide oversight and quality assurance.
    // Administrative staff manage document control, filing and
    // correspondence tracking.
    options.AddPolicy("Correspondence", policy =>
        policy.RequireRole(
            Roles.Director,
            Roles.ComplianceManager,
            Roles.ComplianceOfficer,
            Roles.ComplianceAdministrator));

    options.AddPolicy("CorrespondenceDraft", policy =>
        policy.RequireRole(
            Roles.ComplianceManager,
            Roles.ComplianceOfficer));
});

var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection2") ?? throw new InvalidOperationException("Connection string 'SupabaseConnection' not found.");
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
.AddRoles<IdentityRole<Guid>>()   // <-- Add this
.AddEntityFrameworkStores<ComplianceDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

//data sources
builder.Services.AddHttpClient<IDataverseService, DataverseService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["iTenderApi:BaseUrl"]!);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//Data Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ComplianceDbContext>();

    //await context.Database.MigrateAsync();

    await IdentitySeeder.SeedAsync(services);

    await CorrespondenceTemplateSeeder.SeedAsync(context);
}

app.MapHub<NotificationHub>("/hubs/notifications");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
