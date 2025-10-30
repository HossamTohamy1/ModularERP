﻿using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Purchases.Purchase_Order_Management.DTO.DTO_PODeposite;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Commends.Commends_PODeposite
{
    public class CreatePODepositCommand : IRequest<ResponseViewModel<PODepositResponseDto>>
    {
        public Guid PurchaseOrderId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public bool AlreadyPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Notes { get; set; }
    }
}
