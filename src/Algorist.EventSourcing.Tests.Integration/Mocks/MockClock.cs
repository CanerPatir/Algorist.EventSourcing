using System;
using System.Collections.Generic;
using System.Text;

namespace Algorist.EventSourcing.Tests.Integration.Mocks
{
    public class MockClock : IClock
    {
        public DateTimeOffset Value = DateTimeOffset.Now;

        public DateTimeOffset Now()
        {
            return Value;
        }
    }
}
