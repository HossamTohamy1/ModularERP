namespace ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN
{
    public class GRNNextActionsDto
    {
        public bool CanReceiveMore { get; set; }
        public bool CanCreateReturn { get; set; }
        public bool CanCreateInvoice { get; set; }
        public bool CanClose { get; set; }
        public decimal TotalRemainingToReceive { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
