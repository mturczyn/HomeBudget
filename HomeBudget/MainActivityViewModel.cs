using HomeBudget.Model;
using HomeBudget.Model.NbpApiModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HomeBudget
{
#warning separate files for this struct and enum?

    public class MainActivityViewModel : BaseObservable
    {
        public List<Currency> Currencies { get; } = new List<Currency>();
        private double? _euroRate;
        public double? EuroRate
        {
            get => _euroRate;
            set => Set(value, ref _euroRate); 
        }
        private double _homeBudget;
        public double HomeBudget
        {
            get => _homeBudget;
            set => Set(value, ref _homeBudget);
        }
        private bool _internetConnection;
        public bool InternetConnection
        {
            get => _internetConnection;
            set => Set(value, ref _internetConnection);
        }

        public ObservableCollection<Salary> Salaries { get; } = new ObservableCollection<Salary>();

        public MainActivityViewModel()
        {
            Currencies = Enum.GetValues(typeof(Currency)).Cast<Currency>().ToList();
        }

        private bool CheckInternetConnection() => InternetConnection = Connectivity.NetworkAccess != NetworkAccess.Internet;

        public async Task UpdateEuroRateAsync()
        {
            if (CheckInternetConnection())
                return;

            using var http = new HttpClient();

            var jsonResponse = await http.GetStringAsync("https://api.nbp.pl/api/exchangerates/rates/A/EUR?format=json");

            var exchangeRate = JsonSerializer.Deserialize<ExchangeRateRoot>(jsonResponse);

            EuroRate = exchangeRate?.rates[0].mid;
        }

        public Salary AddSalary()
        {
            var newSalary = new Salary();
            Salaries.Add(newSalary);
            return newSalary;
        }

        public Salary AddSalary(double amount, string currencyCode)
        {
            var enumCurrency = Enum.Parse<Currency>(currencyCode);
            var newSalary = new Salary() { SalaryAmount = amount, Currency = enumCurrency };
            Salaries.Add(newSalary);
            return newSalary;
        }

        public void CalculateBudgetForSalaries()
        {
            if (!EuroRate.HasValue)
                throw new NotImplementedException();

            var results = new double[Salaries.Count];
            var recalculatedToPln = Salaries.Select(s => s.Currency switch
            {
                Currency.EUR => s.SalaryAmount * EuroRate.Value,
                Currency.PLN => s.SalaryAmount,
                _ => throw new ArgumentException()
            }).ToArray();
            var sum = recalculatedToPln.Sum();

            for (int i = 0; i < recalculatedToPln.Length; i++)
                Salaries[i].PartAmount = HomeBudget * recalculatedToPln[i] / sum;
        }
    }
}