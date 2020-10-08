using System.Text.Json.Serialization;

namespace MovieProject.Logic.Proxy.DTO
{
    public class Post
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("userId")]
        public long UserId { get; set; }
    }

}
