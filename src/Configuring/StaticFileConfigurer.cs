﻿using Petecat.Caching;
using Petecat.Monitor;
using Petecat.DependencyInjection.Attribute;

using System;
using Petecat.DependencyInjection;
using Petecat.Caching.Delegates;

namespace Petecat.Configuring
{
    [DependencyInjectable(Inference = typeof(IStaticFileConfigurer), Singleton = true)]
    public class StaticFileConfigurer : IStaticFileConfigurer
    {
        private ICacheContainer _Container = null;

        public ICacheContainer Container { get { return _Container ?? (_Container = new CacheContainerBase()); } }

        public StaticFileConfigurer()
        {
        }

        public void Append(string key, string path, string fileFormat, Type configurationType, 
            CacheItemDirtyChangedHandlerDelegate dirtyChanged = null)
        {
            // build a CacheItem and add to container
            ICacheItem item = null;
            if (string.Equals(fileFormat.Trim(), "xml", StringComparison.OrdinalIgnoreCase))
            {
                item = new XmlFileCacheItem(key, path, configurationType);
            }
            else if (string.Equals(fileFormat.Trim(), "json", StringComparison.OrdinalIgnoreCase))
            {
                item = new JsonFileCacheItem(key, path, configurationType);
            }
            else
            {
                // TODO: throw
            }

            if (dirtyChanged != null)
            {
                item.DirtyChanged += dirtyChanged;
            }

            Container.Add(item);

            // start file monitor
            DependencyInjector.GetObject<IFileSystemMonitor>().Add(this, path, OnFileChanged, null, null, null);
        }

        private void OnFileChanged(string path)
        {
            var item = Container.Get(x => x is IFileCacheItem 
                && string.Equals(path.Replace("\\", "/"), (x as IFileCacheItem).Path.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.IsDirty = true;
            }
        }

        public void Remove(string key, CacheItemDirtyChangedHandlerDelegate dirtyChanged = null)
        {
            var item = Container.Get(key) as IFileCacheItem;
            if (item == null)
            {
                // TODO: throw
            }

            if (dirtyChanged != null)
            {
                item.DirtyChanged -= dirtyChanged;
            }

            // stop file monitor
            DependencyInjector.GetObject<IFileSystemMonitor>().Remove(this, item.Path, OnFileChanged, null, null, null);

            // remove CacheItem from container
            Container.Remove(key);
        }

        public object GetValue(string key)
        {
            return Container.Get(key).GetValue();
        }

        public T GetValue<T>(string key)
        {
            EnsureExists(key, typeof(T));

            return (T)Container.Get(key).GetValue();
        }

        private void EnsureExists(string key, Type configurationType)
        {
            if (Container.Contains(key))
            {
                return;
            }

            // load from DI container
            var obj = DependencyInjector.GetObject(configurationType) as IStaticFileConfigInstance;
            if (obj == null)
            {
                // TODO: throw
            }

            obj.Append(this);
        }

        public bool ContainsKey(string key)
        {
            return Container.Contains(key);
        }
    }
}