using System.Text.Json.Serialization;

namespace EntryLog.Business.DTOs
{
    internal class ImageBBResponseDTO
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    internal class Data
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url_viewer")]
        public string UrlViewer { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("display_url")]
        public string DisplayUrl { get; set; }

        [JsonPropertyName("width")]
        public string Width { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("expiration")]
        public string Expiration { get; set; }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

        [JsonPropertyName("thumb")]
        public Thumb Thumb { get; set; }

        [JsonPropertyName("medium")]
        public Medium Medium { get; set; }

        [JsonPropertyName("delete_url")]
        public string DeleteUrl { get; set; }
    }

    internal class Image
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mime")]
        public string Mime { get; set; }

        [JsonPropertyName("extension")]
        public string Extension { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    internal class Medium
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mime")]
        public string Mime { get; set; }

        [JsonPropertyName("extension")]
        public string Extension { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    internal class Thumb
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mime")]
        public string Mime { get; set; }

        [JsonPropertyName("extension")]
        public string Extension { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
