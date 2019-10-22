using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Algorist.EventSourcing.Repository;
using Algorist.EventSourcing.Samples.Common.Domain.Schedule;
using Algorist.EventSourcing.Samples.ConsoleApp.Handlers;

namespace Algorist.EventSourcing.Samples.ConsoleApp
{ 
    static class Program
    {
        static async Task Main(string[] args)
        {
            await RunAsync();
            Console.ReadLine();
        }

        private static async Task RunAsync()
        {
            var container = DependencyInjection.ConfigureServices(false);

            Guid scheduleId;
            Guid todoId = new Guid();

            using (var scope = container.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                scheduleId = await mediator.Send(new CreateScheduleCommand("test schedule"));
            }

            using (var scope = container.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                await mediator.Send(new CreateTodoCommand(scheduleId, todoId, "todo item 1"));
            }

            using (var scope = container.CreateScope())
            { 
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                await mediator.Send(new CreateTodoCommand(scheduleId, Guid.NewGuid(), "todo item 2"));
            }

            using (var scope = container.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                await mediator.Send(new CreateTodoCommand(scheduleId, Guid.NewGuid(), "todo item 3"));
            }

            using (var scope = container.CreateScope())
            { 
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                await mediator.Send(new UpdateTodoNameCommand(scheduleId, todoId, "todo item 1 updated"));
            }

            using (var scope = container.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                await mediator.Send(new CompleteTodoCommand(scheduleId, todoId));
            }
            
            using (var scope = container.CreateScope())
            {
                var session = scope.ServiceProvider.GetService<ISession<Schedule>>();
                var result = await session.GetByIdAsync(scheduleId);
                Console.WriteLine("--------");
                Console.WriteLine("Final result after applying all events...");
                PrintToConsole(result, ConsoleColor.Green);
            }
        }

        public static void PrintToConsole(object @object, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(JsonConvert.SerializeObject(@object, Formatting.Indented));
            Console.ResetColor();
        }
    }
}
