using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using log4net.ElasticSearch.Infrastructure;

namespace log4net.ElasticSearch.Models
{
    public class Uri
    {
        readonly StringDictionary connectionStringParts;

        Uri(StringDictionary connectionStringParts)
        {
            this.connectionStringParts = connectionStringParts;
        }

        public static implicit operator System.Uri(Uri uri)
        {
            if (!string.IsNullOrWhiteSpace(uri.User()) && !string.IsNullOrWhiteSpace(uri.Password()))
            {
                return
                    new System.Uri($"{uri.Scheme()}://{uri.User()}:{uri.Password()}@{uri.Server()}:{uri.Port()}/{uri.Index()}/{uri.Type()}{uri.Values()}{uri.Bulk()}");
            }
            return string.IsNullOrEmpty(uri.Port())
                ? new System.Uri($"{uri.Scheme()}://{uri.Server()}/{uri.Type()}/{uri.Values()}{uri.Values()}{uri.Bulk()}")
                : new System.Uri($"{uri.Scheme()}://{uri.Server()}:{uri.Port()}/{uri.Index()}/{uri.Type()}{uri.Values()}{uri.Bulk()}");
        }

        public static Uri For(string connectionString)
        {
            return new Uri(connectionString.ConnectionStringParts());
        }

        string User()
        {
            return connectionStringParts[Keys.User];
        }

        string Password()
        {
            return connectionStringParts[Keys.Password];
        }

        string Scheme()
        {
            return connectionStringParts[Keys.Scheme] ?? "http";
        }

        string Server()
        {
            return connectionStringParts[Keys.Server];
        }

        string Port()
        {
            return connectionStringParts[Keys.Port];
        }

        string Routing()
        {
            var routing = connectionStringParts[Keys.Routing];
            if (!string.IsNullOrWhiteSpace(routing))
            {
                return string.Format("routing={0}", routing);
            }

            return string.Empty;
        }

        string Pipeline()
        {
            var pipeline = connectionStringParts[Keys.Pipeline];
            if (!string.IsNullOrWhiteSpace(pipeline))
            {
                return string.Format("pipeline={0}", pipeline);
            }

            return string.Empty;
        }

        string Values()
        {
            List<string> values = new List<string>();

            var routing = this.Routing();
            var pipeline = this.Pipeline();

            if (!string.IsNullOrEmpty(routing))
            {
                values.Add(routing);
            }
            if (!string.IsNullOrEmpty(pipeline))
            {
                values.Add(pipeline);
            }

            return values.Any() ? $"?{string.Join("&", values)}" : string.Empty;
        }

        string Bulk()
        {
            var bufferSize = connectionStringParts[Keys.BufferSize];
            if (Convert.ToInt32(bufferSize) > 1)
            {
                return "/_bulk";
            }

            return string.Empty;
        }

        string Index()
        {
            var index = connectionStringParts[Keys.Index];

            return IsRollingIndex(connectionStringParts)
                       ? "{0}-{1}".With(index, Clock.Date.ToString("yyyy.MM.dd"))
                       : index;
        }

        string Type()
        {
            var index = this.connectionStringParts[Keys.Index];

            if (!string.IsNullOrWhiteSpace(index))
            {
                return index;
            }

            return string.Empty;
        }

        static bool IsRollingIndex(StringDictionary parts)
        {
            return parts.Contains(Keys.Rolling) && parts[Keys.Rolling].ToBool();
        }

        private static class Keys
        {
            public const string Scheme = "Scheme";
            public const string User = "User";
            public const string Password = "Pwd";
            public const string Server = "Server";
            public const string Port = "Port";
            public const string Index = "Index";
            public const string Rolling = "Rolling";
            public const string BufferSize = "BufferSize";
            public const string Routing = "Routing";
            public const string Pipeline = "Pipeline";
        }
    }
}
