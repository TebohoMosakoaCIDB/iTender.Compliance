namespace iTender.Compliance.Application.DTOs
{
    public class OpenAIOptions
    {
        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-5.6";
    }
}
