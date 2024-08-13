namespace Domain.Models.RequestModels
{
    public class OrderRequestModel
    {
        public DateTime OrderDate { get; set; }
        public double TotalValue { get; set; }
        public int ClientId { get; set; }
    }
}
