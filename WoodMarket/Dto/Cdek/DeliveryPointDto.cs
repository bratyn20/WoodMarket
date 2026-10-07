namespace WoodMarket.Dto.Cdek
{
    public class DeliveryPointDto
    {
        public string Code { get; set; }           // Код ПВЗ для создания заказа
        public string Name { get; set; }           // Краткое название офиса
        public string Address { get; set; }        // Полный адрес
        public string WorkTime { get; set; }       //Время работы
        public string Phone { get; set; }          //Телефон пункта выдачи
        public double? Longitude { get; set; }     // Долгота
        public double? Latitude { get; set; }      //Широта
        public string Type { get; set; }           // PVZ или POSTAMAT
        public bool IsDressingRoom { get; set; }   // Примерочная
        public bool HaveCashless { get; set; }     // Безнал
        public bool AllowedCod { get; set; }       // Наложенный платёж
    }
}
