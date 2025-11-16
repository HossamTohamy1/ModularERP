namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class POStatusInfo
    {
        public string ReceptionStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string DocumentStatus { get; set; } = string.Empty;
        public string? PreviousReceptionStatus { get; set; }
    }
}
