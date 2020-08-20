using System.Text.Json.Serialization;

namespace Godwit.HandleRegistrationAction.Model
{
    public class Session
    {
        [JsonPropertyName("x-hasura-user-id")]
        public string UserId { get; set; }

        [JsonPropertyName("x-hasura-role")]
        public string Role { get; set; }
    }
}