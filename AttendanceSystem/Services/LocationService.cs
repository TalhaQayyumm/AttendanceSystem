namespace AttendanceSystem.Services
{
    public class LocationService
    {
        public bool IsWithinAllowedDistance(double lat1, double lon1, double lat2, double lon2, double allowedRadiusMeters)
        {
            if (lat1 == lat2 && lon1 == lon2)
                return true;

            var d1 = lat1 * (Math.PI / 180.0);
            var num1 = lon1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var num2 = lon2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            var distance = 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
            return distance <= allowedRadiusMeters;
        }
    }
}