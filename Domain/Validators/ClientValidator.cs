﻿using Domain.Models.Entities;
using FluentValidation;

namespace Domain.Validators
{
    public class ClientValidator : AbstractValidator<Client>
    {
        public ClientValidator()
        {
            RuleFor(client => client.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("The field {PropertyName} must be a maximum of {MaxLength} characters.");

            RuleFor(client => client.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(256).WithMessage("The field {PropertyName} must be a maximum of {MaxLength} characters.");

            RuleFor(client => client.Telephone)
                .NotEmpty().WithMessage("Telephone is required")
                .MaximumLength(20).WithMessage("The field {PropertyName} must be a maximum of {MaxLength} characters.")
                .Matches(@"^\+\d{1,3}\s?\(\d{2}\)\s?\d{4,5}\-\d{4}$").WithMessage("Invalid phone number. Accepted formats: +55 (47) 99141-0923, (47) 99141-0923, 47991410923.");

            RuleFor(client => client.BirthDate)
                .LessThan(DateTime.Today).WithMessage("BirthDate must be in the past");
        }
    }
}