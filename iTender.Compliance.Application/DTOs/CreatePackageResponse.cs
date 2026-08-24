using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class CreatePackageResponse
    {
        [JsonPropertyName("package_id")]
        public int PackageId { get; set; }
    }
}
