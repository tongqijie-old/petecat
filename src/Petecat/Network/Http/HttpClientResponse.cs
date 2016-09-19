﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

using Petecat.Data.Errors;
using Petecat.Data.Formatters;
using Petecat.Extension;

namespace Petecat.Network.Http
{
    public class HttpClientResponse : IDisposable
    {
        public HttpClientResponse(HttpWebResponse response)
        {
            Response = response;

            StatusCode = response.StatusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public HttpWebResponse Response { get; private set; }

        public byte[] GetBytes()
        {
            using (var inputStream = Response.GetResponseStream())
            {
                using (var outputStream = new MemoryStream())
                {
                    inputStream.CopyTo(outputStream);
                    return outputStream.ToArray();
                }
            }
        }

        public byte[] GetBytes(Action<int, bool> progress, int bufferSize = 4 * 1024)
        {
            var data = new byte[0];

            using (var inputStream = Response.GetResponseStream())
            {
                var buffer = new byte[bufferSize];
                int count = 0;

                while ((count = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    data = data.Append(new byte[count]);
                    Buffer.BlockCopy(buffer, 0, data, data.Length - count, count);

                    progress(data.Length, false);
                }
            }

            progress(data.Length, true);

            return data;
        }

        public void GetStream(Stream outputStream)
        {
            using (var inputStream = Response.GetResponseStream())
            {
                inputStream.CopyTo(outputStream);
            }
        }

        public void GetStream(Stream outputStream, Action<int, bool> progress, int bufferSize = 4 * 1024)
        {
            var totalReadCount = 0;

            using (var inputStream = Response.GetResponseStream())
            {
                var buffer = new byte[bufferSize];
                int count = 0;

                while ((count = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outputStream.Write(buffer, 0, count);
                    totalReadCount += count;

                    progress(totalReadCount, false);
                }
            }

            progress(totalReadCount, true);
        }

        public string GetString(Encoding encoding)
        {
            return encoding.GetString(GetBytes());
        }

        public TResponse GetObject<TResponse>()
        {
            var objectFormatter = HttpFormatterSelector.Get(Response.ContentType);
            if (objectFormatter == null)
            {
                throw new Exception(string.Format("cannot find object formatter for contenttype {0}", Response.ContentType));
            }

            using (var responseStream = Response.GetResponseStream())
            {
                return objectFormatter.ReadObject<TResponse>(responseStream);
            }
        }

        public TResponse GetObject<TResponse>(IObjectFormatter objectFormatter)
        {
            using (var responseStream = Response.GetResponseStream())
            {
                return objectFormatter.ReadObject<TResponse>(responseStream);
            }
        }

        public void Dispose()
        {
            Response.Close();
        }
    }
}
