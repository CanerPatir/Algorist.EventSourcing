using System;
using System.Threading;
using System.Threading.Tasks;
using Algorist.EventSourcing.Repository;
using Algorist.EventSourcing.Samples.Common.Domain.Schedule;
using MediatR;

namespace Algorist.EventSourcing.Samples.ConsoleApp.Handlers
{

    public class CreateScheduleCommand : IRequest<Guid>
    {
        public CreateScheduleCommand(string scheduleName)
        {
            ScheduleName = scheduleName;
        }

        public string ScheduleName { get;}
    }

    public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, Guid>
    {
        private readonly ISession<Schedule> _session;

        public CreateScheduleCommandHandler(ISession<Schedule> session)
        {
            _session = session;
        }

        public async Task<Guid> Handle(CreateScheduleCommand command, CancellationToken cancellationToken)
        {
            var schedule = new Schedule(command.ScheduleName);
            _session.Attach(schedule);
            await _session.SaveAsync(); 
            return schedule.Id;        
        }
    }
}
