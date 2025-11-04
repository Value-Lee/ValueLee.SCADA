using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SCADA.Configuration.WpfControls.TestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, object> valueChangedCache = new Dictionary<string, object>();

        public MainWindow()
        {
            InitializeComponent();
            PrimitiveConfigSource = new PrimitiveConfigSource("system.xml", Encoding.UTF8);
        }

        public PrimitiveConfigSource PrimitiveConfigSource { get; set; }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            valueChangedCache.Clear();
            PrimitiveConfigSourceView.PrimitiveConfigSource.ValueSet -= PrimitiveConfigSource_ValueSet;
            PrimitiveConfigSourceView.PrimitiveConfigSource.Dispose();
            PrimitiveConfigSourceView.PrimitiveConfigSource = new PrimitiveConfigSource(PrimitiveConfigSource.XmlString);
            PrimitiveConfigSourceView.PrimitiveConfigSource.ValueSet += PrimitiveConfigSource_ValueSet;
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSource.SetValue(valueChangedCache.Select(x => (x.Key, x.Value)).ToArray());
            valueChangedCache.Clear();
        }

        private void HideDescColumn_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsDescriptionColumnVisible = true;
        }

        private void HideDescColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsDescriptionColumnVisible = false;
        }

        private void HideDescNoColumn_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsNoColumnVisible = true;
        }

        private void HideDescNoColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsNoColumnVisible = false;
        }

        private void HideExpandCollapse_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsExpandCollapseVisible = true;
        }

        private void HideExpandCollapse_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsExpandCollapseVisible = false;
        }

        private void HideMaxColumn_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsMaxColumnVisible = true;
        }

        private void HideMaxColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsMaxColumnVisible = false;
        }

        private void HideMinColumn_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsMinColumnVisible = true;
        }

        private void HideMinColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsMinColumnVisible = false;
        }

        private void HideSerachBox_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsSearchBoxVisible = true;
        }

        private void HideSerachBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsSearchBoxVisible = false;
        }

        private void HideTreeViewNavigation_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsTreeViewNavigationVisible = true;
        }

        private void HideTreeViewNavigation_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsTreeViewNavigationVisible = false;
        }

        private void HideUnitColumn_Checked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsUnitColumnVisible = true;
        }

        private void HideUnitColumn_Unchecked(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.IsUnitColumnVisible = false;
        }

        private void PrimitiveConfigSource_ValueSet((string configItem, string oldValue, string newValue)[] obj)
        {
            foreach (var item in obj)
            {
                if (valueChangedCache.ContainsKey(item.configItem))
                {
                    valueChangedCache[item.configItem] = item.newValue;
                }
                else
                {
                    valueChangedCache.Add(item.configItem, item.newValue);
                }
            }
        }

        private void Self_Loaded(object sender, RoutedEventArgs e)
        {
            PrimitiveConfigSourceView.PrimitiveConfigSource = new PrimitiveConfigSource(PrimitiveConfigSource.XmlString);
            PrimitiveConfigSourceView.PrimitiveConfigSource.ValueSet += PrimitiveConfigSource_ValueSet;
        }
    }
}