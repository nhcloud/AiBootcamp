
using Newtonsoft.Json;
using System.ComponentModel;

namespace AiBootcamp.SkAgent.Plugins;
public class WeatherPlugin(AppSettings appSettings)
{

    [KernelFunction("GetWeather")]
    [Description("Get weather information")]
    [return: Description("")]
    public async Task<string> GetWeatherInfoAsync(string location)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                string requestUrl = $"{appSettings.WeatherApiUri}?q={location}&appid={appSettings.WeatherApiKey}&units=metric";
                HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var weatherData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                    if (weatherData != null)
                    {
                        string cityName = weatherData.name;
                        string country = weatherData.sys.country;
                        double temperature = weatherData.main.temp;
                        string description = weatherData.weather[0].description;
                        double humidity = weatherData.main.humidity;

                        return $"Weather in {cityName}, {country}; Temperature: {temperature}°C; Condition: {description}; Humidity: {humidity}%";

                    }
                    else
                    {
                        return "Error: Unable to parse weather data.";
                    }
                }
                else
                {
                    return $"Error: Unable to fetch weather data for {location}. Please check the location.";
                }
            }
            catch (Exception ex)
            {
                return $"Exception occurred: {ex.Message}";
            }
        }
    }
}
