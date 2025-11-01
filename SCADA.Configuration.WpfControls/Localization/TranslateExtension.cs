using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace SCADA.Configuration.WpfControls.Localization
{
    [MarkupExtensionReturnType(typeof(string))]
    public class TranslateExtension : MarkupExtension
    {
        public TranslateExtension(string key)
        {
            Key = key;
        }

        // 绑定的资源键
        public string Key { get; set; }

        // 此方法返回最终要在UI上显示的值
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 创建一个绑定，将其连接到 LocalizationService 单例的索引器上
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationService.Instance,
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}