using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

using System.Text;
using System.Text.RegularExpressions;

#if NET462_OR_GREATER || NET8_0_OR_GREATER
using System.Threading.Channels;
#endif

using System.Threading.Tasks;
using System.Xml;

namespace SCADA.Configuration
{
    public class PrimitiveConfigSource : IDisposable
    {
#if NET452
        private readonly BlockingCollection<IEnumerable<(string configItem, string value)>> _blockingCollection = new BlockingCollection<IEnumerable<(string configItem, string value)>>();
#elif NET462_OR_GREATER || NET8_0_OR_GREATER
        private readonly Channel<IEnumerable<(string configItem, string value)>> _channel = Channel.CreateUnbounded<IEnumerable<(string configItem, string value)>>();
#endif

        private readonly Dictionary<string, ConfigItem> _configItems = new Dictionary<string, ConfigItem>();

        private readonly Encoding _encoding;

        private readonly bool _restoredEachTimeRestartingApplication;

        private readonly string _xmlDocumentPath;

        private bool _disposed = false;

        private Task _saveTask;

        public PrimitiveConfigSource(string xmlString) : this(xmlString, (string)null)
        {
            _restoredEachTimeRestartingApplication = true;
        }

        public PrimitiveConfigSource(string xmlDocumentPath, Encoding encoding) : this(File.ReadAllText(xmlDocumentPath, encoding), xmlDocumentPath)
        {
            _restoredEachTimeRestartingApplication = false;
            _encoding = encoding;
        }

        private PrimitiveConfigSource(string xmlString, string xmlDocumentPath)
        {
            _xmlDocumentPath = xmlDocumentPath;
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString);
            XmlString = FormatXmlDocument(xmlDocument);

            // handle the root config nodes
            var xmlNodes = xmlDocument.SelectNodes("root/config").OfType<XmlNode>().ToList()
                .Where(n => n.Attributes["type"] == null && n.Attributes["value"] == null).ToList();
            // validate root names
            List<string> rootNames = new List<string>();
            foreach (XmlNode node in xmlNodes)
            {
                if (node.Attributes["name"] == null)
                {
                    throw new ConfigException($"The 'name' attribute is required. Hint:'{node.OuterXml}'");
                }
                var name = node.Attributes["name"].Value.Trim();
                if (name.Contains("."))
                {
                    throw new ConfigException($"The 'name' attribute can't contain '.'. Hint:'{node.OuterXml}'");
                }
                if (rootNames.Contains(name))
                {
                    throw new ConfigException($"The 'name' attribute must be unique within the same node's children. Hint:'{node.OuterXml}'");
                }
                rootNames.Add(name);
            }

            List<ConfigNode> configNodes = new List<ConfigNode>();
            foreach (XmlNode node in xmlNodes)
            {
                configNodes.Add(Build(node, null));
            }
            foreach (ConfigNode node in configNodes)
            {
                Travel(node);
            }

            foreach (var item in _configItems)
            {
                ValidateValue(item.Key, item.Value.Value);
            }

