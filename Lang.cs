using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;

namespace NetWinI18n
{
    public static class Lang
    {
        /// <summary>
        /// 当前语言。
        /// </summary>
        public static string CurrentLanguage
        {
            get { return LocalizationManager.CurrentLanguage; }
        }

        /// <summary>
        /// 默认语言。
        /// </summary>
        public static string DefaultLanguage
        {
            get { return LocalizationManager.DefaultLanguage; }
            set { LocalizationManager.DefaultLanguage = value; }
        }

        /// <summary>
        /// 语言切换事件。
        /// </summary>
        public static event EventHandler LanguageChanged
        {
            add { LocalizationManager.LanguageChanged += value; }
            remove { LocalizationManager.LanguageChanged -= value; }
        }

        /// <summary>
        /// 设置当前语言。
        /// </summary>
        public static void SetLanguage(string language)
        {
            LocalizationManager.SetLanguage(language);
        }

        /// <summary>
        /// 设置外部 JSON 文件目录。
        /// </summary>
        public static void SetLanguageDirectory(string directory)
        {
            LocalizationManager.SetLanguageDirectory(directory);
        }

        /// <summary>
        /// 根据 Key 获取翻译文本。
        /// </summary>
        public static string T(string key)
        {
            return LocalizationManager.GetString(key);
        }

        /// <summary>
        /// 获取带格式参数的翻译文本。
        /// </summary>
        public static string T(string key, params object[] args)
        {
            string text = T(key);

            if (args == null || args.Length == 0)
                return text;

            try
            {
                return string.Format(CultureInfo.CurrentCulture, text, args);
            }
            catch (FormatException)
            {
                return text;
            }
        }

        /// <summary>
        /// 初始化语言文件。
        /// 创建 Languages 目录，将 DLL 内嵌 JSON 复制到外部目录。
        /// 已存在的 JSON 不覆盖。
        /// 同时注册外部语言目录。
        /// </summary>
        public static void InitializeLanguageFiles()
        {
            string languageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");

            if (!Directory.Exists(languageDirectory))
                Directory.CreateDirectory(languageDirectory);

            Assembly assembly = typeof(Lang).Assembly;

            string[] resources = assembly.GetManifestResourceNames().Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && name.Contains(".Languages.")).ToArray();

            foreach (string resourceName in resources)
            {
                const string marker = ".Languages.";

                int index = resourceName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                    continue;

                string fileName = resourceName.Substring(index + marker.Length);

                string destinationPath = Path.Combine(languageDirectory, fileName);

                if (File.Exists(destinationPath))
                    continue;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        continue;

                    using (FileStream file = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.CopyTo(file);
                    }
                }
            }

            //自动设置语言目录
            LocalizationManager.SetLanguageDirectory(languageDirectory);
        }

        /// <summary>
        /// 刷新语言缓存。
        /// 修改外部 JSON 文件后调用。
        /// </summary>
        public static void RefreshLanguageCache()
        {
            LocalizationManager.RefreshLanguageCache();
        }

        /// <summary>
        /// 获取所有可用语言。
        /// </summary>
        public static List<string> GetAvailableLanguages()
        {
            return LocalizationManager.GetAvailableLanguages();
        }
    }
}