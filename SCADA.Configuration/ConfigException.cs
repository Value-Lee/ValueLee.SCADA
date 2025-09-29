using System;

namespace ValueLee.Configuration
{
    internal class ConfigException : ArgumentException
    {
        public ConfigException(string message) : base(message, string.Empty)
        {
        }
    }
}