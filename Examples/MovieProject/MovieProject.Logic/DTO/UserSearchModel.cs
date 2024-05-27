namespace MovieProject.Logic.DTO
{
    using System.Text.Json.Serialization;

    namespace MovieProject.Logic.Proxy.DTO
    {

        public class UserSearchModel
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("username")]
            public string Username { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("phone")]
            public string Phone { get; set; }

            [JsonPropertyName("website")]
            public string Website { get; set; }
        }

    }
}