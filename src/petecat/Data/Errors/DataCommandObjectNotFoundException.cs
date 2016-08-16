﻿using System;

namespace Petecat.Data.Errors
{
    public class DataCommandObjectNotFoundException : Exception
    {
        public DataCommandObjectNotFoundException(string dataCommandObjectName)
            : base(string.Format("DataCommand '{0}' cannot be found.", dataCommandObjectName))
        {
        }

        public DataCommandObjectNotFoundException(string dataCommandObjectName, Exception innerException)
            : base(string.Format("DataCommand '{0}' cannot be found.", dataCommandObjectName), innerException)
        {
        }
    }
}