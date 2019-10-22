using System.Reflection;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Samples.Common.Domain.Schedule;
using Algorist.EventSourcing.Samples.Common.Domain.Schedule.Snapshots;
using Algorist.EventSourcing.Storage.EventStore;
using Algorist.EventSourcing.Storage.InMemory;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Algorist.EventSourcing.Samples.ConsoleApp
{
    public static class DependencyInjection
    {
        public static ServiceProvider ConfigureServices(bool useEventStore = false)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IEventPublisher, EventPublisher>();

            if (useEventStore)
            {
                services.AddInEventStoreStorage<Schedule, ScheduleSnapshot>();
            }
            else
            {
                services.AddInMemoryStorage<Schedule, ScheduleSnapshot>();
            }

            services.AddMediatR(Assembly.GetExecutingAssembly());

            return services.BuildServiceProvider();
        }
    }
}