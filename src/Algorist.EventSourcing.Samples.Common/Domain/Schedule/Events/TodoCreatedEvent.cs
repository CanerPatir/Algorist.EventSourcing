using System;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Samples.Common.Domain.Schedule.Events
{
    public class TodoCreatedEvent : Event
    {
        public Guid TodoId { get; }
        public string Text { get; }

        public TodoCreatedEvent(Guid aggregateId, int targetVersion, Guid todoId, string text) : base(aggregateId, targetVersion)
        {
            TodoId = todoId;
            Text = text;
        }
    }
}