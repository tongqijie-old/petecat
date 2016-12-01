﻿using System;
using System.Text.RegularExpressions;

using Petecat.Extending;
using Petecat.Configuring;
using Petecat.HttpServer.Configuration;
using Petecat.DependencyInjection.Attribute;

namespace Petecat.HttpServer
{
    [DependencyInjectable(Inference = typeof(IHttpApplicationConfigurer), Singleton = true)]
    public class HttpApplicationConfigurer : IHttpApplicationConfigurer
    {
        private IStaticFileConfigurer _StaticFileConfigurer = null;

        public HttpApplicationConfigurer(IStaticFileConfigurer staticFileConfigurer)
        {
            _StaticFileConfigurer = staticFileConfigurer;
        }

        public string GetStaticResourceMapping(string key)
        {
            var httpApplicationConfiguration = _StaticFileConfigurer.GetValue<IHttpApplicationConfiguration>();
            if (httpApplicationConfiguration == null)
            {
                return null;
            }

            if (httpApplicationConfiguration.StaticResourceMappingConfiguration == null
                || httpApplicationConfiguration.StaticResourceMappingConfiguration.Length == 0)
            {
                return null;
            }

            var config = httpApplicationConfiguration.StaticResourceMappingConfiguration.FirstOrDefault(x => string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase));
            if (config == null)
            {
                return null;
            }

            return config.Value;
        }

        public string GetHttpApplicationRouting(string key)
        {
            var httpApplicationConfiguration = _StaticFileConfigurer.GetValue<IHttpApplicationConfiguration>();
            if (httpApplicationConfiguration == null)
            {
                return null;
            }

            if (httpApplicationConfiguration.HttpApplicationRoutingConfiguration == null
                || httpApplicationConfiguration.HttpApplicationRoutingConfiguration.Length == 0)
            {
                return null;
            }

            var config = httpApplicationConfiguration.HttpApplicationRoutingConfiguration.FirstOrDefault(x => string.Equals(key, x.Key, StringComparison.OrdinalIgnoreCase));
            if (config == null)
            {
                return null;
            }

            return config.Value;
        }

        public string ApplyRewriteRule(string url)
        {
            var httpApplicationConfiguration = _StaticFileConfigurer.GetValue<IHttpApplicationConfiguration>();
            if (httpApplicationConfiguration == null)
            {
                return url;
            }

            if (httpApplicationConfiguration.RewriteRuleConfiguration == null
                || httpApplicationConfiguration.RewriteRuleConfiguration.Length == 0)
            {
                return url;
            }

            foreach (var rewriteRule in httpApplicationConfiguration.RewriteRuleConfiguration)
            {
                if (Regex.IsMatch(url, rewriteRule.Key, RegexOptions.IgnoreCase))
                {
                    return rewriteRule.Value;
                }
            }

            return url;
        }
    }
}
