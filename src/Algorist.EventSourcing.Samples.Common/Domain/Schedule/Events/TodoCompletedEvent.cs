using System;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Samples.Common.Domain.Schedule.Events
{
    public class TodoCompletedEvent : Event
    {
        public Guid TodoId { get; }

        public TodoCompletedEvent(Guid aggregateId, int targetVersion, Guid todoId) : base(aggregateId, targetVersion)
        {
            TodoId = todoId;
        }
    }
}