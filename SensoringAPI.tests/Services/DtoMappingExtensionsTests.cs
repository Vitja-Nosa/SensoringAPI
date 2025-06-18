using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensoringAPI.Models;
using SensoringAPI.ModelsDto;
using SensoringAPI.Services;
using System;

namespace SensoringAPI.Tests.Services
{
    [TestClass]
    public class DtoMappingExtensionsTests
    {
        [TestMethod]
        public void ToResponseDto_MapsAllPropertiesCorrectly_WhenWeatherDataIsPresent()
        {
            // Arrange
            var location = new LocationDto { Latitude = 52.1, Longitude = 4.3 };
            var weatherData = new WeatherData
            {
                Id = 1,
                Temperature = 18.5,
                Location = location,
                WeatherCondition = "Zonnig",
                Time = DateTime.Now
            };
            var wasteDetection = new WasteDetection
            {
                Id = 42,
                CameraId = "CAM123",
                DateTime = new DateTime(2024, 6, 18, 12, 0, 0),
                Location = location,
                Type = "Plastic",
                WeatherId = 1,
                WeatherData = weatherData,
                Confidence = 0.95f
            };

            // Act
            var dto = wasteDetection.ToResponseDto();

            // Assert
            Assert.AreEqual(wasteDetection.Id, dto.Id);
            Assert.AreEqual(wasteDetection.CameraId, dto.CameraId);
            Assert.AreEqual(wasteDetection.DateTime, dto.DateTime);
            Assert.AreEqual(wasteDetection.Location, dto.Location);
            Assert.AreEqual(wasteDetection.Type, dto.Type);
            Assert.AreEqual(wasteDetection.WeatherId, dto.WeatherId);
            Assert.AreEqual(weatherData.Temperature, dto.Temperature);
            Assert.AreEqual(weatherData.WeatherCondition, dto.WeatherCondition);
            Assert.AreEqual(wasteDetection.Confidence, dto.Confidence);
        }

        [TestMethod]
        public void ToResponseDto_SetsWeatherFieldsToNull_WhenWeatherDataIsNull()
        {
            // Arrange
            var location = new LocationDto { Latitude = 51.5, Longitude = 5.1 };
            var wasteDetection = new WasteDetection
            {
                Id = 7,
                CameraId = "CAM999",
                DateTime = new DateTime(2024, 1, 1, 8, 30, 0),
                Location = location,
                Type = "Papier",
                WeatherId = null,
                WeatherData = null,
                Confidence = 0.5f
            };

            // Act
            var dto = wasteDetection.ToResponseDto();

            // Assert
            Assert.AreEqual(wasteDetection.Id, dto.Id);
            Assert.AreEqual(wasteDetection.CameraId, dto.CameraId);
            Assert.AreEqual(wasteDetection.DateTime, dto.DateTime);
            Assert.AreEqual(wasteDetection.Location, dto.Location);
            Assert.AreEqual(wasteDetection.Type, dto.Type);
            Assert.AreEqual(wasteDetection.WeatherId, dto.WeatherId);
            Assert.IsNull(dto.Temperature);
            Assert.IsNull(dto.WeatherCondition);
            Assert.AreEqual(wasteDetection.Confidence, dto.Confidence);
        }

        [TestMethod]
        public void ToResponseDto_MapsEdgeValuesCorrectly()
        {
            // Arrange
            var location = new LocationDto { Latitude = -90, Longitude = 180 };
            var wasteDetection = new WasteDetection
            {
                Id = 0,
                CameraId = string.Empty,
                DateTime = DateTime.MinValue,
                Location = location,
                Type = "Glas",
                WeatherId = 0,
                WeatherData = null,
                Confidence = 0.0f
            };

            // Act
            var dto = wasteDetection.ToResponseDto();

            // Assert
            Assert.AreEqual(0, dto.Id);
            Assert.AreEqual(string.Empty, dto.CameraId);
            Assert.AreEqual(DateTime.MinValue, dto.DateTime);
            Assert.AreEqual(location, dto.Location);
            Assert.AreEqual("Glas", dto.Type);
            Assert.AreEqual(0, dto.WeatherId);
            Assert.IsNull(dto.Temperature);
            Assert.IsNull(dto.WeatherCondition);
            Assert.AreEqual(0.0f, dto.Confidence);
        }
    }
}
