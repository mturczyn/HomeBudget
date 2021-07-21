using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using HomeBudget.Adapters;
using System;
using System.Collections.Generic;

namespace HomeBudget
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MainActivityViewModel _mainActivityController;
        private SalariesListAdapter _listAdapter;

        public MainActivity() : base()
        {
            _mainActivityController = new MainActivityViewModel();
        }
        public MainActivity(int contentLayoutId) : base(contentLayoutId)
        {
            _mainActivityController = new MainActivityViewModel();
        }
        public MainActivity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            _mainActivityController = new MainActivityViewModel();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            InitControlsEventHandlersAndBindings();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void InitControlsEventHandlersAndBindings()
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            var getEuroRate = FindViewById<MaterialButton>(Resource.Id.getEuroRate);
            getEuroRate.Click += GetEuroRate_Click;
            var calculateBudgetBtn = FindViewById<MaterialButton>(Resource.Id.calculateBudgetBtn);
            calculateBudgetBtn.Click += CalculateBudgetBtn_Click;

            var addItemBtn = FindViewById<Android.Widget.Button>(Resource.Id.addItemBtn);
            addItemBtn.Click += AddItemBtn_Click;

            var listView = FindViewById<Android.Widget.ListView>(Resource.Id.listView);
            // Given collection is not bound anyhow to the adapter or list unfortunately.
            // We have to keep both in sync manually.
            _listAdapter = new SalariesListAdapter(this, new List<Salary>(), _mainActivityController.Currencies);
            listView.Adapter = _listAdapter;
        }

        private void CalculateBudgetBtn_Click(object sender, EventArgs e)
        {
            var homeBudget = FindViewById<Android.Widget.EditText>(Resource.Id.homeBudgetMoney);
            _mainActivityController.HomeBudget = double.Parse(homeBudget.Text);

            _mainActivityController.CalculateBudgetForSalaries();
        }

        private async void GetEuroRate_Click(object sender, EventArgs e)
        {
            if (_mainActivityController.CheckInternetConnection())
            {
                var tv = FindViewById<Android.Widget.TextView>(Resource.Id.textBox);
#warning should be taken from resources.
                tv.Text = "No internet connection.";
                return;
            }

            await _mainActivityController.UpdateEuroRateAsync();

            if (_mainActivityController.EuroRate == null)
            {
                return;
            }

            var et = FindViewById<Android.Widget.EditText>(Resource.Id.euroRate);

            et.Text = string.Format("{0:0.0000}", _mainActivityController.EuroRate);
        }

        private void AddItemBtn_Click(object sender, EventArgs e)
        {
            var newSalary = _mainActivityController.AddSalary();
            _listAdapter.Add(newSalary);
        }
    }
}
