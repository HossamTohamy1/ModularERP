namespace ModularERP.Common.Enum.Finance_Enum
{
    public enum FinanceErrorCode
    {
        // General Errors
        ValidationError = 1001,
        NotFound = 1002,
        UnauthorizedAccess = 1003,
        InternalServerError = 1004,
        BusinessLogicError= 1005,
        TreasuryAlreadyExists= 1006,
        TreasuryHasVouchers = 1007,
        // Finance Module Errors
        VoucherNotFound = 2001,
        VoucherAlreadyPosted = 2002,
        VoucherCannotBeEdited = 2003,
        InsufficientBalance = 2004,
        InvalidAmount = 2005,
        InvalidDate = 2006,
        WalletNotFound = 2007,
        WalletInactive = 2008,
        NoPermissionForWallet = 2009,
        CategoryAccountInvalid = 2010,
        JournalAccountInvalid = 2011,
        TaxCalculationError = 2012,
        CurrencyNotSupported = 2013,
        FxRateInvalid = 2014,

        // Treasury Errors
        TreasuryNotFound = 2101,
        TreasuryInactive = 2102,
        TreasuryCodeDuplicate = 2103,

        // Bank Account Errors
        BankAccountNotFound = 2201,
        BankAccountInactive = 2202,
        BankAccountNumberDuplicate = 2203,

        // GL Account Errors
        GlAccountNotFound = 2301,
        GlAccountNotLeaf = 2302,
        GlAccountWrongType = 2303,
        GlAccountCodeDuplicate = 2304,

        // Company Errors
        CompanyNotFound = 2401,
        CompanyInactive = 2402,

        // User & Permission Errors
        UserNotFound = 2501,
        UserInactive = 2502,
        AccessDenied = 2503
    }
}
