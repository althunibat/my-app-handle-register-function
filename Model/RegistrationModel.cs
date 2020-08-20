using System;
using System.Text.Json.Serialization;
using Godwit.Common.Data.Model;

namespace Godwit.HandleRegistrationAction.Model {
    public class RegistrationModel {
        [JsonPropertyName("userName")] public string UserName { get; set; }

        [JsonPropertyName("email")] public string Email { get; set; }

        [JsonPropertyName("firstName")] public string FirstName { get; set; }

        [JsonPropertyName("lastName")] public string LastName { get; set; }

        [JsonPropertyName("password")] public string Password { get; set; }

        [JsonPropertyName("confirmPassword")] public string ConfirmPassword { get; set; }

        [JsonPropertyName("mobileNumber")] public string MobileNumber { get; set; }

        [JsonPropertyName("gender")] public Gender Gender { get; set; }

        [JsonPropertyName("birthDate")] public DateTime BirthDate { get; set; }
    }
}