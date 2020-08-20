using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Godwit.Common.Data.Model;
using Godwit.HandleRegistrationAction.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Godwit.HandleRegistrationAction.Controllers {
    [ApiController]
    [Route("")]
    public class HasuraController : ControllerBase {
        private readonly ILogger _logger;
        private readonly UserManager<User> _manager;
        private readonly IValidator<ActionData> _validator;

        public HasuraController(IValidator<ActionData> validator,
            ILogger<HasuraController> logger, UserManager<User> manager) {
            _validator = validator;
            _logger = logger;
            _manager = manager;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ActionData model) {
            _logger.LogInformation($"Call Started by {model.Session.UserId} having role {model.Session.Role}");
            var validation = _validator.Validate(model);
            if (!validation.IsValid)
            {
                _logger.LogWarning("request validation failed!");
                return Ok(new
                {
                    Errors = validation.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }

            var user = new User(model.Input.UserName) {
                BirthDate = LocalDate.FromDateTime(model.Input.BirthDate),
                CreatedOn = Instant.FromDateTimeOffset(DateTimeOffset.Now),
                Email = model.Input.Email,
                FirstName = model.Input.FirstName,
                LastName = model.Input.LastName,
                Gender = model.Input.Gender,
                EmailConfirmed = false,
                PhoneNumber = model.Input.MobileNumber
            };
            try {
                var result = await _manager.CreateAsync(user, model.Input.Password).ConfigureAwait(false);
                if (result != IdentityResult.Success)
                {
                    _logger.LogWarning(result.Errors.First().Description);
                    return Ok(new
                    {
                        Errors = result.Errors.Select(e => e.Description).ToArray()
                    });
                }

                return Ok(new {user.Id});
            }
            catch (Exception e) {
                _logger.LogError(new EventId(1001, "Exception"), e, "Unable to Save Data!");
                return Ok(new {
                    Errors = new[] {"Unable to Save Data!, An Exception Occur!", e.Message}
                });
            }
        }
    }
}