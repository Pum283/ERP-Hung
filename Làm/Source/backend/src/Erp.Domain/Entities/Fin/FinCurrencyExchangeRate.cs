using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Danh mục đồng tiền hạch toán & tỷ giá quy đổi ngoại tệ (UC_FIN_005).</summary>
public class FinCurrencyExchangeRate : TenantEntity
{
    public string CurrencyCode { get; set; } = "USD";
    public string CurrencyName { get; set; } = "Đô La Mỹ";
    public decimal ExchangeRateToVnd { get; set; } = 25450;
    public string RateSource { get; set; } = "Vietcombank";
    public bool IsBaseCurrency { get; set; } = false;
    public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.UtcNow;
}
