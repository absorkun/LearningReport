using FluentValidation;
using LearningReport.Data;
using LearningReport.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LearningReport.Users.Validators
{
    public class LoginUserValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserValidator(AppDbContext db)
        {
            RuleFor(u => u.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(u => u.Password)
                .NotEmpty()
                .MinimumLength(5);
        }
    }
}
