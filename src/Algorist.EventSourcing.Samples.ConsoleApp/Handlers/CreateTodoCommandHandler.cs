using System;
using System.Threading;
using System.Threading.Tasks;
using Algorist.EventSourcing.Repository;
using Algorist.EventSourcing.Samples.Common.Domain.Schedule;
using MediatR;

namespace Algorist.EventSourcing.Samples.ConsoleApp.Handlers
{
    public class CreateTodoCommand : IRequest
    {
        public CreateTodoCommand(Guid scheduleId, Guid todoId, string todoName)
        {
            ScheduleId = scheduleId;
            TodoId = todoId;
            TodoName = todoName;
        }

        public Guid ScheduleId { get; }
        public Guid TodoId { get; }
        public string TodoName { get; }
    }
    
    public class CreateTodoCommandHandler : AsyncRequestHandler<CreateTodoCommand>
    {
        private readonly ISession<Schedule> _session;

        public CreateTodoCommandHandler(ISession<Schedule> session)
        {
            _session = session;
        }
 
        protected override async Task Handle(CreateTodoCommand command, CancellationToken cancellationToken)
        {
            var schedule = await _session.GetByIdAsync(command.ScheduleId);
            schedule.AddTodo(command.TodoId, command.TodoName);
            await _session.SaveAsync();
        }
    }
}