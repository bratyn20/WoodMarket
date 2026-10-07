using System.Text.Json.Serialization;

namespace WoodMarket.Models.Cdek
{
    public class CdekOffice
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }              // Код ПВЗ (например, "MSK39")

        [JsonPropertyName("name")]
        public string Name { get; set; }              // Название офиса

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }              // UUID офиса

        [JsonPropertyName("address_comment")]
        public string AddressComment { get; set; }    // Комментарий к адресу

        [JsonPropertyName("nearest_station")]
        public string NearestStation { get; set; }    // Ближайшая станция метро

        [JsonPropertyName("work_time")]
        public string WorkTime { get; set; }          // Режим работы

        [JsonPropertyName("emails")]
        public List<CdekEmail> Emails { get; set; }   // Email-адреса

        [JsonPropertyName("phones")]
        public List<CdekPhone> Phones { get; set; }   // Телефоны

        [JsonPropertyName("location")]
        public CdekLocation Location { get; set; }    // Адрес и координаты

        [JsonPropertyName("type")]
        public string Type { get; set; }              // Тип офиса (PVZ, POSTAMAT)

        [JsonPropertyName("owner_code")]
        public string OwnerCode { get; set; }

        [JsonPropertyName("take_only")]
        public bool TakeOnly { get; set; }            // Только приём

        [JsonPropertyName("is_handout")]
        public bool IsHandout { get; set; }           // Пункт выдачи

        [JsonPropertyName("is_reception")]
        public bool IsReception { get; set; }         // Пункт приёма

        [JsonPropertyName("is_dressing_room")]
        public bool IsDressingRoom { get; set; }      // Есть примерочная

        [JsonPropertyName("have_cashless")]
        public bool HaveCashless { get; set; }        // Безналичная оплата

        [JsonPropertyName("have_cash")]
        public bool HaveCash { get; set; }            // Наличная оплата

        [JsonPropertyName("allowed_cod")]
        public bool AllowedCod { get; set; }          // Разрешён наложенный платёж

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("office_image_list")]
        public List<CdekOfficeImage> OfficeImageList { get; set; }

        [JsonPropertyName("work_time_list")]
        public List<CdekWorkTime> WorkTimeList { get; set; }

        [JsonPropertyName("weight_min")]
        public decimal? WeightMin { get; set; }

        [JsonPropertyName("weight_max")]
        public decimal? WeightMax { get; set; }

        [JsonPropertyName("dimensions")]
        public List<CdekDimension> Dimensions { get; set; }
    }

    public class CdekEmail
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }

    public class CdekPhone
    {
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("additional")]
        public string Additional { get; set; }
    }

    public class CdekLocation
    {
        [JsonPropertyName("code")]
        public int? Code { get; set; }                // Код города СДЭК

        [JsonPropertyName("city_code")]
        public int? CityCode { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }              // Название города

        [JsonPropertyName("fias_guid")]
        public string FiasGuid { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }        // Долгота

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }         // Широта

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }            // Регион

        [JsonPropertyName("sub_region")]
        public string SubRegion { get; set; }         // Район

        [JsonPropertyName("address")]
        public string Address { get; set; }           // Полный адрес

        [JsonPropertyName("address_full")]
        public string AddressFull { get; set; }       // Полный адрес (одной строкой)

        [JsonPropertyName("city_uuid")]
        public string CityUuid { get; set; }
    }

    public class CdekOfficeImage
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class CdekWorkTime
    {
        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonPropertyName("periods")]
        public List<CdekWorkPeriod> Periods { get; set; }
    }

    public class CdekWorkPeriod
    {
        [JsonPropertyName("begin")]
        public string Begin { get; set; }             // "09:00"

        [JsonPropertyName("end")]
        public string End { get; set; }               // "18:00"
    }

    public class CdekDimension
    {
        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("length")]
        public int? Length { get; set; }

        [JsonPropertyName("weight")]
        public int? Weight { get; set; }
    }
}
