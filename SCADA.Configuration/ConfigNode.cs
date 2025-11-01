using System.Collections.Generic;
using System.Linq;

namespace SCADA.Configuration
{
    public class ConfigNode
    {
        public ConfigNode()
        { }

        public List<ConfigNode> Children { get; } = new List<ConfigNode>();
        public List<ConfigItem> ConfigItems { get; } = new List<ConfigItem>();
        public string Display { get; set; }
        public bool Enable { get; set; }
        public bool IsLeaf => Children.Count == 0;
        public bool IsRoot => Parent == null;
        public string Name { get; set; }
        public ConfigNode Parent { get; set; }
        public string Path => IsRoot ? Name : Parent.Path + "." + Name;
        public bool Visible { get; set; }

        public static bool Find(string path, bool isTrailConfigItem, ConfigNode node, out ConfigItem configItem, out ConfigNode configNode)
        {
            var names = path.Split('.');
            ConfigNode result = null;
            if (isTrailConfigItem)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (i == names.Length - 1)
                    {
                        break;
                    }

                    if (node == null)
                    {
                        configNode = null;
                        configItem = null;
                        return false;
                    }

                    if (node.Name.Equals(names[i]))
                    {
                        result = node;
                        if (i < names.Length - 1)
                            node = node.Children.FirstOrDefault(x => x.Name.Equals(names[i + 1]));
                    }
                    else
                    {
                        configNode = null;
                        configItem = null;
                        return false;
                    }
                }
                if (result == null)
                {
                    configNode = null;
                    configItem = null;
                    return false;
                }
                configItem = result.ConfigItems.Find(x => x.Name == names[names.Length - 1]);
                if (configItem != null)
                {
                    configNode = result;
                    return true;
                }
                configNode = null;
                configItem = null;
                return false;
            }
            else
            {
                configItem = null;
                for (int i = 0; i < names.Length; i++)
                {
                    if (node == null)
                    {
                        configNode = null;
                        return false;
                    }

                    if (node.Name.Equals(names[i]))
                    {
                        result = node;
                        if (i < names.Length - 1)
                        {
                            node = node.Children.FirstOrDefault(x => x.Name.Equals(names[i + 1]));
                        }
                    }
                    else
                    {
                        configNode = null;
                        return false;
                    }
                }
            }
            configNode = result;
            return true;
        }

        public static bool Find(string path, bool isTrailConfigItem, IEnumerable<ConfigNode> nodes, out ConfigItem configItem, out ConfigNode configNode)
        {
            foreach (var node in nodes)
            {
                if (Find(path, isTrailConfigItem, node, out configItem, out configNode))
                {
                    return true;
                }
            }
            configItem = null;
            configNode = null;
            return false;
        }
    }
}