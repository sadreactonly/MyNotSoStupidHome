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
	public class UIManager
	{
		public UIManager()
		{

		}

		public void CreateToast(Context context, string message)
		{
			ToastLength duration = ToastLength.Short;

			var toast = Toast.MakeText(context, message, duration);
			toast.Show();
		}
	}
}