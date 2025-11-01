using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SCADA.Configuration.WpfControls
{
    internal class SetpointFolderPathButtonBehavior : Behavior<System.Windows.Controls.Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += AssociatedObject_Click;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= AssociatedObject_Click;
        }

        private void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            if (folderBrowserDialog.ShowDialog() == true)
            {
                var button = sender as System.Windows.Controls.Button;
                if (button != null)
                {
                    ObservableConfigItem item = button.DataContext as ObservableConfigItem;
                    if (item != null)
                    {
                        DependencyObject primitiveConfigSourceView = button;
                        while (true)
                        {
                            primitiveConfigSourceView = VisualTreeHelper.GetParent(primitiveConfigSourceView);
                            if (primitiveConfigSourceView is PrimitiveConfigSourceView || primitiveConfigSourceView == null)
                            {
                                break;
                            }
                        }
                        var view = (primitiveConfigSourceView as PrimitiveConfigSourceView);
                        if (view != null)
                        {
                            try
                            {
                                view.PrimitiveConfigSource.SetValue(item.Path, folderBrowserDialog.SelectedPath);
                                item.Value = view.PrimitiveConfigSource.GetValue<string>(item.Path);
                                if (item.Restart)
                                    MessageBox.Show("The application needs to be restarted to take effect.", item.Path, MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, item.Path, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}