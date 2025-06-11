using SensoringAPI.Models;
using SensoringAPI.ModelsDto;

namespace SensoringAPI.Services
{
    public static class DtoMappingExtensions
    {
        public static WasteDetectionResponseDto ToResponseDto(this WasteDetection w)
        {
            return new WasteDetectionResponseDto
            {
                Id = w.Id,
                CameraId = w.CameraId,
                DateTime = w.DateTime,
                Location = w.Location,
                Type = w.Type,
                WeatherId = w.WeatherId,
                Temperature = w.WeatherData?.Temperature,
                WeatherCondition = w.WeatherData?.WeatherCondition,
                Confidence = w.Confidence
            };
        }
    }
}
