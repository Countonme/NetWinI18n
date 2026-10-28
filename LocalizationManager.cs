using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace NetWinI18n
{
    /// <summary>
    /// 多语言管理器，兼容 .NET Framework 4.5+ 和现代 .NET。
    /// 支持外部 JSON、内嵌 JSON、默认语言回退和动态切换。
    /// </summary>
    public static class LocalizationManager
    {
        private static readonly object SyncRoot = new object();

        private static readonly Dictionary<string, Dictionary<string, string>> Cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private static string _currentLanguage = "zh-CN";
        private static string _defaultLanguage = "zh-CN";
        private static string _languageDirectory = null;

        /// <summary>
        /// 语言切换事件。
        /// </summary>
        public static event EventHandler LanguageChanged;

        /// <summary>
        /// 当前语言。
        /// </summary>
        public static string CurrentLanguage
        {
            get
            {
                lock (SyncRoot)
                {
                    return _currentLanguage;
                }
            }
        }

        /// <summary>
        /// 默认语言。
        /// </summary>
        public static string DefaultLanguage
        {
            get
            {
                lock (SyncRoot)
                {
                    return _defaultLanguage;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("默认语言不能为空。", nameof(value));

                lock (SyncRoot)
                {
                    _defaultLanguage = value.Trim();
                }
            }
        }

        /// <summary>
        /// 设置外部语言文件目录。
        /// 外部 JSON 优先，内嵌 JSON 作为后备。
        /// </summary>
        public static void SetLanguageDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("语言目录不能为空。", nameof(directory));

            string fullPath = Path.GetFullPath(directory);

            lock (SyncRoot)
            {
                _languageDirectory = fullPath;
                Cache.Clear();
            }
        }

        /// <summary>
        /// 切换当前语言。
        /// </summary>
        public static void SetLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
                throw new ArgumentException("语言名称不能为空。", nameof(language));

            language = language.Trim();

            lock (SyncRoot)
            {
                if (string.Equals(_currentLanguage, language, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _currentLanguage = language;
            }

            OnLanguageChanged();
        }

        /// <summary>
        /// 触发语言切换事件。
        /// </summary>
        private static void OnLanguageChanged()
        {
            EventHandler handler = LanguageChanged;

            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 获取翻译文本。
        /// 未找到时回退到默认语言，仍未找到则返回 Key。
        /// </summary>
        public static string GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return key ?? string.Empty;

            string currentLanguage;
            string defaultLanguage;

            lock (SyncRoot)
            {
                currentLanguage = _currentLanguage;
                defaultLanguage = _defaultLanguage;
            }

            Dictionary<string, string> current = GetLanguageData(currentLanguage);

            string value;

            if (current.TryGetValue(key, out value))
                return value;
            if (!string.Equals(currentLanguage, defaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, string> fallback = GetLanguageData(defaultLanguage);

                if (fallback.TryGetValue(key, out value))
                    return value;
            }

            return key;
        }

        /// <summary>
        /// 获取指定语言的数据，带线程安全缓存。
        /// </summary>
        private static Dictionary<string, string> GetLanguageData(string language)
        {
            lock (SyncRoot)
            {
                Dictionary<string, string> cached;

                if (Cache.TryGetValue(language, out cached))
                    return cached;

                Dictionary<string, string> data = LoadLanguage(language);

                Cache[language] = data;

                return data;
            }
        }

        /// <summary>
        /// 加载语言文件。
        /// 优先读取外部 JSON，再读取程序集内嵌资源。
        /// </summary>
        private static Dictionary<string, string> LoadLanguage(string language)
        {
            string directory;

            lock (SyncRoot)
            {
                directory = _languageDirectory;
            }

            // 1. 外部 JSON
            if (!string.IsNullOrWhiteSpace(directory))
            {
                string filePath = Path.Combine(directory, language + ".json");

                if (File.Exists(filePath))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);

                        return Deserialize(json);
                    }
                    catch (JsonException)
                    {
                        // JSON 格式错误，尝试内嵌资源。
                    }
                    catch (IOException)
                    {
                        // 文件读取失败，尝试内嵌资源。
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // 没有访问权限，尝试内嵌资源。
                    }
                }
            }

            // 2. 程序集内嵌 JSON
            Assembly assembly = typeof(LocalizationManager).Assembly;

            string resourceName = "NetWinI18n.Languages." + language + ".json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return CreateEmptyDictionary();
                }

                try
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        return Deserialize(json);
                    }
                }
                catch (JsonException)
                {
                    return CreateEmptyDictionary();
                }
                catch (IOException)
                {
                    return CreateEmptyDictionary();
                }
            }
        }

        /// <summary>
        /// 解析 JSON，忽略 Key 大小写差异。
        /// </summary>
        private static Dictionary<string, string> Deserialize(string json)
        {
            Dictionary<string, string> source = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Dictionary<string, string> result = CreateEmptyDictionary();

            if (source == null)
                return result;

            foreach (KeyValuePair<string, string> item in source)
            {
                result[item.Key] = item.Value ?? string.Empty;
            }

            return result;
        }

        /// <summary>
        /// 创建忽略 Key 大小写的字典。
        /// </summary>
        private static Dictionary<string, string> CreateEmptyDictionary()
        {
            return new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取可用语言代码。
        /// 优先扫描外部 Languages 目录，同时读取程序集内嵌的 JSON 资源。
        /// </summary>
        public static List<string> GetAvailableLanguages()
        {
            HashSet<string> languages =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. 扫描外部 Languages 目录
            string directory = _languageDirectory;

            if (!string.IsNullOrWhiteSpace(directory) &&
                Directory.Exists(directory))
            {
                foreach (string file in Directory.GetFiles(
                    directory, "*.json", SearchOption.TopDirectoryOnly))
                {
                    string code = Path.GetFileNameWithoutExtension(file);

                    if (IsValidLanguageCode(code))
                        languages.Add(code);
                }
            }

            // 2. 扫描程序集内嵌资源
            Assembly assembly = typeof(LocalizationManager).Assembly;

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                const string prefix = "NetWinI18n.Languages.";

                if (!resourceName.StartsWith(
                    prefix, StringComparison.OrdinalIgnoreCase) ||
                    !resourceName.EndsWith(
                    ".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string code = resourceName.Substring(
                    prefix.Length,
                    resourceName.Length - prefix.Length - 5);

                if (IsValidLanguageCode(code))
                    languages.Add(code);
            }

            List<string> result = new List<string>(languages);
            result.Sort(StringComparer.OrdinalIgnoreCase);

            return result;
        }

        private static bool IsValidLanguageCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            try
            {
                CultureInfo.GetCultureInfo(code);
                return true;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }
    }
}