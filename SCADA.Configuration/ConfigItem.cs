using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Configuration
{
    public class ConfigItem
    {
        public string Description
        {
            get; set;
        }

        public string Display
        {
            get; set;
        }

        public bool Enable { get; set; }

        public decimal MaxValue
        {
            get; set;
        }

        public decimal MinValue
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string[] Options
        {
            get; set;
        }

        public string Regex
        {
            get; set;
        }

        public string RegexNote { get; set; }

        public bool Restart
        {
            get; set;
        }

        public ValueType Type
        {
            get; set;
        }

        public string Unit
        {
            get; set;
        }

        public Action<string> ValidationRule { get; set; }

        public string Value
        {
            get; set;
        }

        public bool Visible
        {
            get; set;
        }
    }
}