using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using MyNotSoStupidHome.Communication;
using Newtonsoft.Json.Linq;

namespace MyNotSoStupidHome
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ToggleButton lightSwitch;
        private Button pumpButton;
        private Button dhtButton;
        private TextView temperatureText;
        private TextView humidityText;
       
        private CommunicationService communicationService;
        private UIManager uiManager;

        private LinearLayout linear;
        private Gauge tempGauge;
        private Gauge humidityGauge;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            linear = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
          

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            lightSwitch = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
		

            pumpButton = FindViewById<Button>(Resource.Id.button1);
			pumpButton.Click += PumpButton_Click;

            dhtButton = FindViewById<Button>(Resource.Id.button2);
			dhtButton.Click += DhtButton_Click;

       
            tempGauge = new Gauge(this);
            tempGauge.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1f);
            tempGauge.setTotalNicks(120);
            tempGauge.setMinValue(0);
            tempGauge.setMaxValue(100);
            tempGauge.setValuePerNick(1);
            tempGauge.setInitValue(0);
            tempGauge.setUpperText("Temperature");
            tempGauge.setLowerText("°C");

            humidityGauge = new Gauge(this);
            humidityGauge.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1f);
            humidityGauge.setTotalNicks(120);
            humidityGauge.setMinValue(0);
            humidityGauge.setMaxValue(100);
            humidityGauge.setValuePerNick(1);
            humidityGauge.setInitValue(0);
            humidityGauge.setUpperText("Humidity");
            humidityGauge.setLowerText("%");

            linear.AddView(tempGauge);
            linear.AddView(humidityGauge);
        

            communicationService = new CommunicationService();
            Task.Run(() => SetupData());

            uiManager = new UIManager();
        }

        private void SetupData()
        {
            Task.Run(async () =>
            {
              var result =  await communicationService.GetInitialStates();
              var jObj = (JObject)result;

                RunOnUiThread(() => {
                   

                    var t = float.Parse(jObj["temperature"].ToString());
                    var h = float.Parse(jObj["humidity"].ToString());

                    tempGauge.moveToValue(t);
                    humidityGauge.moveToValue(h);
                    
                    string state = jObj["lightState"].ToString();
                    if (state == "1")
                    {
                        lightSwitch.Checked = true;
                    }
                    else
                    {
                        lightSwitch.Checked = false;
                    }
                    lightSwitch.CheckedChange += LightSwitch_CheckedChange;
                });
               
            });
        }

        private async void DhtButton_Click(object sender, EventArgs e)
		{
            var result = await communicationService.GetTemperatureAndHumidity();
            string message = "Done";
            RunOnUiThread(() => {
                tempGauge.moveToValue(result.Temperature);
                humidityGauge.moveToValue(result.Humidity);
            });
            uiManager.CreateToast(this.ApplicationContext, message);
		}

		private async void LightSwitch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
            bool result;
            string state = string.Empty;
            if (e.IsChecked)
			{

                result = await communicationService.SetLight(1);
                state = "on.";
            }
            else
			{
                result = await communicationService.SetLight(0);
                state = "off.";
            }

            uiManager.CreateToast(this.ApplicationContext, "Light is " + state);
        }

		private async void PumpButton_Click(object sender, EventArgs e)
		{
            var result = await communicationService.StartPump();
            
            if(result)
                uiManager.CreateToast(this.ApplicationContext, "Pump is done.");
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
                StartActivity(typeof(CircularProgressbarActivity));
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
