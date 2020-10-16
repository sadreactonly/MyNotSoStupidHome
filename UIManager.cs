using Android.Content;
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