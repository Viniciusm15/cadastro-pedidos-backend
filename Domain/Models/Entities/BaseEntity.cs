using FluentValidation;
using FluentValidation.Results;

namespace Domain.Models.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        protected BaseEntity()
        {
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public ValidationResult Validate<T>(IValidator<T> validator) where T : BaseEntity
        {
            return validator.Validate((T)this);
        }
    }
}
