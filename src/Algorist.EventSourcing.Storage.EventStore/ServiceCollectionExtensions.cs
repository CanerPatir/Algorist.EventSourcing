using System;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Repository;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Storage.EventStore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInEventStoreStorage<TAggregate, TAggregateKey, TSnapshot>(
            this IServiceCollection services, Func<IEventStoreConnection> eventStoreConnectionFactory = null)
            where TAggregate : AggregateRoot<TAggregateKey>, new()
            where TSnapshot : ISnapshot<TAggregateKey>
        {
            services.AddDefaultServices(eventStoreConnectionFactory);
            services.AddScoped<IEventStorageProvider<TAggregate, TAggregateKey>, EventStoreEventStorageProvider<TAggregate, TAggregateKey>>();
            services.AddScoped<ISnapshotStorageProvider<TSnapshot, TAggregateKey>, EventStoreSnapshotStorageProvider<TAggregate, TSnapshot, TAggregateKey>>();
            services.AddScoped<IRepository<TAggregate, TAggregateKey>, Repository<TAggregate, TSnapshot, TAggregateKey>>();
            services.AddScoped<ISession<TAggregate, TAggregateKey>, Session<TAggregate, TAggregateKey>>();
            
            return services;
        }

        public static IServiceCollection AddInEventStoreStorage<TAggregate, TSnapshot>(this IServiceCollection services, Func<IEventStoreConnection> eventStoreConnectionFactory = null)
            where TAggregate : AggregateRoot<Guid>, new() where TSnapshot : ISnapshot<Guid>
        {
            services.AddDefaultServices(eventStoreConnectionFactory);
            services.AddScoped<IEventStorageProvider<TAggregate>, EventStoreEventStorageProvider<TAggregate>>();
            services.AddScoped<IEventStorageProvider<TAggregate, Guid>, EventStoreEventStorageProvider<TAggregate>>();
            services.AddScoped<ISnapshotStorageProvider<TSnapshot>, EventStoreSnapshotStorageProvider<TAggregate, TSnapshot>>();
            services.AddScoped<ISnapshotStorageProvider<TSnapshot, Guid>, EventStoreSnapshotStorageProvider<TAggregate, TSnapshot>>();
            services.AddScoped<IRepository<TAggregate>, Repository<TAggregate, TSnapshot>>();
            services.AddScoped<IRepository<TAggregate, Guid>, Repository<TAggregate, TSnapshot>>();
            services.AddScoped<ISession<TAggregate>, Session<TAggregate>>();
            services.AddScoped<ISession<TAggregate, Guid>, Session<TAggregate>>();

            return services;
        }

        private static void AddDefaultServices(this IServiceCollection services, Func<IEventStoreConnection> eventStoreConnectionFactory)
        {
            services.AddSingleton<ISerializer, Serializer>();
            services.AddSingleton<IClock, DefaultClock>();
            services.AddSingleton<IEventStoreStorageConnectionProvider>(provider =>
                eventStoreConnectionFactory == null
                    ? new EventStoreStorageConnectionProvider()
                    : new EventStoreStorageConnectionProvider(eventStoreConnectionFactory));
        }
    }
}