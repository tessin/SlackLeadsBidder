using System;

namespace SlackLeadsBidder.Utils
{
    public static class DateTimeUtils
    {

        public static double ToUnixEpochSeconds(this DateTime date)
        {
            TimeSpan t = (date - new DateTime(1970, 1, 1));
            return t.TotalSeconds;
        }

    }
}