using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SCADA.Configuration.WpfControls
{
    public class SetpointTextBoxBehavior : Behavior<TextBox>
    {
        // Using a DependencyProperty as the backing store for DirtyBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DirtyBrushProperty =
            DependencyProperty.Register("DirtyBrush", typeof(SolidColorBrush), typeof(SetpointTextBoxBehavior), new PropertyMetadata(Brushes.Silver));

        // Using a DependencyProperty as the backing store for ErrorBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorBrushProperty =
            DependencyProperty.Register("ErrorBrush", typeof(SolidColorBrush), typeof(SetpointTextBoxBehavior), new PropertyMetadata(Brushes.LightPink));

        public SetpointTextBoxBehavior()
        {
        }

        public SolidColorBrush DirtyBrush
        {
            get { return (SolidColorBrush)GetValue(DirtyBrushProperty); }
            set { SetValue(DirtyBrushProperty, value); }
        }

        public SolidColorBrush ErrorBrush
        {
            get { return (SolidColorBrush)GetValue(ErrorBrushProperty); }
            set { SetValue(ErrorBrushProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
            AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            ObservableConfigItem item = AssociatedObject.DataContext as ObservableConfigItem;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
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
                        view.PrimitiveConfigSource.SetValue(item.Path, textBox.Text.Trim());
                        item.Value = view.PrimitiveConfigSource.GetValue<string>(item.Path);
                        RestorePropertyValue(textBox, Control.BackgroundProperty);
                        RestorePropertyValue(textBox, FrameworkElement.ToolTipProperty);
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

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.IsInitialized)
            {
                ObservableConfigItem item = AssociatedObject.DataContext as ObservableConfigItem;
                try
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        RestorePropertyValue(textBox, Control.BackgroundProperty);
                        RestorePropertyValue(textBox, FrameworkElement.ToolTipProperty);
                        return;
                    }

                    item.ValidationRule.Invoke(textBox.Text.Trim());
                    SetPropertyCurrentValue(textBox, Control.BackgroundProperty, DirtyBrush);
                    RestorePropertyValue(textBox, FrameworkElement.ToolTipProperty);
                }
                catch (Exception ex)
                {
                    SetPropertyCurrentValue(textBox, Control.BackgroundProperty, ErrorBrush);
                    SetPropertyCurrentValue(textBox, FrameworkElement.ToolTipProperty, ex.Message);
                }
            }
        }

        private void RestorePropertyValue(DependencyObject dependencyObject, DependencyProperty property)
        {
            AssociatedObject?.InvalidateProperty(property);
            BindingOperations.GetBindingExpression(AssociatedObject, property)?.UpdateTarget();
        }

        private void SetPropertyCurrentValue(DependencyObject dependencyObject, DependencyProperty property, object value)
        {
            AssociatedObject?.SetCurrentValue(property, value);
        }
    }
}