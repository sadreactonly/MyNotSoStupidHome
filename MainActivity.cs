﻿using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using MyNotSoStupidHome.Communication;
using Newtonsoft.Json.Linq;
using Com.Airbnb.Lottie;

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
        private LottieAnimationView animationView;
        private CommunicationService communicationService;
        private UIManager uiManager;

        private LinearLayout linear;
        private Gauge tempGauge;
        private Gauge humidityGauge;

        private readonly string[] animations = new string[5] { "eating_cat.json", "sleeping_cat.json", "surprised_cat.json", "hungry_cat.json", "happy_cat.json" };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);

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
				UpperText = Resources.GetString(Resource.String.temperature),
				LowerText = Resources.GetString(Resource.String.temp_unit)
            };
			
            humidityGauge = new Gauge(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1f),
                TotalNicks = 120,
                MinValue = 0,
                MaxValue = 100,
                ValuePerNick = 1,
                InitValue = 0,
                UpperText = Resources.GetString(Resource.String.humidity),
                LowerText = Resources.GetString(Resource.String.humidity_unit)
            };

            linear.AddView(tempGauge);
            linear.AddView(humidityGauge);
        
            communicationService = new CommunicationService();
            uiManager = new UIManager();

            Task.Run(() => SetupData());

           
        }
        private void PlayRandomAnimation()
        {
            Random rand = new Random();
            RunOnUiThread(() =>
            {
                animationView.SetAnimation(animations[rand.Next(0, 4)]);
                animationView.PlayAnimation();
            });
        }

        private async void FeederButton_Click(object sender, EventArgs e)
		{
            var result = await communicationService.StartFeeder();

            if (result)
                uiManager.CreateToast(this.ApplicationContext, "Feeder is done.");
            else
                uiManager.CreateToast(this.ApplicationContext, "Function is not implemented on server side.");

            PlayRandomAnimation();
        }

		private void SetupData()
        {
            Task.Run(async () =>
            {

              var result =  await communicationService.GetInitialStates();
              var jObj = (JObject)result;

                RunOnUiThread(() => {

                    PlayRandomAnimation();
                    var t = float.Parse(jObj["temperature"].ToString());
                    var h = float.Parse(jObj["humidity"].ToString());

                    tempGauge.MoveToValue(t);
                    humidityGauge.MoveToValue(h);
                    
                    string state = jObj["lightState"].ToString();
                    if (state == "1")
                    {
                        lightSwitch.Checked = true;
                        lightState.Text = string.Format(Resources.GetString(Resource.String.lightState), "on.");
                    }
                    else
                    {
                        lightSwitch.Checked = false;
                        lightState.Text = string.Format(Resources.GetString(Resource.String.lightState), "off.");
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
            if (e.IsChecked)
			{

                await communicationService.SetLight(1);
                lightState.Text = string.Format(Resources.GetString(Resource.String.lightState), "on.");
            }
            else
			{
                await communicationService.SetLight(0);
                lightState.Text = string.Format(Resources.GetString(Resource.String.lightState), "off.");
            }
        }

		private async void PumpButton_Click(object sender, EventArgs e)
		{
            var result = await communicationService.StartPump();
            
            if(result)
                uiManager.CreateToast(this.ApplicationContext, "Pump is done.");

            PlayRandomAnimation();
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
