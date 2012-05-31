using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;

namespace LibKernel_memcache
{
    public class CacheMultithreadingFacade : ICacheResources
    {

        public CacheMultithreadingFacade(ICacheResources cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            _cache = cache;
            _worker = new Thread(Pump) { IsBackground = true, Priority = ThreadPriority.AboveNormal };
            _worker.Start();
        }

        public void AddToCache(Request request, Response response)
        {
            if (!_cache.CheckCachingStrategy(response.Resource)) return;
            Enqueue(new AddToCacheCommand { Request = request, Response = new Response { Status = ResponseCode.Ok, Resource = response.Resource } });
        }
        public void ClearCache() { Enqueue(new ClearCacheCommand()); }

        public void Revoke(Guid revocation)
        {
            Enqueue(new RevokeCommand { Revocation = revocation });
        }

        public void TriggerGarbageCollection() { Enqueue(new TriggerGCCOmmand()); }

        private void Enqueue(CacheCommand command)
        {
            _queue.Enqueue(command);
            _queuenotification.Set();
        }

        private readonly ConcurrentQueue<CacheCommand> _queue = new ConcurrentQueue<CacheCommand>();

            

        private void Pump()
        {
            while (true)
            {
                CacheCommand command;
                if (_queue.Count > 0)
                {
                    _queuenotification.Reset();
                    if (_queue.TryDequeue(out command)) command.Execute(_cache);
                    else Thread.Yield();
                }
                else
                {
                    if (!_queuenotification.Wait(100)) if (DateTime.Now > _lastGarbageCollection + GarbageCollectionInterval)
                    {
                        _lastGarbageCollection = DateTime.Now;
                        TriggerGarbageCollection();
                    }
                }
            }
        }

        public TimeSpan GarbageCollectionInterval = TimeSpan.FromSeconds(10);

        readonly ManualResetEventSlim _queuenotification = new ManualResetEventSlim();
        private readonly Thread _worker;
        private readonly ICacheResources _cache;
        private DateTime _lastGarbageCollection = DateTime.MinValue;

        abstract class CacheCommand
        {
            internal abstract void Execute(ICacheResources cache);
        }

        class AddToCacheCommand : CacheCommand
        {
            internal Response Response;
            internal Request Request;

            internal override void Execute(ICacheResources cache)
            {
                cache.AddToCache(Request, Response);
            }
        }

        class ClearCacheCommand : CacheCommand
        {
            internal override void Execute(ICacheResources cache)
            {
                cache.ClearCache();
            }
        }

        class TriggerGCCOmmand : CacheCommand
        {
            internal override void Execute(ICacheResources cache)
            {
                cache.TriggerGarbageCollection();
            }
        }

        class RevokeCommand : CacheCommand
        {
            internal Guid Revocation;
            internal override void Execute(ICacheResources cache)
            {
                cache.Revoke(Revocation);
            }
        }


        public bool CheckCachingStrategy(ResourceRepresentation resource)
        {
            return _cache.CheckCachingStrategy(resource);
        }


        public bool Match(string nrl) { return _cache.Match(nrl); }
        public ResourceRepresentation RetrieveOrNull(string nrl) { return _cache.RetrieveOrNull(nrl); }

        public int MaxResourcesInCache { set { _cache.MaxResourcesInCache = value; } }
        public int RemovalChunkSize { set { _cache.RemovalChunkSize = value; } }
        public long MaxCacheSize { set { _cache.MaxCacheSize = value; } }
        public long MaxCacheDurationSeconds { set { _cache.MaxCacheDurationSeconds = value; } }
        public long MinCachableEnergy { set { _cache.MinCachableEnergy = value; } }
        public long MaxCachableSize { set { _cache.MaxCachableSize = value; } }
        public int MinimumExpirationTimesEnergyFactor { set { _cache.MinimumExpirationTimesEnergyFactor = value; } }
        public int EnergySizeTradeoffFactor { set { _cache.EnergySizeTradeoffFactor = value; } }
        public IEnumerable<string> CurrentKeys {get { return _cache.CurrentKeys; }}

        public CacheConfiguration Configuration { get { return _cache.Configuration; } }
        public CacheStatistics Statistics { get { return _cache.Statistics; } }

    }

    
}
