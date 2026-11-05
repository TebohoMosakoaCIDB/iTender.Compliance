# Configuration & Environment Specification

## 1. Document control

| Item | Value |
|---|---|
| System | iTender Compliance |
| Document | Configuration & Environment Specification |
| Version | 1.0 |
| Status | Source-code baseline; deployment values require environment-owner confirmation |
| Application target | .NET 10 (net10.0) |
| Primary configuration entry point | iTender.Compliance.Client/Program.cs |

This document describes configuration sections and runtime dependencies identified in the repository. It intentionally excludes credential values. It distinguishes settings confirmed in source from deployment practices that still need to be agreed and verified.

## 2. Scope and configuration model

The application is an ASP.NET Core Blazor Web App. The web host is created with WebApplication.CreateBuilder(args), and configuration is consumed through IConfiguration and strongly typed options in the Infrastructure project.

The application configures:
- EF Core with SQL Server using ConnectionStrings:DefaultConnection.
- ASP.NET Core Identity and role-based authorization.
- HTTP clients for the iTender API, eTender OCDS API, scraper service and SigningHub.
- Dataverse/CRM connection options and encryption options.
- SMTP email settings.
- Scraper and integration options.
- Logging and allowed-host configuration.

ASP.NET Core standard configuration providers are available through the host builder. The repository contains appsettings.json and appsettings.Development.json; it also declares a Visual Studio User Secrets identifier in the Client project file. The inspected startup code does not explicitly add a custom configuration provider or demonstrate production secret-store wiring. The deployment must therefore supply and verify its configuration sources.

## 3. Environment model

The repository explicitly contains a Development settings file. A separate Staging/UAT/Production settings file or deployment manifest was not established by the inspected source.

| Environment | Source status | Required configuration approach |
|---|---|---|
| Development | appsettings.Development.json exists; currently contains logging overrides | Use local non-production endpoints and developer secrets; do not commit credentials |
| Test / QA | No dedicated settings file confirmed | Provision isolated database, integration identities, endpoints and test mail/signing services |
| UAT | UAT endpoint values appear in the checked-in baseline configuration, but no dedicated UAT profile is confirmed | Move environment-specific values to deployment configuration; use UAT credentials and test data |
| Production | No dedicated production profile or deployment manifest confirmed | Use approved secret management, production endpoints, TLS, least-privilege identities, monitoring and tested recovery procedures |

The environment name is determined by the ASP.NET Core hosting environment (for example, ASPNETCORE_ENVIRONMENT). Confirm the actual value and configuration injection method in each hosting slot/service.

## 4. Configuration reference

The following table lists keys/sections consumed by the application or present in its checked-in baseline. Values are deliberately omitted.

### 4.1 Database

| Key | Purpose | Source / use |
|---|---|---|
| ConnectionStrings:DefaultConnection | SQL Server connection string for ComplianceDbContext | Read in Client Program.cs; passed to UseSqlServer |
| ConnectionStrings:AzureConnection | Present in baseline JSON | No use found in the inspected startup path; confirm whether obsolete or consumed elsewhere |

Important: the startup code throws an exception when DefaultConnection is absent, but the exception text refers to AzureConnection. Correct the message to match the actual key. The active EF Core provider in this startup path is SQL Server. The Client project also references Npgsql, but that package reference alone does not establish PostgreSQL as a configured runtime provider.

### 4.2 Dataverse / CRM

Section: Crm

| Key | Purpose |
|---|---|
| Crm:CrmURL | Dataverse/CRM organization URL |
| Crm:ClientId | Application/client identifier |
| Crm:ClientSecret | Client credential |

Bound to CrmOptions in iTender.Compliance.Infrastructure/CRM/CrmOptions.cs. The options registration uses AddOptions<CrmOptions>().Bind(...).ValidateOnStart(). Required members are declared for the three properties.

### 4.3 Encryption

Section: Encryption

| Key | Purpose |
|---|---|
| Encryption:Key | Encryption key material |
| Encryption:IV | Initialization vector material |

Bound to EncryptionOptions in iTender.Compliance.Infrastructure/CRM/EncryptionOptions.cs and validated on startup through the options registration. These values are secrets/cryptographic material and must not be stored in source control. Review the encryption design before changing or rotating existing key material because rotation may affect data that has already been encrypted.

### 4.4 Scraper service

Section: Scraper

