using SensoringAPI.Services.Interfaces;

namespace SensoringAPI.Services
{

    public class TestModeService : ITestModeService
    {
        public bool IsTestMode { get; set; }
    }
}
