using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensoringAPI.Controllers;
using SensoringAPI.Models;
using SensoringAPI.ModelsDto;
using SensoringAPI.Repositories;
using SensoringAPI.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using SensoringAPI.Tests; // Make sure this matches the namespace in TestHelpers.cs

namespace SensoringAPI.Tests.Controllers
{
    [TestClass]
    public class WasteDetectionControllerTests
    {
        private ApplicationDbContext _dbContext;
        private WasteDetectionController _controller;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            var weatherService = new OpenMeteoWeatherService(new HttpClient());

            var repo = new WasteDetectionRepository(weatherService, _dbContext);
            _controller = new WasteDetectionController(_dbContext, repo);
        }

        [TestMethod]
        public async Task AddWasteDetections_NullList_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.AddWasteDetections(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task AddWasteDetections_EmptyList_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.AddWasteDetections(new List<WasteDetection>());

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }
    }
}