using System;

namespace Algorist.EventSourcing.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AggregateEventApplier : Attribute
    {
    }
}