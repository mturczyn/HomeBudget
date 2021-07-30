namespace HomeBudget.Model.NbpApiModel
{
    public class ExchangeRateRoot
    {
        public string table { get; set; }
        public string currency { get; set; }
        public string code { get; set; }
        public Rate[] rates { get; set; }
    }
}