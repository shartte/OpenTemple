using System;

namespace SpicyTemple.Core.Systems
{
    public struct GameTime : IComparable<GameTime>
    {
        public int timeInDays;
        public int timeInMs;

        public GameTime(ulong t)
        {
            timeInMs = (int) ((t >> 32) & 0xFFFFFFFF);
            timeInDays = (int) (t & 0xFFFFFFFF);
        }

        public GameTime(int timeInDays, int timeInMs)
        {
            this.timeInDays = timeInDays;
            this.timeInMs = timeInMs;
        }

        // return 1 if t > t2, -1 if t < t2, 0 if equal
        static int Compare(in GameTime t, in GameTime t2)
        {
            if (t2.timeInDays > t.timeInDays)
                return -1;
            if (t2.timeInDays < t.timeInDays)
                return 1;

            // days is equal, compare msec
            if (t2.timeInMs > t.timeInMs)
                return -1;
            return (t2.timeInMs < t.timeInMs) ? 1 : 0;
        }

        public static GameTime FromSeconds(int seconds)
        {
            if (seconds == 0)
            {
                return new GameTime(0, 1);
            }
            else
            {
                return new GameTime(0, seconds * 1000);
            }
        }

        public int CompareTo(GameTime other)
        {
            var timeInDaysComparison = timeInDays.CompareTo(other.timeInDays);
            if (timeInDaysComparison != 0)
            {
                return timeInDaysComparison;
            }

            return timeInMs.CompareTo(other.timeInMs);
        }

        public static bool operator <(GameTime left, GameTime right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(GameTime left, GameTime right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(GameTime left, GameTime right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(GameTime left, GameTime right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}