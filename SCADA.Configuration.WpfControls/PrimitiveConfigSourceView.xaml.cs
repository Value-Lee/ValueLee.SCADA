using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SCADA.Configuration.WpfControls
{
    /// <summary>
    /// Interaction logic for PrimitiveConfigSourceView.xaml
    /// </summary>
    //[ContentProperty(nameof(HiddenItems))]
    public partial class PrimitiveConfigSourceView : UserControl
    {
        #region Properties

        // Using a DependencyProperty as the backing store for DataGridWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataGridWidthProperty =
            DependencyProperty.Register("DataGridWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(800d));

        // Using a DependencyProperty as the backing store for DescColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescColumnMaxWidthProperty =
            DependencyProperty.Register("DescColumnMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for DescColumnMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescColumnMinWidthProperty =
            DependencyProperty.Register("DescColumnMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(75d));

        // Using a DependencyProperty as the backing store for DisableItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisableItemsProperty =
            DependencyProperty.Register("DisableItems", typeof(ObservableCollection<string>), typeof(PrimitiveConfigSourceView), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for DisableNodes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisableNodesProperty =
            DependencyProperty.Register("DisableNodes", typeof(ObservableCollection<string>), typeof(PrimitiveConfigSourceView), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiddenItemsProperty =
            DependencyProperty.Register("HiddenItems", typeof(ObservableCollection<string>), typeof(PrimitiveConfigSourceView), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for HiddenNodes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiddenNodesProperty =
            DependencyProperty.Register("HiddenNodes", typeof(ObservableCollection<string>), typeof(PrimitiveConfigSourceView), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for IsDescriptionVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDescriptionColumnVisibleProperty =
            DependencyProperty.Register(nameof(IsDescriptionColumnVisible), typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsExpandCollapseVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandCollapseVisibleProperty =
            DependencyProperty.Register("IsExpandCollapseVisible", typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaxColumnProperty =
            DependencyProperty.Register(nameof(IsMaxColumnVisible), typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsMinColumnVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMinColumnVisibleProperty =
            DependencyProperty.Register(nameof(IsMinColumnVisible), typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsNoColumnVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNoColumnVisibleProperty =
            DependencyProperty.Register("IsNoColumnVisible", typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsSearchBoxVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSearchBoxVisibleProperty =
            DependencyProperty.Register("IsSearchBoxVisible", typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsTreeViewNavigationVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTreeViewNavigationVisibleProperty =
            DependencyProperty.Register("IsTreeViewNavigationVisible", typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for IsUnitVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUnitColumnVisibleProperty =
            DependencyProperty.Register(nameof(IsUnitColumnVisible), typeof(bool), typeof(PrimitiveConfigSourceView), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for MaxColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColumnMaxWidthProperty =
            DependencyProperty.Register("MaxColumnMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for MaxColumnMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxColumnMinWidthProperty =
            DependencyProperty.Register("MaxColumnMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(75d));

        // Using a DependencyProperty as the backing store for MinColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinColumnMaxWidthProperty =
            DependencyProperty.Register("MinColumnMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for MinColumnMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinColumnMinWidthProperty =
            DependencyProperty.Register("MinColumnMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(75d));

        // Using a DependencyProperty as the backing store for NameColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameColumnMaxWidthProperty =
            DependencyProperty.Register("NameColumnMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for NameColumnMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameColumnMinWidthProperty =
            DependencyProperty.Register("NameColumnMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(75d));

        // Using a DependencyProperty as the backing store for PrimitiveConfigSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimitiveConfigSourceProperty =
            DependencyProperty.Register(nameof(PrimitiveConfigSource), typeof(PrimitiveConfigSource), typeof(PrimitiveConfigSourceView), new PropertyMetadata(defaultValue: null, propertyChangedCallback: PrimitiveConfigSourceChangedCallback));

        // Using a DependencyProperty as the backing store for SetpointColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetpointColumnMaxWidthProperty =
            DependencyProperty.Register("SetpointColumnMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for SetpointColumnMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetpointColumnMinWidthProperty =
            DependencyProperty.Register("SetpointColumnMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(75d));

        // Using a DependencyProperty as the backing store for TextBoxTemplateDirtyBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxTemplateDirtyBrushProperty =
            DependencyProperty.Register("TextBoxTemplateDirtyBrush", typeof(SolidColorBrush), typeof(PrimitiveConfigSourceView), new PropertyMetadata(Brushes.Silver));

        // Using a DependencyProperty as the backing store for TextBoxTemplateErrorBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxTemplateErrorBrushProperty =
            DependencyProperty.Register("TextBoxTemplateErrorBrush", typeof(SolidColorBrush), typeof(PrimitiveConfigSourceView), new PropertyMetadata(Brushes.LightPink));

        // Using a DependencyProperty as the backing store for TreeViewNavigationMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TreeViewNavigationMaxWidthProperty =
            DependencyProperty.Register("TreeViewNavigationMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for TreeViewNavigationMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TreeViewNavigationMinWidthProperty =
            DependencyProperty.Register("TreeViewNavigationMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(150d));

        // Using a DependencyProperty as the backing store for UnitColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitColumnMaxWidthProperty =
            DependencyProperty.Register("UnitColumnMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for UnitColumnMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitColumnMinWidthProperty =
            DependencyProperty.Register("UnitColumnMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(75d));

        // Using a DependencyProperty as the backing store for ValueColumnMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueColumnMaxWidthProperty =
            DependencyProperty.Register("ValueColumnMaxWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(350d));

        // Using a DependencyProperty as the backing store for ValueColumnMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueColumnMinWidthProperty =
            DependencyProperty.Register("ValueColumnMinWidth", typeof(double), typeof(PrimitiveConfigSourceView), new PropertyMetadata(75d));

        // Using a DependencyProperty as the backing store for ObservableConfigNodes.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey ObservableConfigNodesProperty =
            DependencyProperty.RegisterReadOnly(nameof(ObservableConfigNodes), typeof(IList<ObservableConfigNode>), typeof(PrimitiveConfigSourceView), new PropertyMetadata(null));

        public double DataGridWidth
        {
            get { return (double)GetValue(DataGridWidthProperty); }
            set { SetValue(DataGridWidthProperty, value); }
        }

        public double DescColumnMaxWidth
        {
            get { return (double)GetValue(DescColumnMaxWidthProperty); }
            set { SetValue(DescColumnMaxWidthProperty, value); }
        }

        public double DescColumnMinWidth
        {
            get { return (double)GetValue(DescColumnMinWidthProperty); }
            set { SetValue(DescColumnMinWidthProperty, value); }
        }

        public ObservableCollection<string> DisableItems
        {
            get { return (ObservableCollection<string>)GetValue(DisableItemsProperty); }
            set { SetValue(DisableItemsProperty, value); }
        }

        public ObservableCollection<string> DisableNodes
        {
            get { return (ObservableCollection<string>)GetValue(DisableNodesProperty); }
            set { SetValue(DisableNodesProperty, value); }
        }

        public ObservableCollection<string> HiddenItems
        {
            get { return (ObservableCollection<string>)GetValue(HiddenItemsProperty); }
            set { SetValue(HiddenItemsProperty, value); }
        }

        public ObservableCollection<string> HiddenNodes
        {
            get { return (ObservableCollection<string>)GetValue(HiddenNodesProperty); }
            set { SetValue(HiddenNodesProperty, value); }
        }

        public bool IsDescriptionColumnVisible
        {
            get { return (bool)GetValue(IsDescriptionColumnVisibleProperty); }
            set { SetValue(IsDescriptionColumnVisibleProperty, value); }
        }

        public bool IsExpandCollapseVisible
        {
            get { return (bool)GetValue(IsExpandCollapseVisibleProperty); }
            set { SetValue(IsExpandCollapseVisibleProperty, value); }
        }

        public bool IsMaxColumnVisible
        {
            get { return (bool)GetValue(IsMaxColumnProperty); }
            set { SetValue(IsMaxColumnProperty, value); }
        }

        public bool IsMinColumnVisible
        {
            get { return (bool)GetValue(IsMinColumnVisibleProperty); }
            set { SetValue(IsMinColumnVisibleProperty, value); }
        }

        public bool IsNoColumnVisible
        {
            get { return (bool)GetValue(IsNoColumnVisibleProperty); }
            set { SetValue(IsNoColumnVisibleProperty, value); }
        }

        public bool IsSearchBoxVisible
        {
            get { return (bool)GetValue(IsSearchBoxVisibleProperty); }
            set { SetValue(IsSearchBoxVisibleProperty, value); }
        }

        public bool IsTreeViewNavigationVisible
        {
            get { return (bool)GetValue(IsTreeViewNavigationVisibleProperty); }
            set { SetValue(IsTreeViewNavigationVisibleProperty, value); }
        }

        public bool IsUnitColumnVisible
        {
            get { return (bool)GetValue(IsUnitColumnVisibleProperty); }
            set { SetValue(IsUnitColumnVisibleProperty, value); }
        }

        public double MaxColumnMaxWidth
        {
            get { return (double)GetValue(MaxColumnMaxWidthProperty); }
            set { SetValue(MaxColumnMaxWidthProperty, value); }
        }

        public double MaxColumnMinWidth
        {
            get { return (double)GetValue(MaxColumnMinWidthProperty); }
            set { SetValue(MaxColumnMinWidthProperty, value); }
        }

        public double MinColumnMaxWidth
        {
            get { return (double)GetValue(MinColumnMaxWidthProperty); }
            set { SetValue(MinColumnMaxWidthProperty, value); }
        }

        public double MinColumnMinWidth
        {
            get { return (double)GetValue(MinColumnMinWidthProperty); }
            set { SetValue(MinColumnMinWidthProperty, value); }
        }

        public double NameColumnMaxWidth
        {
            get { return (double)GetValue(NameColumnMaxWidthProperty); }
            set { SetValue(NameColumnMaxWidthProperty, value); }
        }

        public double NameColumnMinWidth
        {
            get { return (double)GetValue(NameColumnMinWidthProperty); }
            set { SetValue(NameColumnMinWidthProperty, value); }
        }

        public PrimitiveConfigSource PrimitiveConfigSource
        {
            get { return (PrimitiveConfigSource)GetValue(PrimitiveConfigSourceProperty); }
            set { SetValue(PrimitiveConfigSourceProperty, value); }
        }

        public double SetpointColumnMaxWidth
        {
            get { return (double)GetValue(SetpointColumnMaxWidthProperty); }
            set { SetValue(SetpointColumnMaxWidthProperty, value); }
        }

        public double SetpointColumnMinWidth
        {
            get { return (double)GetValue(SetpointColumnMinWidthProperty); }
            set { SetValue(SetpointColumnMinWidthProperty, value); }
        }

        public SolidColorBrush TextBoxTemplateDirtyBrush
        {
            get { return (SolidColorBrush)GetValue(TextBoxTemplateDirtyBrushProperty); }
            set { SetValue(TextBoxTemplateDirtyBrushProperty, value); }
        }

        public SolidColorBrush TextBoxTemplateErrorBrush
        {
            get { return (SolidColorBrush)GetValue(TextBoxTemplateErrorBrushProperty); }
            set { SetValue(TextBoxTemplateErrorBrushProperty, value); }
        }

        public double TreeViewNavigationMaxWidth
        {
            get { return (double)GetValue(TreeViewNavigationMaxWidthProperty); }
            set { SetValue(TreeViewNavigationMaxWidthProperty, value); }
        }

        public double TreeViewNavigationMinWidth
        {
            get { return (double)GetValue(TreeViewNavigationMinWidthProperty); }
            set { SetValue(TreeViewNavigationMinWidthProperty, value); }
        }

        public double UnitColumnMaxWidth
        {
            get { return (double)GetValue(UnitColumnMaxWidthProperty); }
            set { SetValue(UnitColumnMaxWidthProperty, value); }
        }

        public double UnitColumnMinWidth
        {
            get { return (double)GetValue(UnitColumnMinWidthProperty); }
            set { SetValue(UnitColumnMinWidthProperty, value); }
        }

        public double ValueColumnMaxWidth
        {
            get { return (double)GetValue(ValueColumnMaxWidthProperty); }
            set { SetValue(ValueColumnMaxWidthProperty, value); }
        }

        public double ValueColumnMinWidth
        {
            get { return (double)GetValue(ValueColumnMinWidthProperty); }
            set { SetValue(ValueColumnMinWidthProperty, value); }
        }

        private IList<ObservableConfigNode> ObservableConfigNodes
        {
            get { return (IList<ObservableConfigNode>)GetValue(ObservableConfigNodesProperty.DependencyProperty); }
        }

        #endregion Properties

        public PrimitiveConfigSourceView()
        {
            InitializeComponent();
            var hiddenItems = new ObservableCollection<string>();
            var hiddenNodes = new ObservableCollection<string>();
            var disableItems = new ObservableCollection<string>();
            var disableNodes = new ObservableCollection<string>();
            disableItems.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null && e.NewItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.NewItems)
                        {
                            ret = ObservableConfigNode.Find(item, true, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret)
                                oconfigItem.XmlEnable = false;
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    if (e.OldItems != null && e.OldItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.OldItems)
                        {
                            ret = ObservableConfigNode.Find(item, true, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret == true)
                                RestoreItemXmlEnable(oconfigItem);
                        }
                    }
                }
            };
            disableNodes.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null && e.NewItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.NewItems)
                        {
                            ret = ObservableConfigNode.Find(item, false, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret)
                            {
                                oconfigNode.XmlEnable = false;
                                AdjustXmlEnable(oconfigNode);
                            }
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    if (e.OldItems != null && e.OldItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.OldItems)
                        {
                            ret = ObservableConfigNode.Find(item, false, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret)
                            {
                                RestoreNodeXmlEnable(oconfigNode);
                                AdjustXmlEnable(oconfigNode);
                            }
                        }
                    }
                }
            };
            hiddenItems.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null && e.NewItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.NewItems)
                        {
                            ret = ObservableConfigNode.Find(item, true, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret)
                                oconfigItem.XmlVisible = false;
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    if (e.OldItems != null && e.OldItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.OldItems)
                        {
                            ret = ObservableConfigNode.Find(item, true, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret == true)
                                RestoreItemXmlVisible(oconfigItem);
                        }
                    }
                }
            };
            hiddenNodes.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null && e.NewItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.NewItems)
                        {
                            ret = ObservableConfigNode.Find(item, false, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret)
                            {
                                oconfigNode.XmlVisible = false;
                                AdjustXmlVisible(oconfigNode);
                            }
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    if (e.OldItems != null && e.OldItems.Count > 0 && ObservableConfigNodes != null)
                    {
                        bool ret = false;
                        ObservableConfigItem oconfigItem = null;
                        ObservableConfigNode oconfigNode = null;
                        foreach (string item in e.OldItems)
                        {
                            ret = ObservableConfigNode.Find(item, false, ObservableConfigNodes, out oconfigItem, out oconfigNode);
                            if (ret)
                            {
                                RestoreNodeXmlVisible(oconfigNode);
                                AdjustXmlVisible(oconfigNode);
                            }
                        }
                    }
                }
            };
            HiddenItems = hiddenItems;
            HiddenNodes = hiddenNodes;
            DisableItems = disableItems;
            DisableNodes = disableNodes;
        }

        public void Set(ObservableConfigItem observableConfigItem)
        {
            try
            {
                string configItem = FindHost(observableConfigItem, ObservableConfigNodes).Path + "." + observableConfigItem.Name;
                PrimitiveConfigSource.SetValue(configItem, observableConfigItem.Setpoint.Trim());
                observableConfigItem.Value = PrimitiveConfigSource.GetValue<string>(configItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void PrimitiveConfigSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = e.NewValue as PrimitiveConfigSource;
            var sourceView = d as PrimitiveConfigSourceView;
            if (source == null)
            {
                sourceView.SetValue(ObservableConfigNodesProperty, null);
                return;
            }
            List<ObservableConfigNode> observableConfigNodes = new List<ObservableConfigNode>();
            foreach (var item in source.Nodes)
            {
                observableConfigNodes.Add(sourceView.Rebuild(item, null));
            }
            sourceView.SetValue(ObservableConfigNodesProperty, observableConfigNodes);

            if (sourceView.HiddenItems != null && sourceView.HiddenItems.Count > 0 && sourceView.ObservableConfigNodes != null)
            {
                foreach (var item in sourceView.HiddenItems)
                {
                    bool ret = false;
                    ObservableConfigItem oconfigItem = null;
                    ObservableConfigNode oconfigNode = null;
                    ret = ObservableConfigNode.Find(item, true, sourceView.ObservableConfigNodes, out oconfigItem, out oconfigNode);
                    if (ret)
                        oconfigItem.XmlVisible = false;
                }
            }
            if (sourceView.HiddenNodes != null && sourceView.HiddenNodes.Count > 0 && sourceView.ObservableConfigNodes != null)
            {
                bool ret = false;
                ObservableConfigItem oconfigItem = null;
                ObservableConfigNode oconfigNode = null;
                foreach (var item in sourceView.HiddenNodes)
                {
                    ret = ObservableConfigNode.Find(item, false, sourceView.ObservableConfigNodes, out oconfigItem, out oconfigNode);
                    if (ret)
                    {
                        oconfigNode.XmlVisible = false;
                        sourceView.AdjustXmlVisible(oconfigNode);
                    }
                }
            }
            if (sourceView.DisableItems != null && sourceView.DisableItems.Count > 0 && sourceView.ObservableConfigNodes != null)
            {
                foreach (var item in sourceView.DisableItems)
                {
                    bool ret = false;
                    ObservableConfigItem oconfigItem = null;
                    ObservableConfigNode oconfigNode = null;
                    ret = ObservableConfigNode.Find(item, true, sourceView.ObservableConfigNodes, out oconfigItem, out oconfigNode);
                    if (ret)
                        oconfigItem.XmlEnable = false;
                }
            }
            if (sourceView.DisableNodes != null && sourceView.DisableNodes.Count > 0 && sourceView.ObservableConfigNodes != null)
            {
                bool ret = false;
                ObservableConfigItem oconfigItem = null;
                ObservableConfigNode oconfigNode = null;
                foreach (var item in sourceView.DisableNodes)
                {
                    ret = ObservableConfigNode.Find(item, false, sourceView.ObservableConfigNodes, out oconfigItem, out oconfigNode);
                    if (ret)
                    {
                        oconfigNode.XmlEnable = false;
                        sourceView.AdjustXmlEnable(oconfigNode);
                    }
                }
            }
            sourceView.AdjustXmlVisible();
            sourceView.AdjustXmlEnable();
            if (observableConfigNodes.Count > 0)
            {
                var visibleNode = observableConfigNodes.Find(x => x.IsVisible);
                if (visibleNode != null)
                    visibleNode.IsSelected = true;
            }
        }

        private void AdjustXmlEnable(ObservableConfigNode node)
        {
            if (node.XmlEnable == false)
            {
                node.ConfigItems.ForEach(x => x.XmlEnable = false);
                node.Children.ForEach(x => x.XmlEnable = false);
                node.Children.ForEach(x => AdjustXmlEnable(x));
            }
            else
            {
                node.Children.ForEach(x => AdjustXmlEnable(x));
            }
        }

        private void AdjustXmlEnable()
        {
            if (this.ObservableConfigNodes == null || ObservableConfigNodes.Count == 0)
            {
                return;
            }
            foreach (var node in ObservableConfigNodes)
            {
                AdjustXmlEnable(node);
            }
        }

        private void AdjustXmlVisible(ObservableConfigNode node)
        {
            if (node.XmlVisible == false)
            {
                node.ConfigItems.ForEach(x => x.XmlVisible = false);
                node.Children.ForEach(x => x.XmlVisible = false);
                node.Children.ForEach(x => AdjustXmlVisible(x));
            }
            else
            {
                node.Children.ForEach(x => AdjustXmlVisible(x));
            }
        }

        private void AdjustXmlVisible()
        {
            if (ObservableConfigNodes == null || ObservableConfigNodes.Count == 0)
            {
                return;
            }
            foreach (var node in ObservableConfigNodes)
            {
                AdjustXmlVisible(node);
            }
        }

        private void BtnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TreeView.Items)
            {
                DependencyObject dObject = TreeView.ItemContainerGenerator.ContainerFromItem(item);
                CollapseTreeviewItems(((TreeViewItem)dObject), false);
            }
        }

        private void BtnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TreeView.Items)
            {
                DependencyObject dObject = TreeView.ItemContainerGenerator.ContainerFromItem(item);
                CollapseTreeviewItems(((TreeViewItem)dObject), true);
            }
        }

        private void CollapseTreeviewItems(TreeViewItem Item, bool status)
        {
            Item.IsExpanded = status;

            foreach (var item in Item.Items)
            {
                DependencyObject dObject = Item.ItemContainerGenerator.ContainerFromItem(item);

                if (dObject != null)
                {
                    ((TreeViewItem)dObject).IsExpanded = status;

                    if (((TreeViewItem)dObject).HasItems)
                    {
                        CollapseTreeviewItems(((TreeViewItem)dObject), status);
                    }
                }
            }
        }

        private ObservableConfigNode FindHost(ObservableConfigItem target, IList<ObservableConfigNode> observableConfigNodes)
        {
            if (observableConfigNodes == null)
            {
                return null;
            }
            foreach (ObservableConfigNode node in observableConfigNodes)
            {
                foreach (var item in node.ConfigItems)
                {
                    if (item == target)
                    {
                        return node;
                    }
                }
                if (node.Children != null && node.Children.Count > 0)
                {
                    return FindHost(target, node.Children);
                }
            }
            return null;
        }

        private void ImageClearFilterText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TbxFilter.Clear();
        }

        private ObservableConfigNode Rebuild(ConfigNode configNode, ObservableConfigNode parent)
        {
            if (configNode == null)
            {
                return null;
            }
            ObservableConfigNode ocn = new ObservableConfigNode();
            ocn.Name = configNode.Name;
            ocn.Display = configNode.Display;
            ocn.XmlVisible = configNode.Visible;
            ocn.IsVisible = configNode.Visible;
            ocn.XmlEnable = configNode.Enable;
            ocn.IsEnable = configNode.Enable;
            ocn.Parent = parent;
            configNode.ConfigItems.ForEach(x => ocn.ConfigItems.Add(new ObservableConfigItem()
            {
                Name = x.Name,
                Value = x.Value,
                Max = x.MaxValue,
                Min = x.MinValue,
                Display = x.Display,
                Description = x.Description,
                Restart = x.Restart,
                Type = x.Type,
                Options = x.Options,
                Unit = x.Unit,
                ValidationRule = x.ValidationRule,
                XmlVisible = x.Visible,
                IsVisible = x.Visible,
                IsEnable = x.Enable,
                XmlEnable = x.Enable,
                Setpoint = string.Empty,
                Path = ocn.Path + "." + x.Name,
            }));

            if (configNode.Children != null && configNode.Children.Count > 0)
            {
                foreach (var child in configNode.Children)
                {
                    ocn.Children.Add(Rebuild(child, ocn));
                }
            }
            return ocn;
        }

        private void RestoreItemXmlEnable(ObservableConfigItem item)
        {
            var ret = ConfigNode.Find(item.Path, true, this.PrimitiveConfigSource.Nodes, out var configItem, out var configNode);
            if (ret)
            {
                item.XmlEnable = configItem.Enable;
            }
        }

        private void RestoreItemXmlVisible(ObservableConfigItem item)
        {
            var ret = ConfigNode.Find(item.Path, true, this.PrimitiveConfigSource.Nodes, out var configItem, out var configNode);
            if (ret)
            {
                item.XmlVisible = configItem.Visible;
            }
        }

        private void RestoreNodeXmlEnable(ObservableConfigNode node)
        {
            var ret = ConfigNode.Find(node.Path, false, PrimitiveConfigSource.Nodes, out var configItem, out var configNode);
            if (ret)
            {
                node.XmlEnable = configNode.Enable;
                node.ConfigItems.ForEach(x => x.XmlEnable = configNode.ConfigItems.First(y => y.Name == x.Name).Enable);
                node.Children.ForEach(x => x.XmlEnable = configNode.Children.First(y => y.Name == x.Name).Enable);
                node.Children.ForEach(x => RestoreNodeXmlEnable(x));
            }
        }

        private void RestoreNodeXmlVisible(ObservableConfigNode node)
        {
            var ret = ConfigNode.Find(node.Path, false, this.PrimitiveConfigSource.Nodes, out var configItem, out var configNode);
            if (ret)
            {
                node.XmlVisible = configNode.Visible;
                node.ConfigItems.ForEach(x => x.XmlVisible = configNode.ConfigItems.First(y => y.Name == x.Name).Visible);
                node.Children.ForEach(x => x.XmlVisible = configNode.Children.First(y => y.Name == x.Name).Visible);
                node.Children.ForEach(x => RestoreNodeXmlVisible(x));
            }
        }

        #region Filter

        public void DisplayAllNodes(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText))
            {
                SetVisibility(ObservableConfigNodes, true);
            }
        }

        public void Filter(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText))
            {
                SetVisibility(ObservableConfigNodes, true);
                return;
            }

            filterText = filterText.Trim();

            foreach (var item in ObservableConfigNodes)
            {
                ApplyFilterRecursive(item, filterText);
            }

            bool ApplyFilterRecursive(ObservableConfigNode node, string filter)
            {
                bool selfMatches;
                if (node.XmlVisible == false)
                {
                    selfMatches = false;
                }
                else
                {
                    selfMatches = node.Display.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || node.ConfigItems.Where(y => y.XmlVisible).Any(x => x.Display.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // 如果有子节点，先对子节点进行过滤
                bool anyChildMatches = false;
                if (node.Children != null && node.Children.Any())
                {
                    foreach (var child in node.Children)
                    {
                        // 如果任何一个子节点匹配，就将 anyChildMatches 设为 true
                        if (ApplyFilterRecursive(child, filter))
                        {
                            anyChildMatches = true;
                        }
                    }
                }

                // 决定当前节点是否可见
                bool isVisible = selfMatches || anyChildMatches;
                node.IsVisible = isVisible;

                // 如果是因其子节点匹配而可见，则应展开该节点以显示匹配的子项
                if (anyChildMatches && !selfMatches)
                {
                    node.IsExpanded = true;
                }
                else
                {
                    // 如果节点本身匹配，可以根据需求决定是否展开
                    // 这里我们选择不改变它的展开状态，除非子项匹配
                }

                // 返回当前节点或其任何子节点是否匹配
                return isVisible;
            }
        }

        private void SetVisibility(IEnumerable<ObservableConfigNode> nodes, bool isVisible)
        {
            foreach (var node in nodes)
            {
                node.IsVisible = isVisible;
                if (node.Children != null && node.Children.Count > 0)
                {
                    SetVisibility(node.Children, isVisible);
                }
            }
        }

        #endregion Filter
    }
}