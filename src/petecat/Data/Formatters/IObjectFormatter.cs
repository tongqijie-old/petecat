﻿using System;
using System.IO;
using System.Text;

namespace Petecat.Data.Formatters
{
    public interface IObjectFormatter
    {
        T ReadObject<T>(string path, Encoding encoding);

        T ReadObject<T>(string stringValue);

        T ReadObject<T>(Stream stream);

        T ReadObject<T>(byte[] byteValues, int offset, int count);

        object ReadObject(Type targetType, string path, Encoding encoding);

        object ReadObject(Type targetType, string stringValue);

        object ReadObject(Type targetType, Stream stream);

        object ReadObject(Type targetType, byte[] byteValues, int offset, int count);

        void WriteObject(object instance, string path, Encoding encoding);

        void WriteObject(object instance, Stream stream);

        string WriteString(object instance);

        byte[] WriteBytes(object instance);
    }
}