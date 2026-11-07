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
    /// Blue modern UI theme.
    /// </summary>
    public class LanguageSettingsForm : Form
    {
        #region Fields

        private readonly ListBox _languageList;

        private readonly Label _titleLabel;
        private readonly Label _descriptionLabel;
        private readonly Label _currentLanguageLabel;
        private readonly Label _listTitleLabel;

        private readonly Button _applyButton;
        private readonly Button _cancelButton;

        private readonly List<LanguageItem> _languages = new List<LanguageItem>();

        private readonly string _settingsFile;

        // Theme colors
        private static readonly Color PrimaryBlue = Color.FromArgb(23, 105, 194);

        private static readonly Color DarkBlue = Color.FromArgb(18, 59, 112);

        private static readonly Color LightBlue = Color.FromArgb(231, 242, 255);

        private static readonly Color BackgroundColor = Color.FromArgb(243, 247, 252);

        private static readonly Color TextColor = Color.FromArgb(39, 57, 79);

        private static readonly Color SecondaryTextColor = Color.FromArgb(112, 132, 155);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the language selected by the user.
        /// </summary>
        public string SelectedLanguage { get; private set; }

        /// <summary>
        /// Language JSON directory.
        /// </summary>
        public static string LanguageDirectory
        {
            get
            {
                return Path.Combine(Application.StartupPath, "Languages");
            }
        }

        #endregion Properties

        #region Constructor

        public LanguageSettingsForm()
        {
            Lang.InitializeLanguageFiles();
            _settingsFile = Path.Combine(Application.LocalUserAppDataPath, "language.txt");

            // Form settings
            Text = "Language Settings";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            ClientSize = new Size(560, 460);

            Font = new Font("Microsoft YaHei UI", 9F);

            BackColor = BackgroundColor;

            // Build UI
            _titleLabel = new Label();
            _descriptionLabel = new Label();
            _currentLanguageLabel = new Label();
            _listTitleLabel = new Label();

            _languageList = new ListBox();

            _applyButton = new Button();
            _cancelButton = new Button();

            InitializeHeader();
            InitializeContent();
            InitializeFooter();

            // Load available languages
            LoadLanguages();

            foreach (LanguageItem item in _languages)
            {
                _languageList.Items.Add(item);
            }

            _applyButton.Enabled = _languageList.Items.Count > 0;

            SelectCurrentLanguage();

            AcceptButton = _applyButton;
            CancelButton = _cancelButton;
        }

        #endregion Constructor

        #region UI Initialization

        /// <summary>
        /// Header section.
        /// </summary>
        private void InitializeHeader()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = DarkBlue
            };

            // Main title
            _titleLabel.Text = "语言设置 / Language Settings";

            _titleLabel.ForeColor = Color.White;

            _titleLabel.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);

            _titleLabel.Location = new Point(25, 17);
            _titleLabel.AutoSize = true;

            // Description
            _descriptionLabel.Text = "选择你希望使用的界面语言，切换后立即生效。";

            _descriptionLabel.ForeColor = Color.FromArgb(195, 216, 241);

            _descriptionLabel.Font = new Font("Microsoft YaHei UI", 9F);

            _descriptionLabel.Location = new Point(27, 58);

            _descriptionLabel.AutoSize = true;

            header.Controls.Add(_titleLabel);
            header.Controls.Add(_descriptionLabel);

            Controls.Add(header);
        }

        /// <summary>
        /// Main content section.
        /// </summary>
        private void InitializeContent()
        {
            // Card panel
            Panel card = new Panel
            {
                Location = new Point(22, 119),
                Size = new Size(516, 267),
                BackColor = Color.White
            };

            card.Paint += delegate (object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Color.FromArgb(222, 232, 244)))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            // List title
            _listTitleLabel.Text = "可用语言 / Available Languages";

            _listTitleLabel.Location = new Point(18, 17);

            _listTitleLabel.AutoSize = true;

            _listTitleLabel.ForeColor = TextColor;

            _listTitleLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

            // Language list
            _languageList.Location = new Point(18, 48);

            _languageList.Size = new Size(480, 157);

            _languageList.Font = new Font("Microsoft YaHei UI", 10F);

            _languageList.BorderStyle = BorderStyle.None;

            _languageList.IntegralHeight = false;

            _languageList.DrawMode = DrawMode.OwnerDrawFixed;

            _languageList.ItemHeight = 42;

            _languageList.BackColor = Color.White;

            _languageList.ForeColor = TextColor;

            _languageList.DrawItem += LanguageList_DrawItem;

            _languageList.SelectedIndexChanged += LanguageList_SelectedIndexChanged;

            // Current language
            _currentLanguageLabel.Location = new Point(20, 218);

            _currentLanguageLabel.Size = new Size(475, 28);

            _currentLanguageLabel.ForeColor = SecondaryTextColor;

            _currentLanguageLabel.Font = new Font("Microsoft YaHei UI", 9F);

            _currentLanguageLabel.Text = "当前语言 / Current Language";

            card.Controls.Add(_listTitleLabel);
            card.Controls.Add(_languageList);
            card.Controls.Add(_currentLanguageLabel);

            Controls.Add(card);
        }

        /// <summary>
        /// Footer section.
        /// </summary>
        private void InitializeFooter()
        {
            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.White
            };

            footer.Paint += delegate (object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Color.FromArgb(224, 232, 241)))
                {
                    e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
                }
            };

            // Apply button
            _applyButton.Text = "应用语言 / Apply";

            _applyButton.Size = new Size(132, 37);

            _applyButton.Location = new Point(270, 14);

            _applyButton.FlatStyle = FlatStyle.Flat;

            _applyButton.FlatAppearance.BorderSize = 0;

            _applyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 125, 213);

            _applyButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(15, 82, 158);

            _applyButton.BackColor = PrimaryBlue;

            _applyButton.ForeColor = Color.White;

            _applyButton.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

            _applyButton.Cursor = Cursors.Hand;

            _applyButton.UseVisualStyleBackColor = false;

            _applyButton.Click += ApplyButton_Click;

            // Cancel button
            _cancelButton.Text = "取消 / Cancel";

            _cancelButton.Size = new Size(112, 37);

            _cancelButton.Location = new Point(414, 14);

            _cancelButton.FlatStyle = FlatStyle.Flat;

            _cancelButton.FlatAppearance.BorderSize = 1;

            _cancelButton.FlatAppearance.BorderColor = Color.FromArgb(210, 222, 236);

            _cancelButton.FlatAppearance.MouseOverBackColor = LightBlue;

            _cancelButton.BackColor = Color.White;

            _cancelButton.ForeColor = TextColor;

            _cancelButton.Font = new Font("Microsoft YaHei UI", 9F);

            _cancelButton.Cursor = Cursors.Hand;

            _cancelButton.Click += CancelButton_Click;

            footer.Controls.Add(_applyButton);
            footer.Controls.Add(_cancelButton);

            Controls.Add(footer);
        }

        #endregion UI Initialization

        #region Load Languages

        /// <summary>
        /// Scans the Languages directory.
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
                    CultureInfo culture = CultureInfo.GetCultureInfo(code);

                    string json = File.ReadAllText(file, Encoding.UTF8);

                    JObject.Parse(json);

                    _languages.Add(new LanguageItem(code, culture.NativeName, culture.EnglishName));
                }
                catch
                {
                    // Ignore invalid language files.
                }
            }

            _languages.Sort(delegate (LanguageItem a, LanguageItem b)
            {
                return string.Compare(a.NativeName, b.NativeName, StringComparison.CurrentCulture);
            });
        }

        #endregion Load Languages

        #region Select Language

        /// <summary>
        /// Selects the current language.
        /// </summary>
        private void SelectCurrentLanguage()
        {
            string current = Lang.CurrentLanguage;

            for (int i = 0; i < _languageList.Items.Count; i++)
            {
                LanguageItem item = _languageList.Items[i] as LanguageItem;

                if (item != null && string.Equals(item.Code, current, StringComparison.OrdinalIgnoreCase))
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

        /// <summary>
        /// Language selection changed.
        /// </summary>
        private void LanguageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LanguageItem item = _languageList.SelectedItem as LanguageItem;

            if (item == null)
                return;

            _currentLanguageLabel.Text = "选择语言 / Selected: " + item.NativeName + " (" + item.Code + ")";

            // Switch language
            Lang.SetLanguage(item.Code);

            // Refresh current form
            LanguageHelper.Apply(this);

            // Refresh other open forms
            foreach (Form form in Application.OpenForms)
            {
                if (form != this && !(form is LanguageSettingsForm))
                {
                    LanguageHelper.Apply(form);
                }
            }

            // Save selected language
            SaveLanguage(item.Code);

            SelectedLanguage = item.Code;

            _languageList.Invalidate();
        }

        #endregion Select Language

        #region Draw Language List

        /// <summary>
        /// Custom language list rendering.
        /// </summary>
        private void LanguageList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _languageList.Items.Count)
                return;

            LanguageItem item = _languageList.Items[e.Index] as LanguageItem;

            if (item == null)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Rectangle bounds = e.Bounds;

            Color background = selected ? LightBlue : Color.White;

            Color foreground = selected ? DarkBlue : TextColor;

            using (SolidBrush brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            // Selected indicator
            if (selected)
            {
                using (SolidBrush brush = new SolidBrush(PrimaryBlue))
                {
                    e.Graphics.FillRectangle(brush, bounds.X, bounds.Y + 5, 4, bounds.Height - 10);
                }
            }

            // Language native name
            Rectangle nativeRect = new Rectangle(bounds.X + 17, bounds.Y + 3, bounds.Width - 35, 20);

            using (Font nativeFont = new Font("Microsoft YaHei UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular))
            {
                TextRenderer.DrawText(e.Graphics, item.NativeName, nativeFont, nativeRect, foreground, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // English name and language code
            Rectangle englishRect = new Rectangle(bounds.X + 17, bounds.Y + 21, bounds.Width - 35, 17);

            string detail = item.EnglishName + "  ·  " + item.Code;

            TextRenderer.DrawText(e.Graphics, detail, new Font("Segoe UI", 8F), englishRect, SecondaryTextColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            e.DrawFocusRectangle();
        }

        #endregion Draw Language List

        #region Apply / Cancel

        /// <summary>
        /// Apply language settings.
        /// </summary>
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

        /// <summary>
        /// Cancel.
        /// </summary>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Apply / Cancel

        #region Save / Restore

        /// <summary>
        /// Saves language settings.
        /// </summary>
        private void SaveLanguage(string language)
        {
            string directory = Path.GetDirectoryName(_settingsFile);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_settingsFile, language, new UTF8Encoding(false));
        }

        /// <summary>
        /// Restores saved language.
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

                string languageFile = Path.Combine(LanguageDirectory, language + ".json");

                if (!File.Exists(languageFile))
                    return;

                string json = File.ReadAllText(languageFile, Encoding.UTF8);

                JObject.Parse(json);

                Lang.SetLanguage(language);
            }
            catch
            {
                // Ignore invalid saved settings.
            }
        }

        #endregion Save / Restore

        #region Language Item

        /// <summary>
        /// Language item model.
        /// </summary>
        private sealed class LanguageItem
        {
            public string Code { get; private set; }

            public string NativeName { get; private set; }

            public string EnglishName { get; private set; }

            public LanguageItem(string code, string nativeName, string englishName)
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

        #endregion Language Item
    }
}