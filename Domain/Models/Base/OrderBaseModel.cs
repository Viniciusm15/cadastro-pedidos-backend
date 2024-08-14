namespace Domain.Models.Base
{
    public abstract class OrderBaseModel
    {
        public DateTime OrderDate { get; set; }
        public double TotalValue { get; set; }
        public int ClientId { get; set; }
    }
}
