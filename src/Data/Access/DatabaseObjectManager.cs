﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;

using Petecat.Caching;
using Petecat.Utility;
using Petecat.Logging;
using System.Text;
using Petecat.Data.Formatters;

namespace Petecat.Data.Access
{
    public class DatabaseObjectManager
    {
        public const string CacheObjectName = "Global_DatabaseObjects";

        private static DatabaseObjectManager _Instance = null;

        public static DatabaseObjectManager Instance { get { return _Instance ?? (_Instance = new DatabaseObjectManager()); } }

        private DatabaseObjectManager() { }

        public void Initialize(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(configPath);
            }

            CacheObjectManager.Instance.Add<Configuration.DatabaseObjectSectionConfig>(CacheObjectName, configPath, 
                ObjectFormatterFactory.GetFormatter(ObjectFormatterType.Xml), true);
        }

        public IDatabaseObject GetDatabaseObject(string name)
        {
            var cacheObject = CacheObjectManager.Instance.GetObject(CacheObjectName);
            if (cacheObject == null)
            {
                throw new Errors.NotInitializedDatabaseObjectManagerException();
            }

            var databaseObjectSectionConfig = cacheObject.GetValue() as Configuration.DatabaseObjectSectionConfig;
            if (databaseObjectSectionConfig == null)
            {
                throw new Errors.DatabaseObjectNotFoundException(name);
            }

            var databaseObjectConfig = databaseObjectSectionConfig.DatabaseObjects.FirstOrDefault(x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (databaseObjectConfig == null)
            {
                throw new Errors.DatabaseObjectNotFoundException(name);
            }

            DatabaseObject databaseObject = null;
            if (string.IsNullOrEmpty(databaseObjectConfig.Provider))
            {
                databaseObject = new DatabaseObject(SqlClientFactory.Instance, databaseObjectConfig.ConnectionString);
            }
            else
            {
                DbProviderFactory providerFactory = null;
                if (_CachedDbProviderFactories.ContainsKey(databaseObjectConfig.Provider))
                {
                    providerFactory = _CachedDbProviderFactories[databaseObjectConfig.Provider];
                }
                else
                {
                    providerFactory = GetDbProviderFactory(databaseObjectConfig.Provider);
                }

                if (providerFactory == null)
                {
                    throw new Errors.DbProviderNotFoundException(databaseObjectConfig.Provider);
                }

                databaseObject = new DatabaseObject(providerFactory, databaseObjectConfig.ConnectionString);
            }

            return databaseObject;
        }

        private Dictionary<string, DbProviderFactory> _CachedDbProviderFactories = new Dictionary<string, DbProviderFactory>();

        private DbProviderFactory GetDbProviderFactory(string providerString)
        {
            Type providerType;
            if (!Reflector.TryGetType(providerString, out providerType))
            {
                LoggerManager.GetLogger().LogEvent(Assembly.GetExecutingAssembly().FullName, LoggerLevel.Error, 
                    string.Format("provider type not found. providerString={0}", providerString));
                return null;
            }

            var providerInstance = providerType.GetField("Instance", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
            if (providerInstance == null)
            {
                LoggerManager.GetLogger().LogEvent(Assembly.GetExecutingAssembly().FullName, LoggerLevel.Error, 
                    string.Format("provider instance not found. providerString={0}", providerString));
                return null;
            }

            if (!providerInstance.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
            {
                LoggerManager.GetLogger().LogEvent(Assembly.GetExecutingAssembly().FullName, LoggerLevel.Error, 
                    string.Format("provider instance is not subclass of DbProviderFactory. providerString={0}", providerString));
                return null;
            }

            var providerFactory = providerInstance.GetValue(null) as DbProviderFactory;
            if (providerFactory == null)
            {
                LoggerManager.GetLogger().LogEvent(Assembly.GetExecutingAssembly().FullName, LoggerLevel.Error, 
                    string.Format("provider instance is null. providerString={0}", providerString));
                return null;
            }

            if (!_CachedDbProviderFactories.ContainsKey(providerString))
            {
                _CachedDbProviderFactories.Add(providerString, providerFactory);
            }

            return providerFactory;
        }
    }
}
