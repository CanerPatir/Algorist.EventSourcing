using System;
using System.Threading;
using System.Threading.Tasks;
using Algorist.EventSourcing.Repository;
using Algorist.EventSourcing.Samples.Common.Domain.Schedule;
using MediatR;

namespace Algorist.EventSourcing.Samples.ConsoleApp.Handlers
{
    public class CompleteTodoCommand : IRequest
    {
        public CompleteTodoCommand(Guid scheduleId, Guid todoId)
        {
            ScheduleId = scheduleId;
            TodoId = todoId;
        }

        public Guid ScheduleId { get; }

        public Guid TodoId { get; }
    }

    public class CompleteTodoCommandHandler : AsyncRequestHandler<CompleteTodoCommand>
    {
        private readonly ISession<Schedule> _session;

        public CompleteTodoCommandHandler(ISession<Schedule> session)
        {
            _session = session;
        }

        protected override async Task Handle(CompleteTodoCommand command, CancellationToken cancellationToken)
        {
            var schedule = await _session.GetByIdAsync(command.ScheduleId);
            await schedule.CompleteTodoAsync(command.TodoId);
            await _session.SaveAsync();
        }
    }
}