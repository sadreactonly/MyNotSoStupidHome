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
        private Button feederButton;
        private TextView lightState;

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
       
            lightSwitch = FindViewById<ToggleButton>(Resource.Id.toggleLightSwitch);
            lightState = FindViewById<TextView>(Resource.Id.textLightState);

            pumpButton = FindViewById<Button>(Resource.Id.buttonPump);
			pumpButton.Click += PumpButton_Click;

            dhtButton = FindViewById<Button>(Resource.Id.buttonGetDht);
			dhtButton.Click += DhtButton_Click;

            feederButton = FindViewById<Button>(Resource.Id.buttonFeed);
			feederButton.Click += FeederButton_Click;

            tempGauge = new Gauge(this)
			{
				LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1f),
				TotalNicks = 120,
				MinValue = 0,
				MaxValue = 100,
				ValuePerNick = 1,
				InitValue = 0,
				UpperText = "Temperature",
				LowerText = "°C"
			};
			
            humidityGauge = new Gauge(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1f),
                TotalNicks = 120,
                MinValue = 0,
                MaxValue = 100,
                ValuePerNick = 1,
                InitValue = 0,
                UpperText = "Humidity",
                LowerText = "%"
            };

            linear.AddView(tempGauge);
            linear.AddView(humidityGauge);
        
            communicationService = new CommunicationService();
            uiManager = new UIManager();

            Task.Run(() => SetupData());

           
        }

		private async void FeederButton_Click(object sender, EventArgs e)
		{
            var result = await communicationService.StartFeeder();

            if (result)
                uiManager.CreateToast(this.ApplicationContext, "Feeder is done.");
            else
                uiManager.CreateToast(this.ApplicationContext, "Function is not implemented on server side.");
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

                    tempGauge.MoveToValue(t);
                    humidityGauge.MoveToValue(h);
                    
                    string state = jObj["lightState"].ToString();
                    if (state == "1")
                    {
                        lightSwitch.Checked = true;
                        lightState.Text = "Light is on.";
                    }
                    else
                    {
                        lightSwitch.Checked = false;
                        lightState.Text = "Light is off.";
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
                tempGauge.MoveToValue(result.Temperature);
                humidityGauge.MoveToValue(result.Humidity);
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
                lightState.Text = "Light is on.";
            }
            else
			{
                result = await communicationService.SetLight(0);
                lightState.Text = "Light is off.";
            }

            //uiManager.CreateToast(this.ApplicationContext, "Light is " + state);
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
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
