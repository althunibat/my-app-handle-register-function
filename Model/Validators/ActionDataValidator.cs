using FluentValidation;

namespace Godwit.HandleRegistrationAction.Model.Validators {
    public class ActionDataValidator : AbstractValidator<ActionData> {
        public ActionDataValidator() {
            RuleFor(x => x.Action).NotNull();
            RuleFor(x => x.Action.Name).Must(x => x.Equals("register"));
            RuleFor(x => x.Input).NotNull();
            RuleFor(x => x.Input.UserName).NotEmpty();
            RuleFor(x => x.Input.Email).NotEmpty();
            RuleFor(x => x.Input.Email).EmailAddress();
            RuleFor(x => x.Input.Password).NotEmpty();
            RuleFor(x => x.Input.ConfirmPassword).NotEmpty();
            RuleFor(x => x.Input.ConfirmPassword).Must((data, cp) => cp == data.Input.Password);
            RuleFor(x => x.Input.LastName).NotEmpty();
            RuleFor(x => x.Input.FirstName).NotEmpty();
            RuleFor(x => x.Input.MobileNumber).NotEmpty();
        }
    }
}