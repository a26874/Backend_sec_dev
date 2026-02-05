/*
*	<copyright file="UserValidator">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2026 2/5/2026 9:56:35 PM</date>
*	<description></description>
**/
using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Shared.Constants;
using FluentValidation;

namespace Backend_sec_dev.Domain.Validators
{
    public class UserCreationValidator : AbstractValidator<UserCredentialsDto>
    {
        public UserCreationValidator()
        {
            RuleFor(u => u.email).NotEmpty().Matches("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$");
            RuleFor(u => u.password).NotEmpty().MinimumLength(8).MaximumLength(16);
            RuleFor(u => u.role).NotEqual(RolesConstants.Admin);
        }
    }
}