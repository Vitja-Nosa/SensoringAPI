namespace SensoringAPI.ModelsDto
{
    public class OpenMeteoResponseDto
    {
        public Hourly hourly { get; set; }

        public class Hourly
        {
            public List<DateTime> time { get; set; }
            public List<double> temperature_2m { get; set; }
            public List<int> weathercode { get; set; }
        }
    }
}
