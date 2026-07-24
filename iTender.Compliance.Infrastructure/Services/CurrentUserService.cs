using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace iTender.Compliance.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }


        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                return Guid.TryParse(userId, out var id)
                    ? id
                    : null;
            }
        }

        public string? FullName
        {
            get
            {
                var user = _userManager
                    .GetUserAsync(_httpContextAccessor.HttpContext!.User)
                    .GetAwaiter()
                    .GetResult();

                return user?.FullName;
            }
        }
    }
}
