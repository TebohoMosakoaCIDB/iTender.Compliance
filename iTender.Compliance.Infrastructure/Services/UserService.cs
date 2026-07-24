using DocumentFormat.OpenXml.Bibliography;
using iTender.Compliance.Application.Common;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Repositories;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Domain.Entities;
using iTender.Compliance.Domain.Enums;
using iTender.Compliance.Infrastructure.Data;
using iTender.Compliance.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace iTender.Compliance.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAgentRepository _agentRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IAgentRepository agentRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _agentRepository = agentRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult> RegisterAsync(
        RegisterUserModel model,
        CancellationToken cancellationToken = default)
    {
        var exists = await _userManager.FindByEmailAsync(model.Email);

        if (exists != null)
        {
            return ServiceResult.Failed("A user with this email already exists.");
        }

        if (model.Password != model.ConfirmPassword)
        {
            return ServiceResult.Failed("Passwords do not match.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return ServiceResult.Failed(
                result.Errors.Select(x => x.Description));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

        if (!roleResult.Succeeded)
        {
            return ServiceResult.Failed(
                roleResult.Errors.Select(x => x.Description));
        }

        if (model.Role == Roles.ComplianceAgent)
        {
            var agent = new Agent
            {
                UserId = user.Id,
                FullName = user.FullName,
                EmployeeNumber = model.EmployeeNumber,
                Department = model.Department ?? "Compliance",
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive,
                Level = model.Level,

                JobTitle = model.JobTitle,
                HeaderImagePath = model.HeaderImagePath,
                SignatureImagePath = model.SignatureImagePath,
                FooterText = model.FooterText
            };

            await _agentRepository.AddAsync(agent, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<PagedResult<UserListModel>> SearchAsync(
    UserSearchModel search,
    CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.SearchText))
        {
            var text = search.SearchText.Trim();

            query = query.Where(x =>
                x.FirstName.Contains(text) ||
                x.LastName.Contains(text) ||
                x.Email!.Contains(text));
        }

        if (search.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == search.IsActive.Value);
        }

        var users = await query
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync(cancellationToken);

        var result = new List<UserListModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (!string.IsNullOrWhiteSpace(search.Role) &&
                !roles.Contains(search.Role))
            {
                continue;
            }

            Agent? agent = null;

            if (roles.Contains(Roles.ComplianceAgent))
            {
                agent = await _agentRepository.GetByUserIdAsync(
                    user.Id,
                    cancellationToken);
            }

            result.Add(new UserListModel
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? "No Role",
                Level = agent?.Level.ToString() ?? "N/A"
            });
        }

        var totalCount = result.Count;

        var page = result
            .Skip((search.PageNumber - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToList();

        return new PagedResult<UserListModel>
        {
            Items = page,
            PageNumber = search.PageNumber,
            PageSize = search.PageSize,
            TotalCount = totalCount
        };
    }
    public async Task<List<UserListModel>> GetUsersWithoutAgentsAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync(cancellationToken);

        var agents = await _agentRepository.GetAllAsync(cancellationToken);

        return await BuildUserList(users, agents, true);
    }

    public async Task<UpdateUserModel?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        Agent? agent = null;

        if (roles.Contains(Roles.ComplianceAgent))
        {
            agent = await _agentRepository.GetByUserIdAsync(
                user.Id,
                cancellationToken);
        }

        return new UpdateUserModel
        {
            Id = user.Id,

            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,

            Role = roles.FirstOrDefault() ?? string.Empty,

            IsActive = user.IsActive,

            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnabled = user.LockoutEnabled,

            AccessFailedCount = user.AccessFailedCount,
            LockoutEnd = user.LockoutEnd,
            CreatedOn = user.CreatedOn,
            LastLoginOn = user.LastLoginOn,
            Department = agent?.Department,
            EmployeeNumber = agent?.EmployeeNumber,
            PhoneNumber = agent?.PhoneNumber,
            Level = agent?.Level ?? AgentLevel.Junior,
            JobTitle = agent?.JobTitle,
            HeaderImagePath = agent?.HeaderImagePath,
            SignatureImagePath = agent?.SignatureImagePath,
            FooterText = agent?.FooterText
        };
    }

    private async Task<List<UserListModel>> BuildUserList(
        List<ApplicationUser> users,
        List<Domain.Entities.Agent> agents,
        bool onlyWithoutAgents = false)
    {
        var result = new List<UserListModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var hasAgent = agents.Any(x => x.UserId == user.Id);

            if (onlyWithoutAgents && hasAgent)
                continue;

            result.Add(new UserListModel
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty,
                HasAgent = hasAgent
            });
        }

        return result;
    }

    public async Task<ServiceResult> UpdateAsync(
    UpdateUserModel model,
    CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(model.Id.ToString());

        if (user == null)
        {
            return ServiceResult.Failed("User not found.");
        }

        // Email already used?
        var existingUser = await _userManager.FindByEmailAsync(model.Email);

        if (existingUser != null && existingUser.Id != user.Id)
        {
            return ServiceResult.Failed("Another user already uses this email address.");
        }

        // Update Identity fields
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.LastLoginOn = model.LastLoginOn;
        user.CreatedOn = model.CreatedOn;
        user.PhoneNumber = model.PhoneNumber;

        user.IsActive = model.IsActive;

        user.PhoneNumber = model.PhoneNumber;
        user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;

        user.EmailConfirmed = model.EmailConfirmed;

        user.TwoFactorEnabled = model.TwoFactorEnabled;

        user.LockoutEnabled = model.LockoutEnabled;

        if (!model.LockoutEnabled)
        {
            user.LockoutEnd = null;
        }
        else
        {
            user.LockoutEnd = model.LockoutEnd;
        }

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return ServiceResult.Failed(
                updateResult.Errors.Select(x => x.Description).ToList());
        }

        // Update role if necessary
        var currentRoles = await _userManager.GetRolesAsync(user);

        if (!currentRoles.Contains(model.Role))
        {
            if (currentRoles.Any())
            {
                var removeResult =
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    return ServiceResult.Failed(
                        removeResult.Errors.Select(x => x.Description).ToList());
                }
            }

            var addResult =
                await _userManager.AddToRoleAsync(user, model.Role);

            if (!addResult.Succeeded)
            {
                return ServiceResult.Failed(
                    addResult.Errors.Select(x => x.Description).ToList());
            }
        }

        // Update Agent information (if applicable)
        var agent = await _agentRepository.GetByUserIdAsync(
            user.Id,
            cancellationToken);

        if (agent != null)
        {
            agent.EmployeeNumber = model.EmployeeNumber ?? string.Empty;
            agent.PhoneNumber = model.PhoneNumber ?? string.Empty;
            agent.IsActive = model.IsActive;
            agent.Department = model.Department ?? string.Empty;
            agent.Level = model.Level;

            // Correspondence Branding
            agent.JobTitle = model.JobTitle;
            agent.HeaderImagePath = model.HeaderImagePath;
            agent.SignatureImagePath = model.SignatureImagePath;
            agent.FooterText = model.FooterText;

            await _agentRepository.UpdateAsync(agent, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var changes = new List<string>();

        if (user.Email != model.Email)
            changes.Add($"Email changed to {model.Email}");

        if (!currentRoles.Contains(model.Role))
            changes.Add($"Role changed to {model.Role}");

        changes.Add($"Active = {model.IsActive}");

        await _auditService.LogAsync(
            AuditAction.Updated,
            AuditEntity.User,
            user.Id,
            $"User updated. {string.Join(", ", changes)}.",
            _currentUser.UserId,
            cancellationToken);

        return ServiceResult.Success();
    }
}