using System;
using System.Collections.Generic;
using Algorist.EventSourcing.Core;

namespace Algorist.EventSourcing.Samples.Common.Domain.Schedule.Snapshots
{
    public class ScheduleSnapshot : Snapshot
    {
        public class TodoSnapshot
        {
            public Guid Id { get; set; }
            public string Text { get; set; }
            public bool IsCompleted { get; set; }
        }

        public string ScheduleName { get; set; }
        public IList<TodoSnapshot> Todos { get; set; }

        public ScheduleSnapshot(Guid aggregateId, int version) : base(aggregateId, version)
        {
        }
    }
}
