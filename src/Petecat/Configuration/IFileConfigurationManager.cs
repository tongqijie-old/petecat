﻿namespace Petecat.Configuration
{
    public interface IFileConfigurationManager : IConfigurationManager
    {
        void LoadFromXml<T>(string filename, string key) where T : class;

        void LoadFromIni(string filename, string key);

        void LoadFromJson<T>(string filename, string key) where T : class;
    }
}