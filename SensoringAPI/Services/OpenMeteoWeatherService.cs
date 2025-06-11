using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SensoringAPI.Models;
using System.Collections.Generic;
using SensoringAPI.ModelsDto;

public class OpenMeteoWeatherService
{
    private readonly HttpClient _httpClient;

    public OpenMeteoWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherData?> GetWeatherAsync(double latitude, double longitude, DateTime dateTime)
    {
        var date = dateTime.Date.ToString("yyyy-MM-dd");
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&start_date={date}&end_date={date}&hourly=temperature_2m,weathercode&timezone=auto";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        var json = await response.Content.ReadAsStringAsync();
        var weather = JsonSerializer.Deserialize<OpenMeteoResponseDto>(json, options);
        if (weather?.hourly?.time == null || weather.hourly.time.Count == 0) return null;

        int index = FindClosestHourIndex(weather.hourly.time, dateTime);

        var weatherData = new WeatherData
        {
            Temperature = weather.hourly.temperature_2m[index],
            WeatherCondition = TranslateWeatherCode(weather.hourly.weathercode[index]),
            Time = weather.hourly.time[index],
            Location = new LocationDto
            {
                Latitude = latitude,
                Longitude = longitude
            }
        };

        return weatherData;
    }

    private int FindClosestHourIndex(List<DateTime> times, DateTime target)
    {
        int bestIndex = 0;
        TimeSpan minDiff = TimeSpan.MaxValue;

        for (int i = 0; i < times.Count; i++)
        {
            var diff = (times[i] - target).Duration();
            if (diff < minDiff)
            {
                minDiff = diff;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private string TranslateWeatherCode(int code)
    {
        return code switch
        {
            0 => "Zonnig",
            1 or 2 => "Bewolkt",
            45 or 48 => "Mist",
            51 or 53 or 55 or 61 or 63 or 65 or 3 => "Regenachtig",
            71 or 73 or 75 => "Sneeuw",
            95 or 96 or 99 => "Onweer",
            _ => "Onbekend"
        };
    }
}
