using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Lib
{
    public class CustomFormatter
    {
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;          // 584388
        // Number of days from 1/1/0001 to 12/30/1899
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/1969
        internal const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;  // 3652059

        internal const long MinTicks = 0;
        internal const long MaxTicks = DaysTo10000 * TicksPerDay - 1;
        private const long MaxMillis = (long)DaysTo10000 * MillisPerDay;

        private const long FileTimeOffset = DaysTo1601 * TicksPerDay;
        private const long DoubleDateOffset = DaysTo1899 * TicksPerDay;
        // The minimum OA date is 0100/01/01 (Note it's year 100).
        // The maximum OA date is 9999/12/31
        private const long OADateMinAsTicks = (DaysPer100Years - DaysPerYear) * TicksPerDay;
        // All OA dates must be greater than (not >=) OADateMinAsDouble
        private const double OADateMinAsDouble = -657435.0;
        // All OA dates must be less than (not <=) OADateMaxAsDouble
        private const double OADateMaxAsDouble = 2958466.0;

        private const int DatePartYear = 0;
        private const int DatePartDayOfYear = 1;
        private const int DatePartMonth = 2;
        private const int DatePartDay = 3;

        private static readonly int[] DaysToMonth365 = {
            0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};
        private static readonly int[] DaysToMonth366 = {
            0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366};

        //private static Dictionary<int, string> _fourDigits = Enumerable.Range(0, 9999).ToDictionary((fourdigit) => fourdigit, (fourdigit) => fourdigit.ToString("0000"));
        private static readonly string[] FourDigits = Enumerable.Range(0, 10000).Select((fourdigit) => fourdigit.ToString("0000")).ToArray();
        //static CustomFormatter()
        //{
        //    _fourDigits = ;
        //}

        public static unsafe string GetDefaultFormat(DateTime dt, bool isUtc = false)
        {
            var result = new string('Z', isUtc ? 28 : 27);

            long ticks = dt.Ticks;
            // n = number of days since 1/1/0001
            int n = (int)(ticks / TicksPerDay);
            // y400 = number of whole 400-year periods since 1/1/0001
            int y400 = n / DaysPer400Years;
            // n = day number within 400-year period
            n -= y400 * DaysPer400Years;
            // y100 = number of whole 100-year periods within 400-year period
            int y100 = n / DaysPer100Years;
            // Last 100-year period has an extra day, so decrement result if 4
            if (y100 == 4) y100 = 3;
            // n = day number within 100-year period
            n -= y100 * DaysPer100Years;
            // y4 = number of whole 4-year periods within 100-year period
            int y4 = n / DaysPer4Years;
            // n = day number within 4-year period
            n -= y4 * DaysPer4Years;
            // y1 = number of whole years within 4-year period
            int y1 = n / DaysPerYear;
            // Last year has an extra day, so decrement result if 4
            if (y1 == 4) y1 = 3;
            // If year was requested, compute and return it
            int yearNumber = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;

            // n = day number within year
            n -= y1 * DaysPerYear;
            // If day-of-year was requested, return it
            //  if (part == DatePartDayOfYear) return n + 1;
            // Leap year calculation looks different from IsLeapYear since y1, y4,
            // and y100 are relative to year 1, not year 0
            bool leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
            int[] days = leapYear ? DaysToMonth366 : DaysToMonth365;
            // All months have less than 32 days, so n >> 5 is a good conservative
            // estimate for the month
            int m = n >> 5 + 1;
            // m = 1-based month number
            while (n >= days[m]) m++;
            // If month was requested, return it
            var monthNumber = m;
            // Return 1-based day-of-month
            var dayNumber = n - days[m - 1] + 1;

            fixed (char* pt = result)
            {

                var v = FourDigits[yearNumber];
                pt[0] = v[0];
                pt[1] = v[1];
                pt[2] = v[2];
                pt[3] = v[3];
                pt[4] = '-';
                v = FourDigits[monthNumber];
                pt[5] = v[2];
                pt[6] = v[3];
                pt[7] = '-';
                v = FourDigits[dayNumber];
                pt[8] = v[2];
                pt[9] = v[3];
                pt[10] = 'T';
                v = FourDigits[(int)((ticks / TicksPerHour) % 24)];
                pt[11] = v[2];
                pt[12] = v[3];
                pt[13] = ':';
                v = FourDigits[(int)((ticks / TicksPerMinute) % 60)];
                pt[14] = v[2];
                pt[15] = v[3];
                pt[16] = ':';
                v = FourDigits[(int)((ticks / TicksPerSecond) % 60)];
                pt[17] = v[2];
                pt[18] = v[3];
                pt[19] = '.';

                var fraction = ticks % TicksPerSecond;
                v = FourDigits[(int)fraction / 1000];
                pt[20] = v[0];
                pt[21] = v[1];
                pt[22] = v[2];
                pt[23] = v[3];
                fraction = fraction % 1000;
                v = FourDigits[(int)fraction];
                pt[24] = v[1];
                pt[25] = v[2];
                pt[26] = v[3];
            }

            return result;
        }
    }
}


//fixed (char * pt = result)
//            {

//               var v = FourDigits[yearNumber];
//pt[0] = v[0];
//                pt[1] = v[1];
//                pt[2] = v[2];
//                pt[3] = v[3];
//                pt[4] = '-';
//                v = FourDigits[monthNumber];
//                pt[5] = v[2];
//                pt[6] = v[3];
//                pt[7] = '-';
//                v = FourDigits[dayNumber];
//                pt[8] = v[2];
//                pt[9] = v[3];
//                pt[10] = 'T';
//                v = FourDigits[(int)((ticks / TicksPerHour) % 24)];
//                pt[11] = v[2];
//                pt[12] = v[3];
//                pt[13] = ':';
//                v = FourDigits[(int)((ticks / TicksPerMinute) % 60)];
//                pt[14] = v[2];
//                pt[15] = v[3];
//                pt[16] = ':';
//                v = FourDigits[(int)((ticks / TicksPerSecond) % 60)];
//                pt[17] = v[2];
//                pt[18] = v[3];
//                pt[19] = '.';

//                var fraction = ticks % TicksPerSecond;
//v = FourDigits[(int)fraction / 1000];
//                pt[20] = v[0];
//                pt[21] = v[1];
//                pt[22] = v[2];
//                pt[23] = v[3];
//                fraction = fraction % 1000;
//                v = FourDigits[(int)fraction];
//                pt[24] = v[1];
//                pt[25] = v[2];
//                pt[26] = v[3];
//            }