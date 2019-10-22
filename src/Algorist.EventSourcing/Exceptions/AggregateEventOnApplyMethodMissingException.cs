using System;

namespace Algorist.EventSourcing.Exceptions
{
    public class AggregateEventOnApplyMethodMissingException : Exception
    {
        public AggregateEventOnApplyMethodMissingException(string message) : base(message)
        {
        }
    }
}