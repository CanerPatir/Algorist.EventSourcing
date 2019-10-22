using System;

namespace Algorist.EventSourcing
{
    public interface IClock
    {
        DateTimeOffset Now();
    }
    
    public class DefaultClock: IClock
    {
        public DateTimeOffset Now()
        {
            return DateTimeOffset.Now;
        }
    }
}
