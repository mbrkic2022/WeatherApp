using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WeatherApp.Models;

namespace WeatherApp.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class WeatherViewModel
    {
        public Weather Weather { get; set; }
        public bool IsVisible { get; set; }
        public bool IsLoading { get; set; }
        public string PlaceName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public ICommand SearchCommand => new Command(async (searchText) =>
        {
            if (searchText != null)
            {
                PlaceName = searchText.ToString();
                Location location = await GetCoordinates(searchText.ToString());
                await GetWeather(location);
            }
        });

        HttpClient client;

        public WeatherViewModel()
        {
            client = new HttpClient();
        }

        private  async Task<Location> GetCoordinates(string address)
        {
            IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(address);
            Location location = locations?.FirstOrDefault();
            return location;
        }
        private async Task GetWeather(Location location)
        {
            var url= $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&daily=weather_code,temperature_2m_max,wind_speed_10m_max&current=temperature_2m,relative_humidity_2m,weather_code,wind_speed_10m&timezone=Europe%2FBerlin";
            IsLoading = true;
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    var data = await JsonSerializer.DeserializeAsync<Weather>(responseStream);
                    Weather = data;
                    for (int i=0; i<Weather.daily.time.Length; i++)
                    {
                        var daily2 = new Daily2
                        {
                            time = Weather.daily.time[i],
                            temperature_2m_max = Weather.daily.temperature_2m_max[i],
                            wind_speed_10m_max = Weather.daily.wind_speed_10m_max[i],
                            weather_code = Weather.daily.weather_code[i]
                        };
                        Weather.daily2.Add(daily2);
                    }
                    IsVisible = true;
                }
            }
            IsLoading = false;
        }


    }
}
