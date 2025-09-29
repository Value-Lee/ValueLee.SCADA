using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ValueLee.Configuration.WpfControls.Localization
{
    public class LocalizationService : INotifyPropertyChanged
    {
        #region Singleton
        private static readonly LocalizationService _instance = new LocalizationService();
        public static LocalizationService Instance => _instance; 
        #endregion

        private CultureInfo _currentCulture;

        private ResourceManager rm;

        private LocalizationService()
        {
            _currentCulture = Thread.CurrentThread.CurrentUICulture;
            rm = new ResourceManager("ValueLee.Configuration.WpfControls.localization.Strings", Assembly.GetExecutingAssembly());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    // 切换线程的语言，影响如日期、货币格式
                    Thread.CurrentThread.CurrentCulture = _currentCulture;
                    Thread.CurrentThread.CurrentUICulture = _currentCulture;

                    // 通知UI更新
                    OnPropertyChanged(nameof(CurrentCulture));
                    // 使用特殊名称通知所有绑定到索引器[]的属性更新
                    OnPropertyChanged("Item[]");
                }
            }
        }

        public string this[string key] => rm.GetString(key, CurrentCulture);

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}