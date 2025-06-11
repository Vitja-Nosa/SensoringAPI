namespace SensoringAPI.Services
{
    public static class HelperService
    {
        public static DateTime RoundToNearestHour(DateTime dt)
        {
            int addHour = dt.Minute >= 30 ? 1 : 0;
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0).AddHours(addHour);
        }
    }
}
