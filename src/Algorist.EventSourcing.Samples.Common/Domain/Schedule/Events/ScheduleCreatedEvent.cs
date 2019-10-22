using System;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Samples.Common.Domain.Schedule.Events
{
    public class ScheduleCreatedEvent : Event
    {
        public string ScheduleName { get; }

        public ScheduleCreatedEvent(Guid scheduleId, string scheduleName) : base(scheduleId)
        {
            ScheduleName = scheduleName;
        }
    }
}
