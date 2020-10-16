using System.Net.Http;
using System.Threading.Tasks;
using MyNotSoStupidHome.Models;
using Newtonsoft.Json;

namespace MyNotSoStupidHome.Communication
{
	public class CommunicationService 
	{

		public CommunicationService()
		{

		}

		public async Task<object> GetInitialStates()
		{
			using (var client = new HttpClient())
			{
				var result = await client.GetAsync("http://192.168.0.154/");
				if (result != null)
				{
					var jsonString = await result.Content.ReadAsStringAsync();
					return JsonConvert.DeserializeObject<object>(jsonString);
				}
				else return null;
			}
		}
		public async Task<bool> StartPump()
		{
			using (var client = new HttpClient())
			{
				var result = await client.GetAsync("http://192.168.0.154/startWatering");
				if (result.IsSuccessStatusCode)
				{
					return true;
				}
				return false;
			}
			
		}

		public async Task<bool> StartFeeder()
		{
			using (var client = new HttpClient())
			{
				var result = await client.GetAsync("http://192.168.0.154/startFeeding");
				if (result.IsSuccessStatusCode)
				{
					return true;
				}
				return false;
			}

		}

		public async Task<bool> SetLight(int state)
		{

			using (var client = new HttpClient())
			{
				//var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");

				var content = new StringContent(state.ToString());
				var result = await client.PostAsync("http://192.168.0.154/setLightState/", content);
				if (result.IsSuccessStatusCode)
				{
					return true;
				}
				return false;
			}
		}
		
		public async Task<DHT11Sensor> GetTemperatureAndHumidity()
		{
			using (var client = new HttpClient())
			{
				var result = await client.GetAsync("http://192.168.0.154/getTemperatureAndHumidity");
				if (result != null)
				{
					var jsonString = await result.Content.ReadAsStringAsync();
					return JsonConvert.DeserializeObject<DHT11Sensor>(jsonString);
				}
				else return null;
			}
		}
		
	}
}