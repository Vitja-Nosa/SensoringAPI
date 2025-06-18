using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using System.Collections.Generic;

namespace SensoringAPI.Tests.Services;

[TestClass]
public class OpenMeteoWeatherServiceTests
{
    [DataTestMethod]
    [DataRow(0, "Zonnig")]
    [DataRow(1, "Bewolkt")]
    [DataRow(2, "Bewolkt")]
    [DataRow(3, "Regenachtig")]
    [DataRow(45, "Mist")]
    [DataRow(48, "Mist")]
    [DataRow(51, "Regenachtig")]
    [DataRow(53, "Regenachtig")]
    [DataRow(55, "Regenachtig")]
    [DataRow(61, "Regenachtig")]
    [DataRow(63, "Regenachtig")]
    [DataRow(65, "Regenachtig")]
    [DataRow(71, "Sneeuw")]
    [DataRow(73, "Sneeuw")]
    [DataRow(75, "Sneeuw")]
    [DataRow(95, "Onweer")]
    [DataRow(96, "Onweer")]
    [DataRow(99, "Onweer")]
    [DataRow(999, "Onbekend")] // unknown code
    public async Task GetWeatherAsync_ReturnsWeatherData_WhenApiResponseIsValid(int weatherCode, string expectedTranslation)
    {
        // Arrange
        var latitude = 52.1;
        var longitude = 4.3;
        var dateTime = new DateTime(2024, 6, 18, 15, 0, 0);

        var responseDto = new
        {
            hourly = new
            {
                time = new List<DateTime> { dateTime },
                temperature_2m = new List<double> { 20.5 },
                weathercode = new List<int> { weatherCode }
            }
        };
        var json = JsonSerializer.Serialize(responseDto);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new OpenMeteoWeatherService(httpClient);

        // Act
        var result = await service.GetWeatherAsync(latitude, longitude, dateTime);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(20.5, result.Temperature);
        Assert.AreEqual(expectedTranslation, result.WeatherCondition);
        Assert.AreEqual(dateTime, result.Time);
        Assert.AreEqual(latitude, result.Location.Latitude);
        Assert.AreEqual(longitude, result.Location.Longitude);
    }

    [TestMethod]
    public async Task GetWeatherAsync_ReturnsNull_WhenApiResponseHasNoTimes()
    {
        // Arrange
        var latitude = 52.1;
        var longitude = 4.3;
        var dateTime = new DateTime(2024, 6, 18, 15, 0, 0);

        var responseDto = new
        {
            hourly = new
            {
                time = new List<DateTime>(),
                temperature_2m = new List<double>(),
                weathercode = new List<int>()
            }
        };
        var json = JsonSerializer.Serialize(responseDto);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new OpenMeteoWeatherService(httpClient);

        // Act
        var result = await service.GetWeatherAsync(latitude, longitude, dateTime);

        // Assert
        Assert.IsNull(result);
    }
}