            Nodes = configNodes.ToArray();
#if NET452
            _saveTask = Task.Run(() =>
            {
                try
                {
                    // 这是最推荐的消费方式。它会创建一个可枚举的序列，
                    // 当集合为空时它会阻塞等待，直到有新数据或集合被标记为“完成”。
                    // 当 CompleteAdding() 被调用且集合变空后，foreach 循环会自动结束。
                    foreach (var pairs in _blockingCollection.GetConsumingEnumerable())
                    {
                        if (pairs.Count() > 0)
                        {
                            ModifyValueInXmlDocument(xmlDocument, pairs.Select(x => (x.configItem, x.value)).ToArray());
                            XmlString = FormatXmlDocument(xmlDocument);
                            if (!_restoredEachTimeRestartingApplication)
                            {
                                Save(XmlString);
                            }
                        }
                    }
                }
                catch
                {
                }
            });
#elif NET462_OR_GREATER || NET8_0_OR_GREATER
            async Task Function()
            {
                try
                {
                    var asyncEnumerator = _channel.Reader.ReadAllAsync().GetAsyncEnumerator();
                    try
                    {
                        while (await asyncEnumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            var pairs = asyncEnumerator.Current;
                            if (pairs.Count() > 0)
                            {
                                ModifyValueInXmlDocument(xmlDocument, pairs.Select(x => (x.configItem, x.value)).ToArray());
                                XmlString = FormatXmlDocument(xmlDocument);
                                if (!_restoredEachTimeRestartingApplication)
                                {
                                    Save(XmlString);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (asyncEnumerator != null)
                        {
                            await asyncEnumerator.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                catch
                {
                }
            }

            _saveTask = Function();
#endif
        }

        public event Action<(string configItem, string oldValue, string newValue)[]> ValueSet;

        public ConfigNode[] Nodes { get; }

        public string XmlString
        {
            private set; get;
        }

        public void Dispose()
        {
#if NET452
            if (!_disposed)
            {
                _blockingCollection.CompleteAdding();
                _saveTask?.Wait(); // 等待保存任务完成
                _blockingCollection.Dispose();
                // GC.SuppressFinalize(this); // 不是必须的，因为没析构函数
                _disposed = true;
            }
#elif NET462_OR_GREATER || NET8_0_OR_GREATER
            if (!_disposed)
            {
                _channel.Writer.Complete();
                _saveTask?.Wait(); // 等待保存任务完成
                // GC.SuppressFinalize(this); // 不是必须的，因为没析构函数
                _disposed = true;
            }
#endif
        }

        public ConfigNode GetConfigNode(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && Nodes != null && Nodes.Length > 0)
            {
                foreach (var node in Nodes)
                {
                    if (ConfigNode.Find(path, false, node, out _, out ConfigNode configNode))
                    {
                        return configNode;
                    }
                }
            }
            return null;
        }

        #region Builder

        private ConfigNode Build(XmlNode xmlNode, ConfigNode root) // root是xmlNode生成的节点
        {
            ConfigNode parentNode;
            if (root == null)
            {
                string parentName;
                string parentDisplay;
                if (xmlNode.Attributes["name"] == null)
                {
                    throw new ConfigException($"The 'name' attribute is required. Hint:'{xmlNode.OuterXml}'");
                }
                parentName = xmlNode.Attributes["name"].Value.Trim();
                if (string.IsNullOrWhiteSpace(parentName))
                {
                    throw new ConfigException($"The 'name' attribute cannot be empty. Hint:'{xmlNode.OuterXml}'");
                }
                if (parentName.Contains("."))
                {
                    throw new ConfigException($"The 'name' attribute can't contain '.'. Hint:'{xmlNode.OuterXml}'");
                }
                if (xmlNode.Attributes["display"] == null)
                {
                    parentDisplay = parentName;
                }
                else
                {
                    parentDisplay = xmlNode.Attributes["display"].Value.Trim();
                    if (string.IsNullOrWhiteSpace(parentDisplay))
                    {
                        parentDisplay = parentName;
                    }
                }
                bool visible = true;
                if (xmlNode.Attributes["visible"] == null)
                {
                    visible = true;
                }
                else
                {
                    string visibleText = xmlNode.Attributes["visible"].Value.Trim();
                    if (string.IsNullOrWhiteSpace(visibleText))
                    {
                        visible = true;
                    }
                    else
                    {
                        if (!bool.TryParse(visibleText, out visible))
                        {
                            throw new ConfigException($"The 'visible' attribute must be a boolean value. Hint:'{xmlNode.OuterXml}'");
                        }
                    }
                }
                bool enable = true;
                if (xmlNode.Attributes["enable"] == null)
                {
                    enable = true;
                }
                else
                {
                    string enableText = xmlNode.Attributes["enable"].Value.Trim();
                    if (string.IsNullOrWhiteSpace(enableText))
                    {
                        enable = true;
                    }
                    else
                    {
                        if (!bool.TryParse(enableText, out enable))
                        {
                            throw new ConfigException($"The 'enable' attribute must be a boolean value. Hint:'{xmlNode.OuterXml}'");
                        }
                    }
                }
                parentNode = new ConfigNode
                {
                    Name = parentName,
                    Display = parentDisplay,
                    Visible = visible,
                    Enable = enable
                };
            }
            else
            {
                parentNode = root;
            }

            XmlNodeList xmlNodes = xmlNode.SelectNodes("config");
            foreach (XmlNode xNode in xmlNodes)
            {
                // Category Node
                if (xNode.Attributes["type"] == null && xNode.Attributes["value"] == null)
                {
                    string name = string.Empty;
                    if (xNode.Attributes["name"] == null)
                    {
                        throw new ConfigException($"The 'name' attribute is required. Hint:'{xNode.OuterXml}'");
                    }
                    else
                    {
                        string nameText = xNode.Attributes["name"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(nameText))
                        {
                            throw new ConfigException($"The 'name' attribute cannot be empty. Hint:'{xNode.OuterXml}'");
                        }
                        if (nameText.Contains("."))
                        {
                            throw new ConfigException($"The 'name' attribute can't contain '.'. Hint:'{xmlNode.OuterXml}'");
                        }
                        if (parentNode.Children.Any(c => c.Name == nameText))
                        {
                            throw new ConfigException($"The 'name' attribute must be unique within the same node's children. Hint:'{xNode.OuterXml}'");
                        }
                        name = nameText;
                    }

                    string display = string.Empty;
                    if (xNode.Attributes["display"] == null)
                    {
                        display = name;
                    }
                    else
                    {
                        string displayText = xNode.Attributes["display"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(displayText))
                        {
                            display = name;
                        }
                        else
                        {
                            display = displayText;
                        }
                    }

                    bool visible = true;
                    if (xNode.Attributes["visible"] == null)
                    {
                        visible = true;
                    }
                    else
                    {
                        string visibleText = xNode.Attributes["visible"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(visibleText))
                        {
                            visible = true;
                        }
                        else
                        {
                            if (!bool.TryParse(visibleText, out visible))
                            {
                                throw new ConfigException($"The 'visible' attribute must be a boolean value. Hint:'{xNode.OuterXml}'");
                            }
                        }
                    }
                    bool enable = true;
                    if (xmlNode.Attributes["enable"] == null)
                    {
                        enable = true;
                    }
                    else
                    {
                        string enableText = xmlNode.Attributes["enable"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(enableText))
                        {
                            enable = true;
                        }
                        else
                        {
                            if (!bool.TryParse(enableText, out enable))
                            {
                                throw new ConfigException($"The 'enable' attribute must be a boolean value. Hint:'{xmlNode.OuterXml}'");
                            }
                        }
                    }
                    ConfigNode configNode = new ConfigNode
                    {
                        Name = name,
                        Display = display,
                        Visible = visible,
                        Enable = enable
                    };
                    configNode.Parent = parentNode;
                    parentNode.Children.Add(configNode);
                    Build(xNode, configNode);
                }
                // Data Node
                else if (xNode.Attributes["type"] != null && xNode.Attributes["value"] != null)
                {
                    #region Resolve

                    string name = string.Empty;
                    if (xNode.Attributes["name"] == null)
                    {
                        throw new ConfigException($"The 'name' attribute is required. Hint:'{xNode.OuterXml}'");
                    }
                    else
                    {
                        string nameText = xNode.Attributes["name"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(nameText))
                        {
                            throw new ConfigException($"The 'name' attribute cannot be empty. Hint:'{xNode.OuterXml}'");
                        }
                        if (nameText.Contains("."))
                        {
                            throw new ConfigException($"The 'name' attribute can't contain '.'. Hint:'{xmlNode.OuterXml}'");
                        }
                        if (parentNode.ConfigItems.Any(c => c.Name == nameText))
                        {
                            throw new ConfigException($"The 'name' attribute must be unique within the same node's configItems. Hint:'{xNode.OuterXml}'");
                        }
                        name = nameText;
                    }

                    string display = string.Empty;
                    if (xNode.Attributes["display"] == null)
                    {
                        display = name;
                    }
                    else
                    {
                        string displayText = xNode.Attributes["display"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(displayText))
                        {
                            display = name;
                        }
                        else
                        {
                            display = displayText;
                        }
                    }

                    string description = string.Empty;
                    if (xNode.Attributes["description"] == null)
                    {
                        description = string.Empty;
                    }
                    else
                    {
                        description = xNode.Attributes["description"].Value.Trim();
                    }

                    ValueType valueType = ValueType.String; // Default value type
                    if (xNode.Attributes["type"] == null)
                    {
                        throw new ConfigException($"The 'type' attribute is required. Hint:'{xNode.OuterXml}'");
                    }
                    else
                    {
                        string typeText = xNode.Attributes["type"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(typeText))
                        {
                            throw new ConfigException($"The 'type' attribute cannot be empty. Hint:'{xNode.OuterXml}'");
                        }
                        if (!Enum.TryParse(typeText, true, out valueType))
                        {
                            throw new ConfigException($"Invalid value for 'type' attribute: '{typeText}'. Hint:'{xNode.OuterXml}'");
                        }
                    }

                    decimal maxValue = decimal.MaxValue;
                    if (xNode.Attributes["max"] == null)
                    {
                        maxValue = decimal.MaxValue;
                    }
                    else
                    {
                        string maxValueText = xNode.Attributes["max"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(maxValueText))
                        {
                            maxValue = decimal.MaxValue;
                        }
                        else if (!Utility.TryParse2Decimal(maxValueText, out maxValue))
                        {
                            throw new ConfigException($"Invalid value for 'max' attribute: '{maxValueText}'. Hint:'{xNode.OuterXml}'");
                        }
                    }

                    decimal minValue = decimal.MinValue;
                    if (xNode.Attributes["min"] == null)
                    {
                        minValue = decimal.MinValue;
                    }
                    else
                    {
                        string minValueText = xNode.Attributes["min"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(minValueText))
                        {
                            minValue = decimal.MinValue;
                        }
                        else if (!Utility.TryParse2Decimal(minValueText, out minValue))
                        {
                            throw new ConfigException($"Invalid value for 'min' attribute: '{minValueText}'. Hint:'{xNode.OuterXml}'");
                        }
                    }

                    List<string> options = new List<string>();
                    if (xNode.Attributes["options"] != null)
                    {
                        string optionsText = xNode.Attributes["options"].Value.Trim();
                        if (!string.IsNullOrWhiteSpace(optionsText))
                        {
                            if (optionsText.Split(';').Any(x => string.IsNullOrWhiteSpace(x)))
                            {
                                throw new ConfigException($"Comma cannot appear at both ends, and it cannot appear consecutively. Hint:'{xNode.OuterXml}'");
                            }
                            options = optionsText.Split(';').Select(o => o.Trim()).ToList();
                            if (valueType == ValueType.Boolean)
                            {
                                if (options.Count != 2)
                                {
                                    throw new ConfigException($"The 'options' attribute must contain only two options for boolean type. Hint:'{xNode.OuterXml}'");
                                }
                            }
                        }
                        else
                        {
                            if (valueType == ValueType.Boolean)
                            {
                                options.Add("Yes");
                                options.Add("No");
                            }
                        }
                    }
                    else
                    {
                        if (valueType == ValueType.Boolean)
                        {
                            options.Add("Yes");
                            options.Add("No");
                        }
                    }

                    string regex = string.Empty;
                    if (xNode.Attributes["regex"] != null)
                    {
                        regex = xNode.Attributes["regex"].Value.Trim();
                    }

                    string regexNote = regex;
                    if (xNode.Attributes["regexnote"] != null)
                    {
                        regexNote = xNode.Attributes["regexnote"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(regexNote))
                        {
                            regexNote = regex;
                        }
                    }

                    string unit = string.Empty;
                    if (xNode.Attributes["unit"] != null)
                    {
                        unit = xNode.Attributes["unit"].Value.Trim();
                    }

                    bool visible = true;
                    if (xNode.Attributes["visible"] == null)
                    {
                        visible = true;
                    }
                    else
                    {
                        string visibleText = xNode.Attributes["visible"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(visibleText))
                        {
                            visible = true;
                        }
                        else
                        {
                            if (!bool.TryParse(visibleText, out visible))
                            {
                                throw new ConfigException($"The 'visible' attribute must be a boolean value. Hint:'{xNode.OuterXml}'");
                            }
                        }
                    }

                    bool enable = true;
                    if (xmlNode.Attributes["enable"] == null)
                    {
                        enable = true;
                    }
                    else
                    {
                        string enableText = xmlNode.Attributes["enable"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(enableText))
                        {
                            enable = true;
                        }
                        else
                        {
                            if (!bool.TryParse(enableText, out enable))
                            {
                                throw new ConfigException($"The 'enable' attribute must be a boolean value. Hint:'{xmlNode.OuterXml}'");
                            }
                        }
                    }

                    bool restart = false;
                    if (xNode.Attributes["restart"] == null)
                    {
                        restart = false;
                    }
                    else
                    {
                        string restartText = xNode.Attributes["restart"].Value.Trim();
                        if (string.IsNullOrWhiteSpace(restartText))
                        {
                            restart = false;
                        }
                        else
                        {
                            if (!bool.TryParse(restartText, out restart))
                            {
                                throw new ConfigException($"The 'restart' attribute must be a boolean value. Hint:'{xNode.OuterXml}'");
                            }
                        }
                    }

                    string value = string.Empty;
                    if (xNode.Attributes["value"] == null)
                    {
                        throw new ConfigException($"The 'value' attribute is required. Hint:'{xNode.OuterXml}'");
                    }
                    else
                    {
                        value = xNode.Attributes["value"].Value.Trim();

                        if (valueType == ValueType.Boolean)
                        {
                            if (!bool.TryParse(value, out _))
                            {
                                throw new ConfigException($"The 'value' attribute must be a boolean value. Hint:'{xNode.OuterXml}'");
                            }
                        }
                        else if (valueType == ValueType.Integer)
                        {
                            if (!Utility.TryParse2Long(value, out _))
                            {
                                throw new ConfigException($"The 'value' attribute must be a integer value. Hint:'{xNode.OuterXml}'");
                            }
                        }
                        else if (valueType == ValueType.Decimal)
                        {
                            if (!Utility.TryParse2Decimal(value, out _))
                            {
                                throw new ConfigException($"The 'value' attribute must be a numeric value. Hint:'{xNode.OuterXml}'");
                            }
                        }
                    }

                    #endregion Resolve

                    var configItem = new ConfigItem
                    {
                        Name = name,
                        Display = display,
                        Type = valueType,
                        Unit = unit,
                        Description = description,
                        MinValue = minValue,
                        MaxValue = maxValue,
                        Value = value,
                        Options = options.ToArray(),
                        Regex = regex,
                        RegexNote = regexNote,
                        Visible = visible,
                        Enable = enable,
                        Restart = restart
                    };

                    parentNode.ConfigItems.Add(configItem);
                }
                else
                {
                    throw new ConfigException($"Can't just have only either 'type' or 'value' configed. Hint:'{xNode.OuterXml}'");
                }
            }

            return parentNode;
        }

        private void Travel(ConfigNode configNode)
        {
            foreach (var item in configNode.ConfigItems)
            {
                string configPath = configNode.Path + "." + item.Name;

                var options = CustomizeOptionsSource(configPath);
                if (options != null && options.Length > 0)
                {
                    item.Options = options;
                }

                item.ValidationRule = new Action<string>((string text) =>
                {
                    ValidateValue(configPath, text);
                });

                _configItems[configPath] = item;
            }
            foreach (var node in configNode.Children)
            {
                Travel(node);
            }
        }

        #endregion Builder

        #region Setter & Getter

        public TValue GetValue<TValue>(string configItem)
        {
            if (!_configItems.ContainsKey(configItem))
            {
                throw new KeyNotFoundException($"Config item '{configItem}' not found.");
            }

            var csType = typeof(TValue);
            var stringValue = _configItems[configItem].Value;
            var configType = _configItems[configItem].Type;
            try
            {
                if (csType == typeof(string))
                {
                    return (TValue)Convert.ChangeType(stringValue, csType);
                }
                else if (csType == typeof(int) || csType == typeof(long) || csType == typeof(short) || csType == typeof(byte))
                {
                    if (configType == ValueType.Integer)
                    {
                        if (Utility.TryParse2Long(stringValue, out long longValue))
                        {
                            return (TValue)Convert.ChangeType(longValue, csType);
                        }
                        throw new InvalidCastException($"Cannot convert config item '{configItem}' to type {csType.Name}.");
                    }
                    throw new InvalidCastException($"'{configItem}' type is not {ValueType.Integer}.");
                }
                else if (csType == typeof(bool))
                {
                    if (configType == ValueType.Boolean)
                    {
                        return (TValue)Convert.ChangeType(stringValue, csType);
                    }
                    throw new InvalidCastException($"'{configItem}' type is not {ValueType.Boolean}.");
                }
                else if (csType == typeof(double) || csType == typeof(float) || csType == typeof(decimal))
                {
                    if (configType == ValueType.Decimal)
                    {
                        if (Utility.TryParse2Decimal(stringValue, out var decimalValue))
                        {
                            return (TValue)Convert.ChangeType(decimalValue, csType);
                        }
                        else if (Utility.TryParse2Long(stringValue, out long longValue))
                        {
                            return (TValue)Convert.ChangeType(longValue, csType);
                        }
                        else
                        {
                            throw new InvalidCastException($"Cannot convert config item '{configItem}' to type {csType.Name}.");
                        }
                    }
                    if (configType == ValueType.Integer)
                    {
                        if (Utility.TryParse2Long(stringValue, out long longValue))
                        {
                            return (TValue)Convert.ChangeType(longValue, csType);
                        }
                        throw new InvalidCastException($"Cannot convert config item '{configItem}' to type {csType.Name}.");
                    }
                    throw new InvalidCastException($"'{configItem}' type is not {ValueType.Decimal} or {ValueType.Integer}.");
                }
                else if (csType == typeof(DateTime))
                {
                    if (configType == ValueType.DateTime)
                    {
                        if (Utility.TryParse2DateTime(stringValue, out DateTime dateTime))
                        {
                            return (TValue)Convert.ChangeType(dateTime, csType);
                        }
                        throw new InvalidCastException($"Cannot convert config item '{configItem}' to type {csType.Name}.");
                    }
                    throw new InvalidCastException($"'{configItem}' type is not {ValueType.DateTime}.");
                }
                else if (csType == typeof(System.Drawing.Color))
                {
                    if (configType == ValueType.Color)
                    {
                        if (Utility.TryParse2Color(stringValue, out System.Drawing.Color color))
                        {
                            return (TValue)((object)color);
                        }
                    }
                    throw new InvalidCastException($"'{configItem}' type is not {ValueType.Color}.");
                }
                else if (csType == typeof(FileInfo))
                {
                    if (configType == ValueType.File)
                    {
                        Utility.TryParse2File(stringValue, out var fileInfo);
                        return (TValue)((object)fileInfo);
                    }
                    throw new InvalidCastException($"'{configItem}' type is not {ValueType.File}.");
                }
                else if (csType == typeof(DirectoryInfo))
                {
                    if (configType == ValueType.Folder)
                    {
                        Utility.TryParse2Directory(stringValue, out var directoryInfo);
                        return (TValue)((object)directoryInfo);
                    }
                    throw new InvalidCastException($"'{configItem}' type is not {ValueType.Folder}.");
                }
                else
                {
                    var supportedTypes = string.Join(", ", new[] { "string", "int", "long", "short", "byte", "bool", "double", "float", "decimal", "Color", "FileInfo", "DirectoryInfo", "DateTime" });
                    throw new NotSupportedException($"Type '{csType.Name}' is not supported and only supports {supportedTypes}.");
                }
            }
            catch
            {
                throw new InvalidCastException($"Cannot convert config item '{configItem}' to type {csType.Name}.");
            }
        }

        public void SetValue(string configItem, object value)
        {
            SetValue((configItem, value));
        }

        public void SetValue(params (string configItem, object value)[] configValuePairs)
        {
            if (configValuePairs == null)
            {
                throw new ArgumentException("At least one config item must be provided.", nameof(configValuePairs));
            }
            if (configValuePairs.Length == 0)
            {
                return; // No need to set values if the array is empty
            }
            if (configValuePairs.Any(x => string.IsNullOrWhiteSpace(x.configItem)))
            {
                throw new ArgumentException("Config item cannot be null or empty.", nameof(configValuePairs));
            }
            if (configValuePairs.Any(x => x.value == null))
            {
                throw new ArgumentNullException("Config value cannot be null.", nameof(configValuePairs));
            }
            if (configValuePairs.Any(x => !_configItems.ContainsKey(x.configItem)))
            {
                throw new KeyNotFoundException("One or more config items not found.");
            }

            (string configItem, string value)[] configValueStringPairs = new (string configItem, string value)[configValuePairs.Length];
            for (int i = 0; i < configValuePairs.Length; i++)
            {
                configValueStringPairs[i] = (configValuePairs[i].configItem, Convert2String(configValuePairs[i].value));
            }

            List<(string configItem, string value)> needModifyinglist = new List<(string configItem, string value)>();

            foreach (var configValuePair in configValueStringPairs)
            {
                if (_configItems[configValuePair.configItem].Value != configValuePair.value)
                {
                    needModifyinglist.Add((configValuePair.configItem, configValuePair.value));
                }
            }

            if (needModifyinglist.Count > 0)
            {
                ValidateValue(needModifyinglist.Select(x => (x.configItem, x.value as object)).ToArray());
                List<(string configItem, string oldValue, string newValue)> list = new List<(string configItem, string oldValue, string newValue)>();
                foreach (var item in needModifyinglist)
                {
                    list.Add((item.configItem, _configItems[item.configItem].Value, item.value));
                    _configItems[item.configItem].Value = item.value;
                }

#if NET452
                _blockingCollection.Add(needModifyinglist);
#elif NET462_OR_GREATER || NET8_0_OR_GREATER
                _channel.Writer.WriteAsync(needModifyinglist).AsTask().GetAwaiter().GetResult();
#endif
                ValueSet?.Invoke(list.ToArray());
            }
        }

        private string Convert2String(object value)
        {
            var valueType = value.GetType();
            if (valueType == typeof(DateTime))
            {
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (valueType == typeof(System.Drawing.Color))
            {
                return "#" + ((System.Drawing.Color)value).ToArgb().ToString("X8", CultureInfo.InvariantCulture);
            }
            else if (valueType == typeof(FileInfo))
            {
                return ((FileInfo)value).FullName;
            }
            else if (valueType == typeof(DirectoryInfo))
            {
                return ((DirectoryInfo)value).FullName;
            }
            else
            {
                return value.ToString();
            }
        }

        #endregion Setter & Getter

        #region Virtual

        protected virtual string[] CustomizeOptionsSource(string configItem)
        {
            return null;
        }

        protected virtual Func<string, bool> CustomizeValidationRule(string configItem)
        {
            return null;
        }

        #endregion Virtual

        #region Validate

        private void ValidateValue(string configItem, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (string.IsNullOrEmpty(configItem))
            {
                throw new ArgumentException("Config item cannot be null or empty.", nameof(configItem));
            }
            if (!_configItems.ContainsKey(configItem))
            {
                throw new KeyNotFoundException($"Config item '{configItem}' not found.");
            }
            string valueString = Convert2String(value);
            ValueType valueType = _configItems[configItem].Type;
            string strValue = valueString.Trim();

            #region CS Data Type Validation

            decimal number = 0;

            if (valueType == ValueType.String)
            {
                ; // do nothing
            }
            else if (valueType == ValueType.Integer)
            {
                if (Utility.TryParse2Long(strValue, out long @long))
                {
                    number = @long;
                }
                else
                {
                    throw new InvalidCastException(ExceptionHelper.GetFormattedString("InvalidCastException_CannotConvert2Integer", strValue, configItem));
                }
            }
            else if (valueType == ValueType.Boolean)
            {
                if (!bool.TryParse(strValue, out _))
                {
                    throw new InvalidCastException(ExceptionHelper.GetFormattedString("InvalidCastException_CannotConvert2Boolean", strValue, configItem));
                }
            }
            else if (valueType == ValueType.Decimal)
            {
                if (!Utility.TryParse2Decimal(strValue, out number))
                {
                    throw new InvalidCastException(ExceptionHelper.GetFormattedString("InvalidCastException_CannotConvert2Decimal", strValue, configItem));
                }
            }
            else if (valueType == ValueType.File)
            {
                if (Utility.TryParse2File(strValue, out var _) == false)
                {
                    throw new InvalidCastException(ExceptionHelper.GetFormattedString("InvalidCastException_CannotConvert2Path", strValue, configItem));
                }
            }
            else if (valueType == ValueType.Folder)
            {
                if (Utility.TryParse2Directory(strValue, out var _) == false)
                {
                    throw new InvalidCastException(ExceptionHelper.GetFormattedString("InvalidCastException_CannotConvert2Path", strValue, configItem));
                }
            }
            else if (valueType == ValueType.DateTime)
            {
                if (!Utility.TryParse2DateTime(strValue, out _))
                {
                    throw new InvalidCastException(ExceptionHelper.GetFormattedString("InvalidCastException_CannotConvert2DateTime", strValue, configItem));
                }
            }
            else if (valueType == ValueType.Color)
            {
                if (!Utility.TryParse2Color(strValue, out _))
                {
                    throw new InvalidCastException(ExceptionHelper.GetFormattedString("InvalidCastException_CannotConvert2Color", strValue, configItem));
                }
            }
            else
            {
                throw new NotSupportedException($"Unsupported value type: {valueType}");
            }

            #endregion CS Data Type Validation

            #region Regular Expression Validation

            var regex = _configItems[configItem].Regex;
            var vtype = _configItems[configItem].Type;
            if (!string.IsNullOrWhiteSpace(regex))
            {
                if ((vtype == ValueType.String ||
                    vtype == ValueType.File ||
                    vtype == ValueType.Folder)
                    && !Regex.IsMatch(strValue, regex))
                {
                    throw new ArgumentException(ExceptionHelper.GetFormattedString("ArgumentException_RegexValidation", strValue, _configItems[configItem].RegexNote, configItem));
                }
                else if (vtype == ValueType.Integer || vtype == ValueType.Decimal)
                {
                    if (Utility.TryParse2Decimal(strValue, out number))
                    {
                        if (!Regex.IsMatch(number.ToString(CultureInfo.InvariantCulture), regex))
                        {
                            throw new ArgumentException(ExceptionHelper.GetFormattedString("ArgumentException_RegexValidation", number.ToString(CultureInfo.InvariantCulture), _configItems[configItem].RegexNote, configItem));
                        }
                    }
                }
            }

            #endregion Regular Expression Validation

            #region Options Validation

            var options = _configItems[configItem].Options;
            var type = _configItems[configItem].Type;
            if (options != null && options.Length > 0)
            {
                if (type == ValueType.String)
                {
                    if (!options.Contains(strValue))
                    {
                        throw new ArgumentOutOfRangeException(nameof(valueString), $"The value '{strValue}' is not in the options for config item '{configItem}'.");
                    }
                }
                else if (type == ValueType.Integer)
                {
                    var longOptions = new List<long>();
                    foreach (var option in options)
                    {
                        if (Utility.TryParse2Long(option, out long longValue))
                        {
                            longOptions.Add(longValue);
                        }
                        else
                        {
                            throw new ConfigException($"option '{option}' can't convert to a integer for '{configItem}'.");
                        }
                    }
                    Utility.TryParse2Long(strValue, out long @long); // 肯定返回true，因为前面已经调用此函数校验过字符串了。
                    if (!longOptions.Contains(@long))
                    {
                        throw new ArgumentOutOfRangeException(nameof(valueString), $"The value '{strValue}' is not in the options for config item '{configItem}'.");
                    }
                }
                else if (type == ValueType.Decimal)
                {
                    var decimalOptions = new List<decimal>();
                    foreach (var option in options)
                    {
                        if (Utility.TryParse2Decimal(option, out decimal decimalValue))
                        {
                            decimalOptions.Add(decimalValue);
                        }
                        else
                        {
                            throw new ConfigException($"option '{option}' can't convert to a decimal for '{configItem}'.");
                        }
                    }
                    Utility.TryParse2Decimal(strValue, out decimal @decimal); // 肯定返回true，因为前面已经调用此函数校验过字符串了。
                    if (!decimalOptions.Contains(@decimal))
                    {
                        throw new ArgumentOutOfRangeException(nameof(valueString), $"The value '{strValue}' is not in the options for config item '{configItem}'.");
                    }
                }
            }

            #endregion Options Validation

            #region Max & Min Validation

            if (valueType == ValueType.Integer || valueType == ValueType.Decimal)
            {
                if (number > _configItems[configItem].MaxValue || number < _configItems[configItem].MinValue)
                {
                    throw new ArgumentOutOfRangeException("", ExceptionHelper.GetFormattedString("ArgumentOutOfRangeException_MaxMin", strValue, configItem, _configItems[configItem].MinValue, _configItems[configItem].MaxValue));
                }
            }

            #endregion Max & Min Validation

            #region Customized Rule Validation

            var validationRule = CustomizeValidationRule(configItem);
            if (validationRule != null && !validationRule.Invoke(configItem))
            {
                throw new ArgumentException(ExceptionHelper.GetFormattedString("ArgumentException_CustomizeValidation", strValue, configItem));
            }

            #endregion Customized Rule Validation
        }

        private void ValidateValue((string configItem, object value)[] configValuePairs)
        {
            foreach (var pair in configValuePairs)
            {
                ValidateValue(pair.configItem, pair.value);
            }
        }

        #endregion Validate

        #region Save

        private string[] CheckConfigItemFormatting(string configItem)
        {
            if (configItem == null)
            {
                throw new ArgumentNullException(nameof(configItem));
            }
            configItem = configItem.Trim();
            if (string.IsNullOrWhiteSpace(configItem))
            {
                throw new ArgumentException("ConfigItem cannot be white space.", nameof(configItem));
            }
            var names = configItem.Split('.');
            if (configItem.StartsWith(".") || configItem.EndsWith(".") || names.Length < 2 || names.Any(x => string.IsNullOrWhiteSpace(x)))
            {
                throw new ArgumentException($"There must be at least one dot in the middle of the {configItem}, and it cannot appear at both ends, and it cannot appear consecutively", nameof(configItem));
            }
            return names;
        }

        private XmlNode Find(string configItem, XmlDocument document)
        {
            var names = CheckConfigItemFormatting(configItem);
            if (!_configItems.ContainsKey(configItem))
            {
                throw new ArgumentException($"{configItem} not found in XmlDocument.", nameof(configItem));
            }
            var xmlNodes = document.SelectNodes("root/config").OfType<XmlNode>().Where(x => x.Attributes["type"] != null && x.Attributes["value"] != null).ToList();
            var node = document.SelectSingleNode($"root/config[@name='{names[0]}']");
            for (var i = 1; i < names.Length; i++)
            {
                if (node == null)
                {
                    throw new ArgumentException($"{configItem} not found in XmlDocument.", nameof(configItem));
                }
                node = node.SelectSingleNode($"config[@name='{names[i]}']");
            }
            if (node == null || node.Attributes["type"] == null || node.Attributes["value"] == null) // 必须是数据节点
            {
                throw new ArgumentException($"{configItem} not found in XmlDocument.", nameof(configItem));
            }
            return node;
        }

        private string FormatXmlDocument(XmlDocument document)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriter(sb))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace,
                    OmitXmlDeclaration = true,
                    Encoding = _encoding ?? Encoding.UTF8
                };

                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    document.Save(xmlWriter);
                }
            }

            return sb.ToString();
        }

        private void ModifyValueInXmlDocument(XmlDocument document, params (string configItem, string valueString)[] configValuePairs)
        {
            Array.ForEach(configValuePairs, x => ModifyValueInXmlDocument(x.configItem, x.valueString, document));
        }

        private void ModifyValueInXmlDocument(string configItem, string valueString, XmlDocument document)
        {
            var target = Find(configItem, document);
            target.Attributes["value"].Value = valueString;
        }

        private void Save(string xmlDocument)
        {
            string fileFolder = Path.GetDirectoryName(_xmlDocumentPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_xmlDocumentPath);
            string extension = Path.GetExtension(_xmlDocumentPath);
            string bkFileName = fileNameWithoutExtension + ".bk" + extension;
            string bkFilePath = Path.Combine(fileFolder, bkFileName);
            string tmpFileName = fileNameWithoutExtension + ".tmp.bk" + extension;
            string tmpFilePath = Path.Combine(fileFolder, tmpFileName);
            File.WriteAllText(tmpFilePath, xmlDocument);
            File.Replace(tmpFilePath, _xmlDocumentPath, bkFilePath);
        }

        #endregion Save
    }
}