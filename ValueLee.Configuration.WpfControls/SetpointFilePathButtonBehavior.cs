using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media; // Assuming Ookii.Dialogs.Wpf is used for file dialogs

namespace ValueLee.Configuration.WpfControls
{
    internal class SetpointFilePathButtonBehavior:Behavior<System.Windows.Controls.Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += AssociatedObject_Click;
        }

        private void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaSaveFileDialog saveFileDialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                RestoreDirectory = true,
            };
            if (saveFileDialog.ShowDialog() == true)
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
                                view.PrimitiveConfigSource.SetValue(item.Path, saveFileDialog.FileName);
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

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= AssociatedObject_Click;
        }
    }
}
