using FluentValidation;
using LearningReport.Users.Dtos;

namespace LearningReport.Users.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(u => u.Email)
                .EmailAddress().When(u => u.Email is not null);

            RuleFor(u => u.Password)
                .MinimumLength(5).When(u => u.Password is not null);

            RuleFor(u => u.Role)
                .Must(r => r == "Admin" || r == "User").When(u => u.Role is not null);
        }
    }
}
