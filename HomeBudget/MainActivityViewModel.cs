using AddTranslationCore.Abstractions;
using HomeBudget.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HomeBudget
{
#warning separate files for this struct and enum?
    public enum Currency
    {
        PLN,
        EUR,
    }
    public class Salary : BaseObservable
    {
        private Currency _currency;
        public Currency Currency 
        { 
            get => _currency;
            set => Set(value, ref _currency); 
        }
        private double _salaryAmount;
        public double SalaryAmount 
        {
            get => _salaryAmount;
            set => Set(value, ref _salaryAmount);
        }
        private double _partAmount;
        public double PartAmount 
        {
            get => _partAmount; 
            set => Set(value, ref _partAmount); 
        }
    }
    public class MainActivityViewModel : BaseObservable
    {
        public List<Currency> Currencies { get; } = new List<Currency>();
        public double? EuroRate { get; private set; }
        public double HomeBudget { get; set; }
        public BindingList<Salary> Salaries { get; } = new BindingList<Salary>();

        public MainActivityViewModel()
        {
            Currencies = Enum.GetValues(typeof(Currency)).Cast<Currency>().ToList();
            Salaries.ListChanged += Salaries_ListChanged;
        }

        public bool CheckInternetConnection() => Connectivity.NetworkAccess != NetworkAccess.Internet;

        public async Task UpdateEuroRateAsync()
        {
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
            if (! EuroRate.HasValue)
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

        private void Salaries_ListChanged(object sender, ListChangedEventArgs e)
        {

        }
    }
}