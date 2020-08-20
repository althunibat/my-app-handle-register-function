using System.Text.Json.Serialization;

namespace Godwit.HandleRegistrationAction.Model {
    public class Action {
        [JsonPropertyName("name")] public string Name { get; set; }
    }
}