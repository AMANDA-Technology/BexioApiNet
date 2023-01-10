namespace BexioApiNet.Abstractions.Models.Banking.BankAccounts.Views;

/// <summary>
/// Bexio bank account get view. <see href="https://docs.bexio.com/#tag/Bank-Accounts/operation/ShowBankAccount"/>
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Owner"></param>
/// <param name="OwnerAddress"></param>
/// <param name="OwnerZip"></param>
/// <param name="OwnerCity"></param>
/// <param name="BcNr"></param>
/// <param name="BankName"></param>
/// <param name="BankNr"></param>
/// <param name="BankAccountNr"></param>
/// <param name="IbanNr"></param>
/// <param name="CurrencyId"></param>
/// <param name="AccountId"></param>
/// <param name="Remarks"></param>
/// <param name="InvoiceMode"></param>
/// <param name="QrInvoiceIban"></param>
/// <param name="Type"></param>
public sealed record BankAccountGet(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("owner_address")] string OwnerAddress,
    [property: JsonPropertyName("owner_zip")] string OwnerZip,
    [property: JsonPropertyName("owner_city")] string OwnerCity,
    [property: JsonPropertyName("bc_nr")] string BcNr,
    [property: JsonPropertyName("bank_name")] string BankName,
    [property: JsonPropertyName("bank_nr")] string BankNr,
    [property: JsonPropertyName("bank_account_nr")] string BankAccountNr,
    [property: JsonPropertyName("iban_nr")] string IbanNr,
    [property: JsonPropertyName("currency_id")] int? CurrencyId,
    [property: JsonPropertyName("account_id")] int? AccountId,
    [property: JsonPropertyName("remarks")] string Remarks,
    [property: JsonPropertyName("invoice_mode")] string InvoiceMode,
    [property: JsonPropertyName("qr_invoice_iban")] string QrInvoiceIban,
    [property: JsonPropertyName("type")] string Type
);
