using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SCADA.Configuration.WpfControls
{
    public class ObservableConfigNode : INotifyPropertyChanged
    {
        private bool _isEnable;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isVisible = true;
        private bool _xmlEnable;
        private bool _xmlVisible;

        public ObservableConfigNode()
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<ObservableConfigNode> Children { get; } = new List<ObservableConfigNode>();

        public List<ObservableConfigItem> ConfigItems { get; } = new List<ObservableConfigItem>();

        public string Display { get; set; }

        public bool IsEnable
        {
            get
            {
                if (XmlEnable == false)
                {
                    return false;
                }
                return _isEnable;
            }
            set
            {
                if (_isEnable != value)
                {
                    _isEnable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnable)));
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                }
            }
        }

        public bool IsLeaf => Children.Count == 0;

        public bool IsRoot => Parent == null;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public bool IsVisible
        {
            get
            {
                if (XmlVisible == false)
                {
                    return false;
                }
                return _isVisible;
            }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                }
            }
        }

        public string Name { get; set; }

        public ObservableConfigNode Parent { get; set; }

        public string Path => IsRoot ? Name : Parent.Path + "." + Name;

        public bool XmlEnable
        {
            get => _xmlEnable; set
            {
                if (_xmlEnable != value)
                {
                    _xmlEnable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnable)));
                }
            }
        }

        public bool XmlVisible
        {
            get => _xmlVisible; set
            {
                if (_xmlVisible != value)
                {
                    _xmlVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                }
            }
        }

        public static bool Find(string path, bool isTrailConfigItem, ObservableConfigNode node, out ObservableConfigItem configItem, out ObservableConfigNode configNode)
        {
            var names = path.Split('.');
            ObservableConfigNode result = null;
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

        public static bool Find(string path, bool isTrailConfigItem, IEnumerable<ObservableConfigNode> nodes, out ObservableConfigItem configItem, out ObservableConfigNode configNode)
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