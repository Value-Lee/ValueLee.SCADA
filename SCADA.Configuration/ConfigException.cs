using System;

namespace SCADA.Configuration
{
    internal class ConfigException : ArgumentException
    {
        public ConfigException(string message) : base(message, string.Empty)
        {
        }
    }
}