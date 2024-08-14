namespace Domain.Models.Base
{
    public abstract class CategoryBaseModel
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}
