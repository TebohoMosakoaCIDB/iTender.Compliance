using System.Text.Json.Serialization;

namespace iTender.Compliance.Application.DTOs
{
    public class AutoplaceSignatureRequest
    {
        [JsonPropertyName("search_text")]
        public string SearchText { get; set; } = "{Agent_Signature}";

        [JsonPropertyName("placement")]
        public string Placement { get; set; } = "TOP";

        [JsonPropertyName("order")]
        public int Order { get; set; } = 1;

        [JsonPropertyName("field_type")]
        public string FieldType { get; set; } = "SIGNATURE";

        [JsonPropertyName("level_of_assurance")]
        public List<string> LevelOfAssurance { get; set; }
            = new() { "HIGH_TRUST_ADVANCED" };

        [JsonPropertyName("multiline")]
        public bool Multiline { get; set; } = false;

        [JsonPropertyName("value")]
        public bool Value { get; set; } = false;

        [JsonPropertyName("max_length")]
        public int MaxLength { get; set; } = 100;

        [JsonPropertyName("validation_rule")]
        public string ValidationRule { get; set; } = "MANDATORY";

        [JsonPropertyName("placeholder")]
        public string Placeholder { get; set; } = "Compliance Agent";

        [JsonPropertyName("format")]
        public string Format { get; set; } = "yyyy-MM-dd";

        [JsonPropertyName("font")]
        public SignatureFont Font { get; set; } = new();

        [JsonPropertyName("dimensions")]
        public SignatureDimensions Dimensions { get; set; } = new();
    }

    public class SignatureFont
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "HELVETICA";

        [JsonPropertyName("size")]
        public int Size { get; set; } = 12;

        [JsonPropertyName("embedded_size")]
        public double EmbeddedSize { get; set; } = 7.5;
    }

    public class SignatureDimensions
    {
        [JsonPropertyName("width")]
        public int Width { get; set; } = 150;

        [JsonPropertyName("height")]
        public int Height { get; set; } = 50;
    }
}
