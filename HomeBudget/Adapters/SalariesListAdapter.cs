using Android.App;
using Android.Views;
using Android.Widget;
using HomeBudget.Model;
using System;
using System.Collections.Generic;

namespace HomeBudget.Adapters
{
    public class SalariesListAdapter : ArrayAdapter<Salary>
    {
        private class ViewHolder : Java.Lang.Object
        {
            public EditText SalaryAmount { get; set; }
            public Spinner CurrenciesSpinner { get; set; }
            public TextView BudgetPart { get; set; }
        }

        private readonly List<Currency> _currencies;
        public SalariesListAdapter(Android.Content.Context context, List<Salary> objects, List<Currency> currencies) : base(context, Resource.Layout.salary_list_item_layout, objects)
        {
            _currencies = currencies;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView; // re-use an existing view, if one is available
            ViewHolder vw;
            var item = base.GetItem(position);

            if (convertView == null) // otherwise create a new one
            {
                view = (Context as Activity).LayoutInflater.Inflate(Resource.Layout.salary_list_item_layout, null);
                vw = new ViewHolder();
                // Binding to element to underlying object.
                item.PropertyChanged += (sender, eventArgs) =>
                {
                    if (eventArgs.PropertyName != nameof(Salary.PartAmount)) return;
                    vw.BudgetPart.Text = string.Format("{0:0.00}", (sender as Salary).PartAmount);
                };

                vw.SalaryAmount = view.FindViewById<EditText>(Resource.Id.salaryEdit);
                vw.BudgetPart = view.FindViewById<TextView>(Resource.Id.salaryPartText);
                vw.CurrenciesSpinner = view.FindViewById<Spinner>(Resource.Id.salarySpinner);

                vw.SalaryAmount.TextChanged += SalaryAmount_TextChanged;

                vw.CurrenciesSpinner.ItemSelected += Spinner_ItemSelected;
                var dataAdapter = new ArrayAdapter<Currency>(this.Context, Android.Resource.Layout.SimpleSpinnerItem, _currencies);  //simple_spinner_item
                dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);//simple_spinner_dropdown_item
                vw.CurrenciesSpinner.Adapter = dataAdapter;

                vw.SalaryAmount.Tag = position;
                vw.BudgetPart.Tag = position;
                vw.CurrenciesSpinner.Tag = position;

                view.Tag = vw;
            }
            else
            {
                vw = (ViewHolder)view.Tag;
            }

            vw.SalaryAmount.Text = string.Format("{0:0.00}", item.SalaryAmount);

            return view;
        }

        private void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var spinner = sender as Spinner;
            var position = (int)spinner.Tag;
            var item = GetItem(position);
            var strCurrency = spinner.SelectedItem.ToString();
            item.Currency = Enum.Parse<Currency>(strCurrency);
        }

        private void SalaryAmount_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var salaryEdit = sender as EditText;
            var position = (int)salaryEdit.Tag;
            var item = GetItem(position);
            double.TryParse(salaryEdit.Text, out double parsedSalary);
            item.SalaryAmount = parsedSalary;
        }
    }
}