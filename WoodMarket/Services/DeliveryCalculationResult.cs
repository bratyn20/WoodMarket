namespace WoodMarket.Services
{
    public class DeliveryCalculationResult
    {
        public string TariffName { get; set; }
        public decimal Cost { get; set; }
        public int? MinDays { get; set; }
        public int? MaxDays { get; set; }
        public string Currency { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
    }
}
