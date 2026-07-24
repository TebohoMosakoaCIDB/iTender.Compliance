namespace iTender.Compliance.Application.DTOs
{
    public class UserSearchModel
    {
        public string? SearchText { get; set; }

        public bool? IsActive { get; set; }

        public string? Role { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
