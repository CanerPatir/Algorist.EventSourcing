﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algorist.EventSourcing.Core.Domain;

namespace Algorist.EventSourcing.Repository
{
    public class Session<TAggregate> : Session<TAggregate, Guid>, ISession<TAggregate>
        where TAggregate : AggregateRoot<Guid>, new()
    {
        public Session(IRepository<TAggregate, Guid> repository) : base(repository)
        {
        }
    }

    public class Session<TAggregate, TAggregateKey> : ISession<TAggregate, TAggregateKey>
        where TAggregate : AggregateRoot<TAggregateKey>, new()
    {
        private readonly IRepository<TAggregate, TAggregateKey> _repository;
        private readonly IList<TAggregate> _trackedItems = new List<TAggregate>();
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        public Session(IRepository<TAggregate, TAggregateKey> repository)
        {
            _repository = repository;
        }

        public async Task<TAggregate> GetByIdAsync(TAggregateKey id)
        {
            await _syncLock.WaitAsync();

            try
            {
                var item = _trackedItems.FirstOrDefault(a => a.Id.Equals(id));
                if (item == null)
                {
                    item = await _repository.GetByIdAsync(id);
                    _trackedItems.Add(item);
                }

                return item;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public void Attach(TAggregate aggregate)
        {
            _syncLock.Wait();

            try
            {
                var item = _trackedItems.FirstOrDefault(a => a.Id.Equals(aggregate.Id));

                if (item == null)
                {
                    _trackedItems.Add(aggregate);
                }
                else
                {
                    throw new ArgumentException("Item with the same id is already tracked", nameof(aggregate));
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public void Detach(TAggregate aggregate)
        {
            _syncLock.Wait();

            try
            {
                var item = _trackedItems.FirstOrDefault(a => a.Id.Equals(aggregate.Id));

                if (item != null)
                {
                    _trackedItems.Remove(aggregate);
                }
                else
                {
                    throw new ArgumentException("Item with the same id is not tracked", nameof(aggregate));
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public async Task SaveAsync()
        {
            await _syncLock.WaitAsync();

            try
            {
                foreach (var trackedItem in _trackedItems)
                {
                    await _repository.SaveAsync(trackedItem);
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public void DetachAll()
        {
            _syncLock.Wait();

            try
            {
                _trackedItems.Clear();
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public void Dispose()
        {
            _trackedItems.Clear();
        }
    }
}