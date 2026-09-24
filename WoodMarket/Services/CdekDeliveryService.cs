using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using WoodMarket.Models;
using System.Text.Json.Serialization;
using WoodMarket.Dto;

namespace WoodMarket.Services
{
    public class CdekDeliveryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CdekDeliveryService> _logger;
        private readonly CdekOptions _options;
        private string _accessToken;
        private DateTime _tokenExpiry;

        public CdekDeliveryService(
            IHttpClientFactory httpClientFactory,
            IOptions<CdekOptions> options,
            ILogger<CdekDeliveryService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        // ========================================
        // 1. ПОИСК ГОРОДОВ
        // ========================================
        public async Task<List<CdekCity>> SearchCitiesAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                    return new List<CdekCity>();

                var client = _httpClientFactory.CreateClient();
                var token = await GetAccessTokenAsync();

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // ✅ ИСПОЛЬЗУЕМ /location/cities и правильные параметры
                // Параметр "city" требует полного совпадения, но мы можем использовать его для точного поиска
                var url = $"{_options.BaseUrl}/location/cities?city={Uri.EscapeDataString(query)}&country_code=RU";

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Ошибка при поиске городов: {StatusCode}, {Error}",
                        response.StatusCode, error);
                    return new List<CdekCity>();
                }

                var json = await response.Content.ReadAsStringAsync();


                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };

                var cities = JsonSerializer.Deserialize<List<CdekCity>>(json, options);
                return cities ?? new List<CdekCity>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске городов: {Query}", query);
                return new List<CdekCity>();
            }
        }

        // ========================================
        // 2. РАСЧЕТ ДОСТАВКИ
        // ========================================
        public async Task<DeliveryCalculationResult> CalculateDeliveryAsync(CalculateDeliveryRequest calculateDeliveryRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = await GetAccessTokenAsync();

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // ✅ ПРАВИЛЬНЫЙ формат запроса
                var request = new
                {
                    type = 1, // 1 = интернет-магазин
                    currency = 1, // 1 = RUB
                    lang = "rus",
                    from_location = new
                    {
                        code = _options.SenderCityId,
                        country_code = "RU"
                    },
                    to_location = new
                    {
                        code = calculateDeliveryRequest.CityCode,
                        country_code = "RU"
                    },
                    packages = new[]
                    {
                        new
                        {
                            weight = (int)(calculateDeliveryRequest.TotalWeight * 1000), // ✅ вес в ГРАММАХ
                            length = calculateDeliveryRequest.Length,
                            width = calculateDeliveryRequest.Width,
                            height = calculateDeliveryRequest.Height
                        }
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                // ✅ Эндпоинт для расчёта по всем доступным тарифам
                var response = await client.PostAsync(
                    $"{_options.BaseUrl}/calculator/tarifflist",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Ошибка при расчете доставки: {StatusCode}, {Error}",
                        response.StatusCode, error);

                    return new DeliveryCalculationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Ошибка API СДЭК: {response.StatusCode}",
                        Cost = 0
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TariffResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Tariffs == null || !result.Tariffs.Any())
                {
                    return new DeliveryCalculationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Нет доступных тарифов для доставки",
                        Cost = 0
                    };
                }

                // Выбираем самый дешёвый тариф
                var bestTariff = result.Tariffs
                    .Where(t => t.DeliverySum.HasValue && t.DeliverySum > 0)
                    .OrderBy(t => t.DeliverySum)
                    .FirstOrDefault();

                if (bestTariff == null)
                {
                    return new DeliveryCalculationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Не удалось найти подходящий тариф",
                        Cost = 0
                    };
                }

                return new DeliveryCalculationResult
                {
                    IsSuccess = true,
                    TariffName = bestTariff.TariffName,         // "Экономичная посылка склад-склад"
                    Cost = bestTariff.DeliverySum.Value,         // 245.0
                    MinDays = bestTariff.PeriodMin,              // 0
                    MaxDays = bestTariff.PeriodMax,              // 1
                    Currency = "RUB"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете доставки");
                return new DeliveryCalculationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Внутренняя ошибка сервера",
                    Cost = 0
                };
            }
        }

        // ========================================
        // 3. ПОЛУЧЕНИЕ ТОКЕНА ДОСТУПА
        // ========================================
        private async Task<string> GetAccessTokenAsync()
        {
            // Проверяем, есть ли валидный токен
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
                return _accessToken;

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Формируем запрос на получение токена
                var request = new
                {
                    grant_type = "client_credentials",
                    client_id = _options.ClientId,
                    client_secret = _options.ClientSecret
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    $"{_options.BaseUrl}/oauth/token",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка получения токена СДЭК: {StatusCode}, {Error}",
                        response.StatusCode, error);
                    throw new Exception($"Не удалось получить токен СДЭК: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
                    throw new Exception("Токен СДЭК не получен");

                _accessToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Минута запаса

                _logger.LogInformation("Получен новый токен СДЭК, истекает через {ExpiresIn} сек",
                    tokenResponse.ExpiresIn);

                return _accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения токена СДЭК");
                throw;
            }
        }

        // ========================================
        // 4. ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ДЛЯ ДЕСЕРИАЛИЗАЦИИ
        // ========================================
        private class TokenResponse
        {
            [JsonPropertyName("access_token")] // ✅ Явно указываем имя поля в JSON
            public string AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("scope")]
            public string Scope { get; set; }

            [JsonPropertyName("jti")]
            public string Jti { get; set; }
        }

        private class TariffResponse
        {
            [JsonPropertyName("tariff_codes")]
            public List<Tariff> Tariffs { get; set; }

        }

        private class Tariff
        {
            [JsonPropertyName("tariff_code")]
            public int TariffCode { get; set; }

            [JsonPropertyName("tariff_name")]
            public string TariffName { get; set; }

            [JsonPropertyName("tariff_description")]
            public string TariffDescription { get; set; }

            [JsonPropertyName("delivery_mode")]
            public int DeliveryMode { get; set; }

            [JsonPropertyName("delivery_sum")]
            public decimal? DeliverySum { get; set; }

            [JsonPropertyName("period_min")]
            public int? PeriodMin { get; set; }

            [JsonPropertyName("period_max")]
            public int? PeriodMax { get; set; }

            [JsonPropertyName("calendar_min")]
            public int? CalendarMin { get; set; }

            [JsonPropertyName("calendar_max")]
            public int? CalendarMax { get; set; }
        }
    }

    public class CdekOptions
    {
        public string ClientId { get; set; } = "wqGwiQx0gg8mLtiEKsUinjVSICCjtTEP";
        public string ClientSecret { get; set; } = "RmAmgvSgSl1yirlz9QupbzOJVqhCxcP5";
        public string BaseUrl { get; set; } = "https://api.edu.cdek.ru/v2";
        public int SenderCityId { get; set; } = 269; // Томск
        public bool IsTest { get; set; } = true;
    }
}
