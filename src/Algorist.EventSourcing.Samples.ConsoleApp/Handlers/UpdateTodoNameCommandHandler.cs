using System;
using System.Threading;
using System.Threading.Tasks;
using Algorist.EventSourcing.Repository;
using Algorist.EventSourcing.Samples.Common.Domain.Schedule;
using MediatR;

namespace Algorist.EventSourcing.Samples.ConsoleApp.Handlers
{
    public class UpdateTodoNameCommand : IRequest
    {
        public UpdateTodoNameCommand(Guid scheduleId, Guid todoId, string todoName)
        {
            ScheduleId = scheduleId;
            TodoId = todoId;
            TodoName = todoName;
        }

        public Guid ScheduleId { get; }
        public Guid TodoId { get; }
        public string TodoName { get; }
    }

    public class UpdateTodoNameCommandHandler : AsyncRequestHandler<UpdateTodoNameCommand>
    {
        private readonly ISession<Schedule> _session;

        public UpdateTodoNameCommandHandler(ISession<Schedule> session)
        {
            _session = session;
        }

        protected override async Task Handle(UpdateTodoNameCommand command, CancellationToken cancellationToken)
        {
            var schedule = await _session.GetByIdAsync(command.ScheduleId);
            schedule.UpdateTodo(command.TodoId, command.TodoName);
            await _session.SaveAsync();
        }
    }
}