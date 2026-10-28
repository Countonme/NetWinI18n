using System;
using System.Windows.Forms;

namespace NetWinI18n
{
    /// <summary>
    /// WinForms 多语言控件辅助类。
    /// 使用控件的 Tag 作为 JSON 翻译 Key。
    /// </summary>
    public static class LanguageHelper
    {
        /// <summary>
        /// 自动翻译窗体及其所有子控件。
        /// 原有调用方式保持兼容。
        /// </summary>
        public static void Apply(Control parent)
        {
            if (parent == null || parent.IsDisposed)
                return;

            ApplyControlText(parent);

            // 翻译右键菜单
            if (parent.ContextMenuStrip != null && !parent.ContextMenuStrip.IsDisposed)
            {
                Apply(parent.ContextMenuStrip);
            }

            // 翻译 MenuStrip、ToolStrip、StatusStrip
            if (parent is ToolStrip toolStrip)
            {
                ApplyToolStripItems(toolStrip.Items);
            }

            // 递归翻译子控件
            foreach (Control child in parent.Controls)
            {
                Apply(child);
            }
        }

        /// <summary>
        /// 单独翻译右键菜单。
        /// </summary>
        public static void Apply(ContextMenuStrip menu)
        {
            if (menu == null || menu.IsDisposed)
                return;

            ApplyToolStripItems(menu.Items);
        }

        /// <summary>
        /// 单独翻译任意 ToolStrip。
        /// </summary>
        public static void Apply(ToolStrip toolStrip)
        {
            if (toolStrip == null || toolStrip.IsDisposed)
                return;

            ApplyToolStripItems(toolStrip.Items);
        }

        /// <summary>
        /// 翻译普通 WinForms 控件。
        /// </summary>
        private static void ApplyControlText(Control control)
        {
            if (control.Tag == null)
                return;

            string key = control.Tag.ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                control.Text = Lang.T(key);
            }
            catch
            {
                // 多语言翻译失败时，不影响其他控件。
            }
        }

        /// <summary>
        /// 递归翻译菜单项、工具栏项及子菜单。
        /// </summary>
        private static void ApplyToolStripItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item.Tag != null)
                {
                    string key = item.Tag.ToString()?.Trim()
                                 ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        try
                        {
                            item.Text = Lang.T(key);
                        }
                        catch
                        {
                            // 单个菜单项失败不影响其他项目。
                        }
                    }
                }

                if (item is ToolStripDropDownItem dropDownItem)
                {
                    ApplyToolStripItems(dropDownItem.DropDownItems);
                }
            }
        }
    }
}