namespace Domain.Models.Base
{
    public abstract class OrderItemBaseModel
    {
        public int Quantity { get; set; }
        public double UnitaryPrice { get; set; }
        public double Subtotal { get; set; }
    }
}
