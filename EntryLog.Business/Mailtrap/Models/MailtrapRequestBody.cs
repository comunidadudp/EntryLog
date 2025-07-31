using System.Text.Json.Serialization;

namespace EntryLog.Business.Mailtrap.Models
{
    internal class MailtrapRequestBody
    {
        [JsonPropertyName("from")]
        public From From { get; set; } = default!;

        [JsonPropertyName("to")]
        public List<To> To { get; set; } = [];

        [JsonPropertyName("template_uuid")]
        public string TemplateUuid { get; set; } = "";

        [JsonPropertyName("template_variables")]
        public object? TemplateVariables { get; set; }
    }

    public class From
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    public class To
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = "";
    }
}
