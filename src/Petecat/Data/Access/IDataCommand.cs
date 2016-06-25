﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Petecat.Data.Access
{
    public interface IDataCommand
    {
        DbCommand GetDbCommand();

        void AddParameter(string parameterName, DbType dbType, ParameterDirection direction, int size);

        void SetParameterValue(string parameterName, object parameterValue);

        void SetParameterValues(string parameterName, object[] parameterValues);

        object GetParameterValue(string parameterName);

        T QueryScalar<T>();

        List<T> QueryEntities<T>() where T : class, new();

        int ExecuteNonQuery();
    }
}