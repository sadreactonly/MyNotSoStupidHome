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

namespace MyNotSoStupidHome.Models
{
	public class DHT11Sensor
	{
		public float Temperature { get; set; }
		public float Humidity { get; set; }
	}
}