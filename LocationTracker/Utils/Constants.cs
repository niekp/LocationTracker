using System;
namespace LocationTracker.Utils
{
    public static class Constants
    {
        public const int KNOWN_LOCATION_RADIUS = 50;
        public const double MINIMUM_MOVING_SPEED = 3.0;
        public const int MINIMUM_SECONDS_KNOWN_LOCATION = 10;
        public const int MINIMUM_SECONDS_UNKNOWN_LOCATION = (5 * 60);
        public const int MINIMUM_SECONDS_RIDING = 10;

        public const string USER_NAME_CLAIM = System.Security.Claims.ClaimTypes.Email;
        public const string USER_ID_CLAIM = System.Security.Claims.ClaimTypes.Sid;
        public const string USER_EMAIL_CLAIM = System.Security.Claims.ClaimTypes.Email;

        public const string RUNNING_TAG = "Hardlopen";
        public const string WALKING_TAG = "Wandelen";

        public const string BASE_MAP_PNG = "/tmp/location.net.map.png";
    }
}
