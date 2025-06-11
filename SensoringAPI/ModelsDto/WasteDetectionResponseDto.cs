namespace SensoringAPI.ModelsDto
{
    public class WasteDetectionResponseDto
    {
        public int Id { get; set; }
        public string CameraId { get; set; }
        public DateTime DateTime { get; set; }
        public LocationDto Location { get; set; }
        public string Type { get; set; }
        public int? WeatherId { get; set; }
        public double? Temperature { get; set; }
        public string? WeatherCondition { get; set; }
        public float Confidence { get; set; }
    }
}
