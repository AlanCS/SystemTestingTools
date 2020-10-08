using System.Text.Json.Serialization;

namespace MovieProject.Logic.Proxy.DTO
{
    public class SavedPost : Post
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

}
