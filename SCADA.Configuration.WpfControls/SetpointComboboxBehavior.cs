using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SCADA.Configuration.WpfControls
{
    internal class SetpointComboboxBehavior : Behavior<ComboBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combox = sender as ComboBox;
            if (combox.IsInitialized)
            {
                ObservableConfigItem item = AssociatedObject.DataContext as ObservableConfigItem;
                DependencyObject primitiveConfigSourceView = AssociatedObject;
                
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
                            view.PrimitiveConfigSource.SetValue(item.Path, combox.SelectedItem);
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