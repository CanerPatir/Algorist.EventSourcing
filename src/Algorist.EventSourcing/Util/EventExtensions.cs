using System;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Exceptions;
using Algorist.EventSourcing.Core;

namespace Algorist.EventSourcing.Util
{
    internal static class EventExtensions
    {
        public static async Task InvokeOnAggregateAsync<TAggregateKey>(this object @event,
            AggregateRoot<TAggregateKey> aggregate, string methodName)
        {
            var method = ReflectionHelper.GetMethod(aggregate.GetType(), methodName, new Type[] { @event.GetType() }); //Find the right method

            if (method != null)
            {
                var task = method.Invoke(aggregate, new[] { @event }); //invoke with the event as argument

                if (task != null && task.GetType() == typeof(Task))
                {
                    await (Task)task;
                }
            }
            else
            {
                throw new AggregateEventOnApplyMethodMissingException($"No event Apply method found on {aggregate.GetType()} for {@event.GetType()}");
            }
        }
    }
}