| Key | Type / default in options class | Purpose |
|---|---|---|
| Scraper:BaseUrl | string; default is local HTTP endpoint | Base address of the scraper service |
| Scraper:TimeoutSeconds | integer; default 180 | HTTP timeout |
| Scraper:Enabled | Boolean; default true | Feature enablement flag |

Bound to ScraperOptions in iTender.Compliance.Infrastructure/Scrapers/ScraperOptions.cs. The typed HTTP client sets its base address and timeout from these options. Confirm whether Enabled is enforced by all scraper execution paths; the registration alone does not prove that the flag gates execution.

### 4.5 iTender API

Section: iTenderApi

| Key | Purpose |
|---|---|
| iTenderApi:BaseUrl | Base address for the IDataverseService HTTP client registered in Client startup |

The Client startup constructs a URI from this key. It currently uses a null-forgiving operator rather than explicit startup validation; ensure the key is present and is an absolute URI in every environment.

### 4.6 eTender integrations

| Section / key | Purpose |
|---|---|
| Etenders:BaseUrl | Base address for the eTender OCDS client |
| ETenderApi:BaseUrl | Base address for the separately modelled ETender API scraper |
| ETenderApi:Username | Basic-auth username for that scraper |
| ETenderApi:Password | Basic-auth password for that scraper |

EtendersOptions is bound in Infrastructure and used to configure an HTTP client with a 360-second timeout. ETenderApiOptions is bound in Infrastructure; the scraper implementation adds an HTTP Basic Authorization header when both username and password are supplied. The inspected registration shows the options binding, while the scraper service registration itself is commented out. Confirm whether this scraper is currently active, scheduled, or retired before treating the settings as operationally required.

### 4.7 SigningHub

Section: SigningHub

| Key | Purpose |
|---|---|
| SigningHub:BaseUrl | SigningHub API base address |
| SigningHub:ClientId | Client/application identifier |
| SigningHub:ClientSecret | Client secret |
| SigningHub:Username | Integration username, if required by configured authentication flow |
| SigningHub:Password | Integration password, if required |
| SigningHub:Scope | Authentication scope |
| SigningHub:AuthenticationEndpoint | Authentication endpoint |
| SigningHub:CallbackUrl | Callback URL present in baseline configuration |

The options class is iTender.Compliance.Infrastructure/Models/SigningHubOptions.cs. Infrastructure binds SigningHubOptions and configures an HTTP client with the configured base URL and JSON Accept header. Verify callback URL handling and the exact authentication flow against the deployed SigningHub configuration.

### 4.8 SMTP

Section: Smtp

| Key | Purpose |
|---|---|
| Smtp:Host | SMTP server hostname |
| Smtp:Port | SMTP port; options default is 587 |
| Smtp:EnableSsl | Whether SSL/TLS is enabled; options default is true |
| Smtp:Username | SMTP account |
| Smtp:Password | SMTP credential |
| Smtp:FromAddress | Sender email address |
| Smtp:FromName | Sender display name; options default is iTender Compliance |

Bound to SmtpOptions and consumed by the SMTP email service. Confirm the approved relay, authentication mode, sender authorization and outbound network rules per environment.

### 4.9 Documents and synchronization

| Key | Purpose |
|---|---|
| Documents:RootFolder | Document storage root configured in baseline JSON |
| Documents:LogoPath | CIDB logo path configured in baseline JSON |
| Synchronization:SystemUserId | System user identifier configured for synchronization |

Confirm whether document paths are resolved relative to the application content root, whether the target storage is persistent and backed up, and whether the synchronization identity is valid in each environment. Do not assume local web-root storage is suitable for production documents.

### 4.10 Other configuration

| Key / section | Purpose / status |
|---|---|
| Logging:LogLevel:Default | Default logging threshold; baseline is Information |
| Logging:LogLevel:Microsoft.AspNetCore | ASP.NET Core logging threshold; baseline is Warning |
| AllowedHosts | Host filtering configuration; baseline is wildcard |
| OpenAI | Options binding is registered; exact option properties and required values must be confirmed from the OpenAI options implementation and enabled feature path |
| SystemSetting database record | Runtime business settings such as deadlines, reminder behaviour, sync interval, assignment, report schedules and pagination; maintained through the application settings service rather than solely through appsettings |

## 5. Runtime configuration and startup behaviour

### 5.1 Startup sequence

