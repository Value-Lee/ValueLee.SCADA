namespace ValueLee.Configuration
{
    using System.Reflection;
    using System.Resources;
    using System.Threading;

    public static class ExceptionHelper
    {
        private static readonly ResourceManager rm =
            new ResourceManager("ValueLee.Configuration.ExceptionMessages", Assembly.GetExecutingAssembly());

        // 新增：获取并格式化字符串的方法
        // 使用 params 关键字允许传入任意数量的参数
        public static string GetFormattedString(string name, params object[] args)
        {
            var format = GetString(name);

            // 如果资源文件中没有找到对应的键，format可能为null
            if (string.IsNullOrEmpty(format))
            {
                // 返回一个通用的错误信息，而不是让string.Format抛出异常
                return name;
            }

            return string.Format(format, args);
        }

        // 获取简单字符串的方法
        public static string GetString(string name)
        {
            // 使用当前线程的UI文化来获取字符串
            return rm.GetString(name, Thread.CurrentThread.CurrentUICulture);
        }
    }
}