using System.Threading;

namespace dirhash
{
    public class Counters
    {
        private int _counter;
        public int Counter
        {
            get => Interlocked.CompareExchange(ref _counter, 0, 0);
            set => Interlocked.Exchange(ref _counter, value);
        }
    }
}
