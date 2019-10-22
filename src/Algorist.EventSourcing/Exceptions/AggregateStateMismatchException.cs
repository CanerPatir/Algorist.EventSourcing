using System;

namespace Algorist.EventSourcing.Exceptions
{
    public class AggregateStateMismatchException : Exception
    {
        public AggregateStateMismatchException(string message) : base(message)
        {
        }
    }
}