using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoringAPI.Attributes;
using SensoringAPI.Data;
using SensoringAPI.Models;

namespace SensoringAPI.Repositories;

public class WasteDetectionRepository
{
    private readonly OpenMeteoWeatherService _weatherService;

    public WasteDetectionRepository(OpenMeteoWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    private readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Glas", "Papier", "Plastic Fles", "Plastic Overig", "Rookwaar", "Blikje", "Gft"
    };

    private readonly HashSet<string> AllowedWeatherConditions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Zonnig", "Regenachtig", "Bewolkt", "Sneeuw", "Onweer", "Mistig", "Winderig"
    };

    public List<string>? ValidateQueryParameters(
    int pageNumber,
    int pageSize,
    float? confidenceMin,
    float? confidenceMax,
    DateTime? fromDate,
    DateTime? toDate,
    string? type,
    string? weatherCondition)
    {
        var errors = new List<string>();

        if (pageNumber < 1)
            errors.Add("pageNumber must be at least 1.");
        if (pageSize < 1)
            errors.Add("pageSize must be at least 1.");
        if (confidenceMin.HasValue && (confidenceMin < 0.0f || confidenceMin > 1.0f))
            errors.Add("confidenceMin must be between 0.0 and 1.0.");
        if (confidenceMax.HasValue && (confidenceMax < 0.0f || confidenceMax > 1.0f))
            errors.Add("confidenceMax must be between 0.0 and 1.0.");
        if (confidenceMin.HasValue && confidenceMax.HasValue && confidenceMin > confidenceMax)
            errors.Add("confidenceMin cannot be greater than confidenceMax.");
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            errors.Add("fromDate cannot be after toDate.");
        if (!string.IsNullOrWhiteSpace(type) && !AllowedTypes.Contains(type))
            errors.Add($"type '{type}' is not allowed. Allowed values: {string.Join(", ", AllowedTypes)}.");
        if (!string.IsNullOrWhiteSpace(weatherCondition) && !AllowedWeatherConditions.Contains(weatherCondition))
            errors.Add($"weatherCondition '{weatherCondition}' is not allowed. Allowed values: {string.Join(", ", AllowedWeatherConditions)}.");

        return errors.Count > 0 ? errors : null;
    }

    public IQueryable<WasteDetection> ApplyFilters(
    IQueryable<WasteDetection> query,
    string? cameraId,
    int? fromWasteDetectionId,
    string? type,
    string? weatherCondition,
    DateTime? fromDate,
    DateTime? toDate,
    float? confidenceMin,
    float? confidenceMax)
    {
        if (!string.IsNullOrWhiteSpace(cameraId))
            query = query.Where(w => w.CameraId == cameraId);

        if (fromWasteDetectionId.HasValue)
            query = query.Where(w => w.Id >= fromWasteDetectionId.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(w => w.Type == type);

        if (!string.IsNullOrWhiteSpace(weatherCondition))
            query = query.Where(w => w.WeatherCondition == weatherCondition);

        if (fromDate.HasValue)
            query = query.Where(w => w.DateTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(w => w.DateTime <= toDate.Value);

        if (confidenceMin.HasValue)
            query = query.Where(w => w.Confidence >= confidenceMin.Value);

        if (confidenceMax.HasValue)
            query = query.Where(w => w.Confidence <= confidenceMax.Value);

        return query;
    }

    public WasteDetection EnrichWithWeather(WasteDetection wasteDetection)
    {
        WeatherData? weatherData = _weatherService.GetWeatherAsync(
            wasteDetection.Location.Latitude,
            wasteDetection.Location.Longitude,
            wasteDetection.DateTime).Result;

        if (weatherData != null)
        {
            wasteDetection.Temperature = (float)weatherData.Temperature;
            wasteDetection.WeatherCondition = weatherData.WeatherCondition;
        }

        return wasteDetection;
    }

    public List<WasteDetection> EnrichWithWeather(List<WasteDetection> wasteDetection)
    {
        List<List<WasteDetection>> groupedDetections = GroupWasteDetections(wasteDetection);

        for (int i = 0; i < groupedDetections.Count; i++)
        {
            var group = groupedDetections[i];

            WeatherData? weatherData = _weatherService.GetWeatherAsync(
                group[0].Location.Latitude,
                group[0].Location.Longitude,
                group[0].DateTime).Result;

            for (int j = 0; j < group.Count; j++)
            {
                if (weatherData != null)
                {
                    group[j].Temperature = (float)weatherData.Temperature;
                    group[j].WeatherCondition = weatherData.WeatherCondition;
                }
            }
        }

        return groupedDetections.SelectMany(group => group).ToList();
    }

    private List<List<WasteDetection>> GroupWasteDetections(IEnumerable<WasteDetection> detections)
    {
        return detections
            .GroupBy(d => (
                Math.Round(d.Location.Latitude, 2),
                Math.Round(d.Location.Longitude, 2),
                new DateTime(d.DateTime.Year, d.DateTime.Month, d.DateTime.Day, d.DateTime.Hour, 0, 0)
            ))
            .Select(g => g.ToList())
            .ToList();
    }


}