using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SCADA.UIClient
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : UserControl
    {
        public LogViewer()
        {
            InitializeComponent();
        }



        public string Top1 { get; set; }
        public string Top2 { get; set; }
        public string Top3 { get; set; }

        public int MaxLogLines { get; set; } = 1000;

        public string DummyTop1 { get; set; } = "VceCluster V1.0";
        public string DummyTop2 { get; set; } = "Copyright Weiyun";
        public string DummyTop3 { get; set; } = "2024-06-10 10:00:00";
    }
}
