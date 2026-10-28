using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetWinI18n
{
    public static class Lang
    {
        /// <summary>
        /// 当前语言。
        /// </summary>
        public static string CurrentLanguage => LocalizationManager.CurrentLanguage;

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

        /// <summary>
        /// 初始化外部语言文件。
        /// 将 DLL 内嵌的 JSON 文件复制到程序目录 Languages 文件夹。
        /// 已存在的文件不会被覆盖。
        /// </summary>
        public static void InitializeLanguageFiles()
        {
            string languageDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Languages");

            if (!Directory.Exists(languageDirectory))
            {
                Directory.CreateDirectory(languageDirectory);
            }

            Assembly assembly = typeof(Lang).Assembly;

            string[] resources = assembly.GetManifestResourceNames()
                .Where(name =>
                    name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                    name.Contains(".Languages."))
                .ToArray();

            foreach (string resourceName in resources)
            {
                string fileName = resourceName.Substring(
                    resourceName.LastIndexOf('.') - 0);

                // 从资源名中提取语言代码，例如 zh-CN.json。
                // 假设资源名格式为 NetWinI18n.Languages.zh-CN.json。
                const string marker = ".Languages.";
                int index = resourceName.IndexOf(
                    marker,
                    StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                    continue;

                fileName = resourceName.Substring(index + marker.Length);

                string destinationPath = Path.Combine(
                    languageDirectory,
                    fileName);

                if (File.Exists(destinationPath))
                    continue;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        continue;

                    using (FileStream file = new FileStream(
                        destinationPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
                    {
                        stream.CopyTo(file);
                    }
                }
            }
        }
    }
}