using System;

namespace NetWinI18n
{
    public static class Lang
    {
        /// <summary>
        /// 当前语言。
        /// </summary>
        public static string CurrentLanguage =>
            LocalizationManager.CurrentLanguage;

        /// <summary>
        /// 默认语言。
        /// </summary>
        public static string DefaultLanguage
        {
            get => LocalizationManager.DefaultLanguage;
            set => LocalizationManager.DefaultLanguage = value;
        }

        /// <summary>
        /// 语言切换事件。
        /// </summary>
        public static event EventHandler LanguageChanged
        {
            add => LocalizationManager.LanguageChanged += value;
            remove => LocalizationManager.LanguageChanged -= value;
        }

        /// <summary>
        /// 设置当前语言，例如 zh-CN、en-US、vi-VN。
        /// </summary>
        public static void SetLanguage(string language)
        {
            LocalizationManager.SetLanguage(language);
        }

        /// <summary>
        /// 设置外部 JSON 文件目录。
        /// 例如：Application.StartupPath/Languages。
        /// </summary>
        public static void SetLanguageDirectory(string directory)
        {
            LocalizationManager.SetLanguageDirectory(directory);
        }

        /// <summary>
        /// 根据 Key 获取翻译文本。
        /// 如果找不到翻译，返回 Key。
        /// </summary>
        public static string T(string key)
        {
            return LocalizationManager.GetString(key);
        }

        /// <summary>
        /// 获取带格式参数的翻译文本。
        /// 例如：Lang.T("Welcome.User", "Tom")。
        /// </summary>
        public static string T(string key, params object[] args)
        {
            string text = T(key);

            if (args == null || args.Length == 0)
                return text;

            try
            {
                return string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    text,
                    args);
            }
            catch (FormatException)
            {
                return text;
            }
        }
    }
}