using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Algorist.EventSourcing.Core;
using Algorist.EventSourcing.Core.Domain;
using Algorist.EventSourcing.Repository;
using Algorist.EventSourcing.Storage;

namespace Algorist.EventSourcing.Storage.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryStorage<TAggregate, TAggregateKey, TSnapshot>(this IServiceCollection services)
            where TAggregate : AggregateRoot<TAggregateKey>, new()
            where TSnapshot : ISnapshot<TAggregateKey>
        {
            var inMemoryEventStorePath = InMemoryEventStorePath(out var inMemorySnapshotStorePath);
            services.AddScoped<IEventStorageProvider<TAggregate, TAggregateKey>>(provider => new InMemoryEventStorageProvider<TAggregate, TAggregateKey>(inMemoryEventStorePath));
            services.AddScoped<ISnapshotStorageProvider<TSnapshot, TAggregateKey>>(provider => new InMemorySnapshotStorageProvider<TSnapshot, TAggregateKey>(2, inMemorySnapshotStorePath));
            services.AddScoped<IRepository<TAggregate, TAggregateKey>, Repository<TAggregate, TSnapshot, TAggregateKey>>();
            services.AddScoped<ISession<TAggregate, TAggregateKey>, Session<TAggregate, TAggregateKey>>();
            services.AddSingleton<IClock, DefaultClock>();

            return services;
        }

        public static IServiceCollection AddInMemoryStorage<TAggregate, TSnapshot>(this IServiceCollection services)
            where TAggregate : AggregateRoot<Guid>, new() where TSnapshot : ISnapshot<Guid>
        {
            var inMemoryEventStorePath = InMemoryEventStorePath(out var inMemorySnapshotStorePath);
            services.AddScoped<IEventStorageProvider<TAggregate>>(provider => new InMemoryEventStorageProvider<TAggregate>(inMemoryEventStorePath));
            services.AddScoped<IEventStorageProvider<TAggregate, Guid>>(provider => new InMemoryEventStorageProvider<TAggregate>(inMemoryEventStorePath));
            services.AddScoped<ISnapshotStorageProvider<TSnapshot>>(provider => new InMemorySnapshotStorageProvider<TSnapshot>(2, inMemorySnapshotStorePath));
            services.AddScoped<ISnapshotStorageProvider<TSnapshot, Guid>>(provider => new InMemorySnapshotStorageProvider<TSnapshot>(2, inMemorySnapshotStorePath));
            services.AddScoped<IRepository<TAggregate>, Repository<TAggregate, TSnapshot>>();
            services.AddScoped<IRepository<TAggregate, Guid>, Repository<TAggregate, TSnapshot>>();
            services.AddScoped<ISession<TAggregate>, Session<TAggregate>>();
            services.AddScoped<ISession<TAggregate, Guid>, Session<TAggregate>>();
            services.AddSingleton<IClock, DefaultClock>();

            return services;
        }

        private static string InMemoryEventStorePath(out string inMemorySnapshotStorePath)
        {
            string strTempDataFolderPath = AppDomain.CurrentDomain.BaseDirectory + @"App_Data" + Path.DirectorySeparatorChar;
            new FileInfo(strTempDataFolderPath).Directory?.Create();

            var inMemoryEventStorePath = $@"{strTempDataFolderPath}events.stream.dump";
            inMemorySnapshotStorePath = $@"{strTempDataFolderPath}events.snapshot.dump";

            File.Delete(inMemoryEventStorePath);
            File.Delete(inMemorySnapshotStorePath);
            
            return inMemoryEventStorePath;
        }
    }
}