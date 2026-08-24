using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class CreatePackageRequest
    {
        [JsonPropertyName("package_name")]
        public string PackageName { get; set; } = string.Empty;
    }
}
