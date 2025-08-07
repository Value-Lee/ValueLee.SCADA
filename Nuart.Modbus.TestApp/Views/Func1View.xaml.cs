using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Nuart.Modbus.TestApp.Views
{
    /// <summary>
    /// Interaction logic for Func1View.xaml
    /// </summary>
    public partial class Func1View : UserControl
    {
        public Func1View()
        {
            InitializeComponent();
        }

        private void CheckBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CheckBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
    }

    public class TextBoxValidationBehavior : Behavior<TextBox>
    {
        public TextBoxValidationBehavior()
        {
        }

        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            string backup = AssociatedObject.Text; // backup initial value
            AssociatedObject.Text = AssociatedObject.Text + " "; //have to modify the value
            AssociatedObject.Text = backup; // restore backup value to raise TextChanged event
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = AssociatedObject.Text;
            if (string.IsNullOrEmpty(text))
            {
                AssociatedObject?.SetCurrentValue(Control.BackgroundProperty, Brushes.LightPink);
            }
            text = new string(text.Trim().Where(x => !char.IsWhiteSpace(x)).ToArray());
            try
            {
                int value;
                if (text.StartsWith("0x") || text.StartsWith("0X"))
                {
                    text = text.Substring(2);
                    value = Convert.ToInt32(text, 16);
                }
                else
                {
                    value = Convert.ToInt32(text);
                }

                if (value < MinValue || value > MaxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }

                AssociatedObject?.InvalidateProperty(Control.BackgroundProperty); // 恢复原来的值
                BindingOperations.GetBindingExpression(AssociatedObject, Control.BackgroundProperty)?.UpdateTarget(); // 如果dp设置了绑定(d.SetBinding(dp,source))，在使用强制值期间源属性的值发生了变化，即使调用了InvalidateProperty()，dp不会被更新。此行代码可以强制读取数据源属性更新一下dp。
            }
            catch
            {
                AssociatedObject?.SetCurrentValue(Control.BackgroundProperty, Brushes.LightPink);
            }
        }
    }
}