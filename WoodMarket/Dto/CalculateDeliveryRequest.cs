using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace WoodMarket.Dto
{
    /// <summary>
    /// Запрос на расчет стоимости доставки
    /// </summary>
    public class CalculateDeliveryRequest
    {
        /// <summary>
        /// Код города получателя (код СДЭК)
        /// </summary>
        [Required(ErrorMessage = "Код города обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "Код города должен быть положительным")]
        public int CityCode { get; set; }

        /// <summary>
        /// Общий вес посылки в килограммах
        /// </summary>
        [Required(ErrorMessage = "Вес обязателен")]
        [Range(0.01, 1000, ErrorMessage = "Вес должен быть от 0.01 до 1000 кг")]
        public decimal TotalWeight { get; set; }

        /// <summary>
        /// Длина посылки в сантиметрах
        /// </summary>
        [Required(ErrorMessage = "Длина обязательна")]
        [Range(1, 500, ErrorMessage = "Длина должна быть от 1 до 500 см")]
        public int Length { get; set; }

        /// <summary>
        /// Ширина посылки в сантиметрах
        /// </summary>
        [Required(ErrorMessage = "Ширина обязательна")]
        [Range(1, 500, ErrorMessage = "Ширина должна быть от 1 до 500 см")]
        public int Width { get; set; }

        /// <summary>
        /// Высота посылки в сантиметрах
        /// </summary>
        [Required(ErrorMessage = "Высота обязательна")]
        [Range(1, 500, ErrorMessage = "Высота должна быть от 1 до 500 см")]
        public int Height { get; set; }
    }
}
