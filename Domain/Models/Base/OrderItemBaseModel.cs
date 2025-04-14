namespace Domain.Models.Base
{
    public abstract class OrderItemBaseModel
    {
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public double UnitaryPrice { get; set; }
        public double Subtotal { get; set; }
    }
}
