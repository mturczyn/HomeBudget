namespace HomeBudget.Model
{
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
}