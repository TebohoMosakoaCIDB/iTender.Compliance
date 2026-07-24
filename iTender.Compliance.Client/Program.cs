using iTender.Compliance.Application.Common;
using iTender.Compliance.Application.Interfaces;
using iTender.Compliance.Client.Components;
using iTender.Compliance.Client.Components.Account;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Extensions;
using iTender.Compliance.Infrastructure.Hubs;
using iTender.Compliance.Infrastructure.Identity;
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
    options.AddPolicy("Administration", policy =>
        policy.RequireRole(Roles.Administrator));

    options.AddPolicy("CaseAssignment", policy =>
        policy.RequireRole(
            Roles.Administrator,
            Roles.Supervisor));

    options.AddPolicy("CaseManagement", policy =>
        policy.RequireRole(
            Roles.Administrator,
            Roles.Supervisor,
            Roles.ComplianceAgent));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseSqlServer(connectionString));
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

    await context.Database.MigrateAsync();

    await IdentitySeeder.SeedAsync(services);
}

app.MapHub<NotificationHub>("/hubs/notifications");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
