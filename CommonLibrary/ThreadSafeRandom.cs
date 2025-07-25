using System;
using System.Threading;

namespace CommonLibrary
{
    public class ThreadSafeRandom
    {
        private static ThreadLocal<Random> _random =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        public static int Next()
        {
            return _random.Value.Next();
        }

        public static int Next(int maxValue)
        {
            return _random.Value.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return _random.Value.Next(minValue, maxValue);
        }

        public static void Reset()
        {
            _random.Value = new Random(Guid.NewGuid().GetHashCode());
        }
    }
}
