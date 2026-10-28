using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace NetWinI18n.Forms
{
    /// <summary>
    /// WinForms language settings window.
    /// Automatically discovers language JSON files in the Languages directory.
    /// </summary>
    public class LanguageSettingsForm : Form
    {
        private readonly ListBox _languageList;
        private readonly Label _titleLabel;
        private readonly Label _descriptionLabel;
        private readonly Label _currentLanguageLabel;
        private readonly Button _applyButton;
        private readonly Button _cancelButton;

        private readonly List<LanguageItem> _languages = new List<LanguageItem>();

        private readonly string _settingsFile;

        /// <summary>
        /// Gets the language selected by the user.
        /// </summary>
        public string SelectedLanguage { get; private set; }

        /// <summary>
        /// Directory containing language JSON files.
        /// Defaults to the Languages directory beside the application.
        /// </summary>
        public static string LanguageDirectory
        {
            get
            {
                return Path.Combine(Application.StartupPath, "Languages");
            }
        }

        /// <summary>
        /// Initializes the language settings window.
        /// </summary>
        public LanguageSettingsForm()
        {
            Lang.InitializeLanguageFiles();
            _settingsFile = Path.Combine(Application.LocalUserAppDataPath, "language.txt");

            Text = "语言设置 / Language Settings";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(510, 360);
            Font = new Font("Microsoft YaHei UI", 9F);
            BackColor = Color.FromArgb(243, 247, 251);

            // Header
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Color.FromArgb(18, 43, 73)
            };

            _titleLabel = new Label
            {
                Text = "语言设置 / Language Settings",
                ForeColor = Color.White,
                Font = new Font(
                    "Microsoft YaHei UI",
                    14F,
                    FontStyle.Bold),
                Location = new Point(22, 12),
                AutoSize = true
            };

            _descriptionLabel = new Label
            {
                Text = "选择你希望使用的界面语言",
                ForeColor = Color.FromArgb(190, 211, 232),
                Location = new Point(24, 45),
                AutoSize = true
            };

            header.Controls.Add(_titleLabel);
            header.Controls.Add(_descriptionLabel);

            // Available languages title
            Label listTitle = new Label
            {
                Text = "可用语言 / Available Languages",
                Location = new Point(24, 94),
                AutoSize = true,
                ForeColor = Color.FromArgb(45, 64, 84),
                Font = new Font(
                    "Microsoft YaHei UI",
                    9F,
                    FontStyle.Bold)
            };

            // Language list
            _languageList = new ListBox
            {
                Location = new Point(24, 120),
                Size = new Size(462, 145),
                Font = new Font("Microsoft YaHei UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 34,
                BackColor = Color.White
            };

            _languageList.DrawItem += LanguageList_DrawItem;
            _languageList.SelectedIndexChanged +=
                LanguageList_SelectedIndexChanged;

            _currentLanguageLabel = new Label
            {
                Location = new Point(25, 274),
                Size = new Size(460, 22),
                ForeColor = Color.FromArgb(65, 91, 117),
                Text = "当前语言 / Current language"
            };

            // Apply button
            _applyButton = new Button
            {
                Text = "应用 / Apply",
                Size = new Size(120, 34),
                Location = new Point(235, 310),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(22, 121, 207),
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };

            _applyButton.FlatAppearance.BorderSize = 0;
            _applyButton.Click += ApplyButton_Click;

            // Cancel button
            _cancelButton = new Button
            {
                Text = "取消 / Cancel",
                Size = new Size(120, 34),
                Location = new Point(366, 310),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(45, 64, 84)
            };

            _cancelButton.Click += CancelButton_Click;

            Controls.Add(header);
            Controls.Add(listTitle);
            Controls.Add(_languageList);
            Controls.Add(_currentLanguageLabel);
            Controls.Add(_applyButton);
            Controls.Add(_cancelButton);

            AcceptButton = _applyButton;
            CancelButton = _cancelButton;

            // Load available languages after initializing controls.
            LoadLanguages();

            foreach (LanguageItem item in _languages)
            {
                _languageList.Items.Add(item);
            }

            _applyButton.Enabled = _languageList.Items.Count > 0;

            SelectCurrentLanguage();
        }

        /// <summary>
        /// Scans the Languages directory for valid language JSON files.
        /// The file name without extension is used as the language code.
        /// Example: Languages/ja-JP.json.
        /// </summary>
        private void LoadLanguages()
        {
            _languages.Clear();

            string directory = LanguageDirectory;

            if (!Directory.Exists(directory))
                return;

            string[] files;

            try
            {
                files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "无法读取语言目录：\r\n" + ex.Message, "语言设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            foreach (string file in files)
            {
                string code = Path.GetFileNameWithoutExtension(file);

                if (string.IsNullOrWhiteSpace(code))
                    continue;

                try
                {
                    // Validate the language code.
                    CultureInfo culture = CultureInfo.GetCultureInfo(code);

                    // Validate that the file contains valid JSON.
                    string json = File.ReadAllText(file, Encoding.UTF8);

                    JObject.Parse(json);

                    _languages.Add(new LanguageItem(code, culture.NativeName, culture.EnglishName));
                }
                catch (CultureNotFoundException)
                {
                    // Ignore files whose names are not valid culture codes.
                }
                catch (Exception)
                {
                    // Ignore malformed or unreadable language files.
                }
            }

            _languages.Sort(delegate (LanguageItem a, LanguageItem b)
            {
                return string.Compare(a.NativeName, b.NativeName, StringComparison.CurrentCulture);
            });
        }

        /// <summary>
        /// Selects the currently active language, if available.
        /// </summary>
        private void SelectCurrentLanguage()
        {
            string current = Lang.CurrentLanguage;

            for (int i = 0; i < _languageList.Items.Count; i++)
            {
                LanguageItem item = _languageList.Items[i] as LanguageItem;

                if (item != null &&
                    string.Equals(item.Code, current, StringComparison.OrdinalIgnoreCase))
                {
                    _languageList.SelectedIndex = i;
                    return;
                }
            }

            if (_languageList.Items.Count > 0)
            {
                _languageList.SelectedIndex = 0;
            }
            else
            {
                _currentLanguageLabel.Text = "未找到语言文件，请检查 Languages 目录。";
            }
        }

        private void LanguageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LanguageItem item = _languageList.SelectedItem as LanguageItem;

            if (item == null)
                return;

            _currentLanguageLabel.Text = "选择语言 / Selected: " + item.NativeName + " (" + item.Code + ")";

            // 切换当前语言
            Lang.SetLanguage(item.Code);

            // 刷新语言设置窗体
            LanguageHelper.Apply(this);

            // 自动获取并刷新其他已打开的窗体
            foreach (Form form in Application.OpenForms)
            {
                if (form != this && !(form is LanguageSettingsForm))
                {
                    LanguageHelper.Apply(form);
                }
            }

            // 保存语言设置
            SaveLanguage(item.Code);

            SelectedLanguage = item.Code;
        }

        private void LanguageList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _languageList.Items.Count)
                return;

            LanguageItem item = _languageList.Items[e.Index] as LanguageItem;

            if (item == null)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color background = selected
                ? Color.FromArgb(224, 239, 255)
                : Color.White;

            Color foreground = Color.FromArgb(38, 57, 77);

            using (SolidBrush brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            if (selected)
            {
                using (SolidBrush brush = new SolidBrush(
                    Color.FromArgb(22, 121, 207)))
                {
                    e.Graphics.FillRectangle(
                        brush,
                        e.Bounds.X,
                        e.Bounds.Y,
                        4,
                        e.Bounds.Height);
                }
            }

            string displayText =
                item.NativeName + "    " + item.EnglishName;

            TextRenderer.DrawText(
                e.Graphics,
                displayText,
                _languageList.Font,
                new Rectangle(
                    e.Bounds.X + 14,
                    e.Bounds.Y,
                    e.Bounds.Width - 20,
                    e.Bounds.Height),
                foreground,
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.Left |
                TextFormatFlags.EndEllipsis);

            e.DrawFocusRectangle();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            LanguageItem item = _languageList.SelectedItem as LanguageItem;

            if (item == null)
            {
                MessageBox.Show(this, "请选择一种语言。", "语言设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            string languageFile = Path.Combine(LanguageDirectory, item.Code + ".json");

            if (!File.Exists(languageFile))
            {
                MessageBox.Show(this, "语言文件不存在：\r\n" + languageFile, "语言设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            try
            {
                // The Lang implementation must support external JSON files.
                Lang.SetLanguage(item.Code);

                SaveLanguage(item.Code);
                LanguageHelper.Apply(this);
                SelectedLanguage = item.Code;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "语言切换失败：\r\n" + ex.Message, "语言设置", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Saves the selected language code.
        /// </summary>
        private void SaveLanguage(string language)
        {
            string directory = Path.GetDirectoryName(_settingsFile);

            if (!string.IsNullOrEmpty(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_settingsFile, language, new UTF8Encoding(false));
        }

        /// <summary>
        /// Restores the saved language.
        /// Call before creating the main form.
        /// </summary>
        public static void LoadSavedLanguage()
        {
            try
            {
                string file = Path.Combine(Application.LocalUserAppDataPath, "language.txt");

                if (!File.Exists(file))
                    return;

                string language = File.ReadAllText(file, Encoding.UTF8).Trim();

                if (string.IsNullOrWhiteSpace(language))
                    return;

                // Only restore a language that is currently available.
                string languageFile = Path.Combine(
                    LanguageDirectory,
                    language + ".json");

                if (!File.Exists(languageFile))
                    return;

                // Validate JSON before applying the language.
                string json = File.ReadAllText(
                    languageFile,
                    Encoding.UTF8);

                JObject.Parse(json);

                Lang.SetLanguage(language);
            }
            catch
            {
                // Ignore invalid or inaccessible saved settings.
            }
        }

        /// <summary>
        /// Represents a language discovered from a JSON file.
        /// </summary>
        private sealed class LanguageItem
        {
            public string Code { get; private set; }

            public string NativeName { get; private set; }

            public string EnglishName { get; private set; }

            public LanguageItem(
                string code,
                string nativeName,
                string englishName)
            {
                Code = code;
                NativeName = nativeName;
                EnglishName = englishName;
            }

            public override string ToString()
            {
                return NativeName;
            }
        }
    }
}