The Client startup:
1. Creates the web application builder and registers Razor Components with Interactive Server rendering.
2. Calls Infrastructure registration, including options, typed HTTP clients, repositories, application services and hosted services.
3. Configures Identity cookies, authorization policies and the EF Core context.
4. Builds the application and configures environment-dependent exception handling.
5. Maps static assets, Razor Components and the notification SignalR hub.
6. Creates a service scope and runs the Identity, Agent, correspondence-template, public-holiday and system-setting seeders.
7. Maps additional Identity endpoints and starts the host.

Database connectivity and seed operations are therefore startup-critical. A database outage, invalid seed data, or failed seeding can prevent successful application startup. Deployment readiness checks should include database access and seeding behaviour.

### 5.2 Environment-dependent middleware

- In Development, the application uses the EF Core migrations endpoint.
- Outside Development, it configures the /Error exception handler and HSTS.
- HTTPS redirection and antiforgery middleware are enabled.
- Status-code handling re-executes through /not-found.

Confirm TLS termination and forwarded-header handling if hosted behind a reverse proxy or load balancer. The inspected startup does not show forwarded-header configuration.

### 5.3 Background services

Infrastructure registers the following hosted services:
- TenderSyncBackgroundService
- AutoAssignmentBackgroundService
- RoPComplianceBackgroundService
- ComplianceWorkflowBackgroundService
- SigningApprovalBackgroundService
- MonthlyReportBackgroundService

These services run in the web host process. Their effective intervals and dependencies may also be influenced by database-backed System Settings. For multiple application instances, confirm distributed coordination, duplicate-execution protection, idempotency and recovery behaviour before scaling out.

## 6. Secret management requirements

### 6.1 Current repository finding — critical

The inspected checked-in iTender.Compliance.Client/appsettings.json contains non-placeholder values in credential-bearing settings. The affected areas include database connection credentials, CRM client credentials, encryption material, eTender API credentials, SigningHub credentials and SMTP credentials.

The IdentitySeeder source also contains hard-coded bootstrap and seeded-user passwords. Treat these values as potentially compromised, regardless of whether the repository is private. Do not copy the values into tickets, documentation, deployment variables, logs or chat messages.

Required response:
1. Immediately rotate/revoke affected database, CRM, eTender, SigningHub and SMTP credentials with the relevant service owners.
2. Assess whether the encryption key/IV have protected persisted data. Plan a controlled key migration/rotation; do not simply replace key material without assessing decryptability.
3. Remove secret values from tracked configuration and replace them with non-sensitive placeholders or omit them.
4. Review repository history, forks, clones, build artifacts and logs. Coordinate a Git-history purge where required, while recognizing that history rewriting does not replace credential rotation.
5. Review access/audit logs for unexpected use of the exposed credentials.
6. Remove hard-coded seed passwords. Provision bootstrap credentials through a controlled, one-time process, force password change, and ensure seeded accounts are not silently recreated with known credentials.
7. Add secret scanning and push protection where available, and prevent future commits containing secrets.

### 6.2 Required target approach

- Use a managed secret store for deployed environments (for example, Azure Key Vault if Azure hosting is selected), with workload identity/managed identity where supported.
- Use .NET User Secrets only for local development; never commit the secrets store.
- Use environment variables or deployment-platform secret references for settings injected at runtime.
- Restrict secret access by application identity and environment; separate development, test, UAT and production credentials.
- Rotate credentials on a defined schedule and immediately after suspected exposure or personnel/vendor changes.
- Never log connection strings, access tokens, passwords, client secrets, encryption keys or authorization headers.

### 6.3 Environment variable naming

ASP.NET Core environment variables use double underscores to represent configuration hierarchy. Examples of names (values intentionally omitted):

| Configuration key | Environment variable |
|---|---|
| ConnectionStrings:DefaultConnection | ConnectionStrings__DefaultConnection |
| Crm:ClientSecret | Crm__ClientSecret |
| Encryption:Key | Encryption__Key |
| SigningHub:ClientSecret | SigningHub__ClientSecret |
| Smtp:Password | Smtp__Password |
| ETenderApi:Password | ETenderApi__Password |

Use the deployment platform's protected secret-reference mechanism rather than storing literal values in deployment manifests committed to source control.

## 7. Configuration validation and operational controls

The current code validates CRM and Encryption options on startup. Other sections are configured/bound without equivalent explicit validation in the inspected registration.

