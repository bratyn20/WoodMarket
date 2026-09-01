using System.Text.Json.Serialization;

namespace WoodMarket.Services
{
    public class CdekCity
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("city_uuid")]
        public string CityUuid { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("region_code")]
        public int? RegionCode { get; set; }  // ✅ Может быть null

        [JsonPropertyName("sub_region")]
        public string SubRegion { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }  // ✅ Может быть null

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }  // ✅ Может быть null

        [JsonPropertyName("time_zone")]
        public string TimeZone { get; set; }

        [JsonPropertyName("payment_limit")]
        public decimal? PaymentLimit { get; set; }  // ✅ Может быть null или -1.0
    }
}
