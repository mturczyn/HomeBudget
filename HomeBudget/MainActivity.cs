using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using HomeBudget.Adapters;
using HomeBudget.Model;
using System;
using System.Collections.Generic;

namespace HomeBudget
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MainActivityViewModel _mainActivityController;
        private SalariesListAdapter _listAdapter;
        private readonly Dictionary<int, View> _cachedControls = new Dictionary<int, View>();

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
            InitBindings();
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

        public override View FindViewById(int id)
        {
            if (_cachedControls.ContainsKey(id)) return _cachedControls[id];
            var view = base.FindViewById(id);
            if (view != null)
                _cachedControls.Add(id, view);
            return view;
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

        private void InitBindings()
        {
            // Here we bind to viewmodel and observe its chagnes.
            // We bind to every observable object in view model's property additionally.
            _mainActivityController.PropertyChanged += _mainActivityController_PropertyChanged;
            _mainActivityController.Salaries.CollectionChanged += _mainActivityControllerSalaries_CollectionChanged;

            // Further we handle the other direction of binding - changing viewmodel's properties
            // based on changes in UI (forwarding user interactions).
            var homeBudget = FindViewById<EditText>(Resource.Id.homeBudgetMoney);
            homeBudget.TextChanged += (s, e) => _mainActivityController.HomeBudget = double.Parse(homeBudget.Text);

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            var getEuroRate = FindViewById<Button>(Resource.Id.getEuroRate);
            getEuroRate.Click += async (s, e) => await _mainActivityController.UpdateEuroRateAsync();

            var calculateBudgetBtn = FindViewById<Button>(Resource.Id.calculateBudgetBtn);

            calculateBudgetBtn.Click += (s, e) => _mainActivityController.CalculateBudgetForSalaries();

            var addItemBtn = FindViewById<Button>(Resource.Id.addItemBtn);
            addItemBtn.Click += (s, e) => _mainActivityController.AddSalary();

            var listView = FindViewById<ListView>(Resource.Id.listView);
            // Given collection is not bound anyhow to the adapter or list unfortunately.
            // We have to keep both in sync manually.
            // Adapter is responsible fir bindings with its elements.
            _listAdapter = new SalariesListAdapter(this, new List<Salary>(), _mainActivityController.Currencies);
            listView.Adapter = _listAdapter;
        }
        private void UpdateControl<T>(int id, Action<T> updateAction) where T : View
        {
            T control = FindViewById<T>(id);
            updateAction(control);
        }

        #region Binding TO View Model, updating UI based on changes in VM
        private void _mainActivityControllerSalaries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add) return;
            _listAdapter.AddAll(e.NewItems);
        }

        private void _mainActivityController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainActivityViewModel.HomeBudget):
                    UpdateControl<EditText>(
                        Resource.Id.homeBudgetMoney,
                        (tv) =>
                        {
                            if (!double.TryParse(tv.Text, out double result))
                                tv.Text = "0";
                            else if (result != _mainActivityController.HomeBudget)
                                tv.Text = string.Format("{0:0.00}", _mainActivityController.HomeBudget);
                        });
                    break;
                case nameof(MainActivityViewModel.EuroRate):
                    UpdateControl<EditText>(Resource.Id.euroRate,
                        (et) =>
                        {
                            if(!double.TryParse(et.Text, out double result))
                                et.Text = "0";
                            else if (result != _mainActivityController.EuroRate) 
                                et.Text = string.Format("{0:0.0000}", _mainActivityController.EuroRate); 
                        });
                    break;
                case nameof(MainActivityViewModel.InternetConnection):
                    UpdateControl<TextView>(Resource.Id.textBox,
                        tv => tv.Text = _mainActivityController.InternetConnection ? string.Empty : "No internet connection");
                    break;
            }
        }
        #endregion
    }
}
