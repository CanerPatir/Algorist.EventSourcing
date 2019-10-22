using System;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Samples.ConsoleApp
{
    public class EventPublisher : IEventPublisher
    {
        public Task PublishAsync(IEvent<Guid> @event)
        {
            Console.WriteLine($"Event {@event.TargetVersion + 2}");
            Program.PrintToConsole(@event, ConsoleColor.Cyan);
            Console.WriteLine();

            return Task.CompletedTask;
        }
    }
}