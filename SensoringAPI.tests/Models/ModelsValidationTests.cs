using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensoringAPI.Models;
using SensoringAPI.ModelsDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SensoringAPI.Tests.Models
{
    [TestClass]
    public class ModelsValidationTests
    {
        // WasteDetection validation tests
        [DataTestMethod]
        [DataRow("CAM1", "2024-06-18T12:00:00", "Plastic", 0.5f, true, "Valid")] // All valid
        [DataRow(null, "2024-06-18T12:00:00", "Plastic", 0.5f, true, "Missing CameraId")] // CameraId null
        [DataRow("CAM1", null, "Plastic", 0.5f, true, "Missing DateTime")] // DateTime null
        [DataRow("CAM1", "2024-06-18T12:00:00", null, 0.5f, true, "Missing Type")] // Type null
        [DataRow("CAM1", "2024-06-18T12:00:00", "Plastic", null, true, "Missing Confidence")] // Confidence null
        [DataRow("CAM1", "2024-06-18T12:00:00", "Plastic", 0.5f, false, "Missing Location")] // Location null
        [DataRow("CAM1", "2024-06-18T12:00:00", "Plastic", -0.1f, true, "Confidence too low")] // Confidence < 0
        [DataRow("CAM1", "2024-06-18T12:00:00", "Plastic", 1.1f, true, "Confidence too high")] // Confidence > 1
        public void WasteDetection_Validation_Works(
            string cameraId,
            string dateTimeStr,
            string type,
            float confidence,
            bool hasLocation,
            string scenario)
        {
            var dateTime = dateTimeStr != null ? DateTime.Parse(dateTimeStr) : default;
            var model = new WasteDetection
            {
                CameraId = cameraId,
                DateTime = dateTime,
                Type = type,
                Confidence = confidence,
                Location = hasLocation ? new LocationDto { Latitude = 0, Longitude = 0 } : null
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(model, context, results, true);

            // Only the first row is valid, others should fail
            if (scenario == "Valid")
                Assert.IsTrue(valid, $"Scenario '{scenario}' should be valid.");
            else
                Assert.IsFalse(valid, $"Scenario '{scenario}' should be invalid.");
        }

        // WeatherData validation tests
        [DataTestMethod]
        [DataRow(20.0, "2024-06-18T12:00:00", "Zonnig", true, "Valid")]
        [DataRow(20.0, "2024-06-18T12:00:00", null, true, "Missing WeatherCondition")]
        [DataRow(20.0, "2024-06-18T12:00:00", "Zonnig", false, "Missing Location")]
        public void WeatherData_Validation_Works(
            double temperature,
            string timeStr,
            string weatherCondition,
            bool hasLocation,
            string scenario)
        {
            var time = DateTime.Parse(timeStr);
            var model = new WeatherData
            {
                Temperature = temperature,
                Time = time,
                WeatherCondition = weatherCondition,
                Location = hasLocation ? new LocationDto { Latitude = 0, Longitude = 0 } : null
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(model, context, results, true);

            // Only the first row is valid, others should fail
            if (scenario == "Valid")
                Assert.IsTrue(valid, $"Scenario '{scenario}' should be valid.");
            else
                Assert.IsFalse(valid, $"Scenario '{scenario}' should be invalid.");
        }
    }
}
