namespace ModularERP.Common.Enum.Inventory_Enum
{
    public enum PriceRuleType
    {
        Markup,              // Add percentage/amount to base price
        Margin,              // Calculate based on profit margin
        FixedAdjustment,     // Fixed amount adjustment
        CurrencyConversion,  // Convert from one currency to another
        Promotion            // Promotional pricing
    }
}
