using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MyNotSoStupidHome
{
	[Activity(Label = "CircularProgressbarActivity")]
	public class CircularProgressbarActivity : Activity
	{
		RelativeLayout layout;
		Button button;
		Gauge gauge;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.circular_progressbar);
			layout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
			button = FindViewById<Button>(Resource.Id.button1);
			RunOnUiThread(() =>
			{
				gauge = new Gauge(this);
				gauge.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
				gauge.setTotalNicks(120);
				gauge.setMinValue(0);
				gauge.setMaxValue(100);
				gauge.setValuePerNick(1);
				gauge.setInitValue(23);
				gauge.setUpperText("Temperature");
				gauge.setLowerText("°C");

				layout.AddView(gauge);
			});
	

			button.Click += Button_Click;
			// Create your application here
		}

		private void Button_Click(object sender, EventArgs e)
		{
			RunOnUiThread(() =>
			{
				gauge.moveToValue(43.2f);
			});
		}
	}
}