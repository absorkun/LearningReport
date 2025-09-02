using FluentValidation;
using LearningReport.Data;
using LearningReport.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LearningReport.Users.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator(AppDbContext db)
        {
            RuleFor(u => u.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(async (email, cancellation) =>
                    !await db.Users.AnyAsync(u => u.Email == email, cancellation))
                .WithMessage("User with that email address is not null");

            RuleFor(u => u.Password)
                .NotEmpty()
                .MinimumLength(5);

            RuleFor(u => u.Role)
                .NotEmpty()
                .Must(r => r == "Admin" || r == "User")
                .WithMessage("Choose: Admin or User");
        }
    }
}