Recommended improvements:
- Add startup validation for required URLs, credentials, SMTP port, timeout ranges and environment-specific required sections.
- Validate absolute URI values and reject insecure HTTP endpoints in production, except explicitly approved internal service paths protected by the network/security architecture.
- Fail startup with a clear, non-sensitive message when required configuration is missing.
- Avoid null-forgiving configuration access for required values.
- Keep non-secret defaults in source control and environment-specific secret values outside it.
- Use a documented configuration change/release process; record who approved and changed operational settings.
- Distinguish deployment configuration (credentials, endpoints, storage paths) from business configuration stored in the System Settings table.

## 8. Deployment configuration checklist

### Before deployment
- [ ] Confirm target environment name and hosting platform.
- [ ] Confirm the SQL Server server/database, network access, TLS requirements and least-privilege database identity.
- [ ] Provision all required integration endpoints and environment-specific service identities.
- [ ] Rotate all credentials identified in the checked-in configuration and seeded account code.
- [ ] Configure secret-store access and verify the application identity can retrieve required secrets.
- [ ] Configure persistent document storage and backup policy.
- [ ] Confirm scraper service deployment, base URL, timeout and enablement.
- [ ] Confirm eTender, Dataverse, SigningHub and SMTP connectivity.
- [ ] Confirm production allowed-host values and HTTPS/TLS termination.
- [ ] Confirm database migration ownership and rollback plan.
- [ ] Confirm scheduled/background service behaviour for the intended instance count.
- [ ] Confirm logging destination, retention, alerting and sensitive-data redaction.

### After deployment
- [ ] Verify application startup and database connectivity.
- [ ] Verify Identity sign-in, email confirmation and role assignments.
- [ ] Verify SignalR notification connection.
- [ ] Verify each enabled integration using a non-destructive health/test operation.
- [ ] Verify background services execute once as intended and report failures.
- [ ] Verify document generation, storage and retrieval.
- [ ] Verify email and SigningHub workflows in the target environment.
- [ ] Verify logs and monitoring contain no secrets or personal data beyond approved requirements.
- [ ] Record deployed configuration version and change approval.

## 9. Source files reviewed

- iTender.Compliance.Client/Program.cs
- iTender.Compliance.Client/appsettings.json
- iTender.Compliance.Client/appsettings.Development.json
- iTender.Compliance.Client/iTender.Compliance.Client.csproj
- iTender.Compliance.Infrastructure/Extensions/ServiceCollectionExtensions.cs
- iTender.Compliance.Infrastructure/Data/ComplianceDbContext.cs
- iTender.Compliance.Infrastructure/CRM/CrmOptions.cs
- iTender.Compliance.Infrastructure/CRM/EncryptionOptions.cs
- iTender.Compliance.Infrastructure/Scrapers/ScraperOptions.cs
- iTender.Compliance.Infrastructure/Models/SigningHubOptions.cs
- iTender.Compliance.Infrastructure/Models/SmtpOptions.cs
- iTender.Compliance.Infrastructure/Models/EtendersOptions.cs
- iTender.Compliance.Infrastructure/Scrapers/ETenderApiScraper.cs
- iTender.Compliance.Infrastructure/Identity/IdentitySeeder.cs
- iTender.Compliance.Application/Common/Roles.cs
- iTender.Compliance.Infrastructure/Services/SystemSettingService.cs

## 10. Items requiring environment-owner confirmation

1. Actual hosting platform, deployment slots, environment names and configuration injection mechanism.
2. Whether Azure Key Vault or another managed secret store is already configured outside the repository.
3. Which database connection key is authoritative in each environment; only DefaultConnection is used by the inspected startup path.
4. Whether the ETender API scraper is active, since its service registration appears commented out.
5. Whether Documents:RootFolder points to persistent production storage and how access/backup/retention are enforced.
6. Whether the OpenAI integration is enabled and its complete configuration requirements.
7. Whether the application is deployed as a single instance or multiple instances and how hosted services are coordinated.
8. Whether production TLS is terminated at the application or an upstream proxy.
9. The approved database migration, backup, restore and rollback procedures.
10. Whether seeded accounts and passwords in IdentitySeeder are still present in deployed databases and whether their credentials have been changed.

---

**Document note:** This specification describes the configuration observed in the repository at the time of review. It is not proof that the deployed environment uses the same values or infrastructure. Environment owners must validate the deployment-specific items above before this document is treated as the production configuration baseline.
