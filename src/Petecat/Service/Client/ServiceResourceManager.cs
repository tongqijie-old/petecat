﻿using Petecat.Caching;
using Petecat.Data.Formatters;
using Petecat.Extension;

using System;
using System.Linq;
using System.Text;

namespace Petecat.Service.Client
{
    public class ServiceResourceManager
    {
        public const string CacheObjectName = "RestServiceResources";

        private static ServiceResourceManager _Instance = null;

        public static ServiceResourceManager Instance { get { return _Instance ?? (_Instance = new ServiceResourceManager()); } }

        private ServiceResourceManager()
        {
            CacheObjectManager.Instance.Add<Configuration.ServiceClientConfig>(CacheObjectName, "./Configuration/ServiceResources.config".FullPath(), Encoding.UTF8,
                ObjectFormatterFactory.GetFormatter(ObjectFormatterType.Xml), true);
        }

        public bool TryGetResource(string name, out Configuration.ServiceResourceConfig serviceResourceConfig, out string fullUrl)
        {
            serviceResourceConfig = null;
            fullUrl = null;

            var resources = CacheObjectManager.Instance.GetValue<Configuration.ServiceClientConfig>(CacheObjectName);
            if (resources == null || resources.Resources == null || resources.Hosts == null || resources.Resources.Length == 0 || resources.Hosts.Length == 0)
            {
                return false;
            }

            var resource = resources.Resources.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (resource == null)
            {
                return false;
            }

            var host = resources.Hosts.FirstOrDefault(x => x.Name.Equals(resource.Host, StringComparison.OrdinalIgnoreCase));
            if (host == null)
            {
                return false;
            }

            serviceResourceConfig = resource;
            fullUrl = host.Url.TrimEnd('/') + "/" + resource.Url.TrimStart('/');
            return true;
        }
    }
}