using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoringAPI.Attributes;
using SensoringAPI.Data;
using SensoringAPI.Models;
using SensoringAPI.Services;

namespace SensoringAPI.Repositories;

public class WasteDetectionRepository
{
    private readonly OpenMeteoWeatherService _weatherService;
    private readonly ApplicationDbContext _dbContext;

    public WasteDetectionRepository(OpenMeteoWeatherService weatherService, ApplicationDbContext dbContext)
    {
        _weatherService = weatherService;
        _dbContext = dbContext;
    }

    private readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Glas", "Papier", "Plastic", "Rookwaar", "Blikje", "Gft"
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
            query = query.Where(w => w.WeatherData != null && w.WeatherData.WeatherCondition == weatherCondition);

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
        var weatherData = GetOrCreateWeatherData(
            wasteDetection.Location.Latitude,
            wasteDetection.Location.Longitude,
            wasteDetection.DateTime);

        if (weatherData != null)
        {
            wasteDetection.WeatherId = weatherData.Id;
            wasteDetection.WeatherData = weatherData;
        }

        return wasteDetection;
    }

    public List<WasteDetection> EnrichWithWeather(List<WasteDetection> wasteDetections)
    {
        var groupedDetections = GroupWasteDetections(wasteDetections);

        foreach (var group in groupedDetections)
        {
            var first = group[0];
            var weatherData = GetOrCreateWeatherData(
                first.Location.Latitude,
                first.Location.Longitude,
                first.DateTime);

            foreach (var detection in group)
            {
                if (weatherData != null)
                {
                    detection.WeatherId = weatherData.Id;
                    detection.WeatherData = weatherData;
                }
            }
        }

        return groupedDetections.SelectMany(group => group).ToList();
    }

    private WeatherData? GetOrCreateWeatherData(double latitude, double longitude, DateTime dateTime)
    {
        double roundedLat = Math.Round(latitude, 2);
        double roundedLon = Math.Round(longitude, 2);
        DateTime roundedHour = HelperService.RoundToNearestHour(dateTime);

        // Try to find existing WeatherData
        var weatherData = _dbContext.WeatherData
            .FirstOrDefault(w =>
                Math.Round(w.Location.Latitude, 2) == roundedLat &&
                Math.Round(w.Location.Longitude, 2) == roundedLon &&
                w.Time == roundedHour);

        if (weatherData == null)
        {
            // Fetch from service and add to DB
            weatherData = _weatherService.GetWeatherAsync(roundedLat, roundedLon, roundedHour).Result;
            if (weatherData != null)
            {
                weatherData.Time = roundedHour;
                weatherData.Location.Latitude = roundedLat;
                weatherData.Location.Longitude = roundedLon;
                _dbContext.WeatherData.Add(weatherData);
                _dbContext.SaveChanges();
            }
        }

        return weatherData;
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