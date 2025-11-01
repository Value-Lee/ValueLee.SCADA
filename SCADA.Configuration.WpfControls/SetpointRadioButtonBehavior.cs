using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SCADA.Configuration.WpfControls
{
    internal class SetpointRadioButtonBehavior : Behavior<RadioButton>
    {
        public SetpointRadioButtonBehavior()
        {
            Value = false;
        }

        public bool Value { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Checked += AssociatedObject_Checked;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Checked -= AssociatedObject_Checked;
        }

        private void AssociatedObject_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton.IsInitialized)
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
                        view.PrimitiveConfigSource.SetValue(item.Path, Value);
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