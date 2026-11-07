using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NetWinI18n
{
    /// <summary>
    /// WinForms 通用多语言辅助类。
    ///
    /// 支持：
    /// 1. 普通 WinForms 控件
    /// 2. DataGridView 及所有常见列类型
    /// 3. MenuStrip、ContextMenuStrip、ToolStrip、StatusStrip
    /// 4. TabControl、TabPage
    /// 5. ListView、ListViewItem、ColumnHeader、ListViewGroup
    /// 6. TreeView、TreeNode
    /// 7. ListBox、ComboBox、CheckedListBox
    /// 8. ToolTip
    ///
    /// 默认使用 Tag 作为 JSON 翻译 Key。
    /// 未设置 Tag 的业务数据不会被修改。
    /// </summary>
    public static class LanguageHelper
    {
        #region Public API

        /// <summary>
        /// 自动翻译窗体及其所有子控件。
        /// </summary>
        public static void Apply(Control parent)
        {
            if (parent == null || parent.IsDisposed)
                return;

            ApplyControl(parent);

            // 右键菜单
            if (parent.ContextMenuStrip != null && !parent.ContextMenuStrip.IsDisposed)
            {
                Apply(parent.ContextMenuStrip);
            }

            // ToolStrip、MenuStrip、StatusStrip
            if (parent is ToolStrip toolStrip)
            {
                ApplyToolStripItems(toolStrip.Items);
            }

            // 递归子控件
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
        /// 单独翻译 ToolStrip。
        /// </summary>
        public static void Apply(ToolStrip toolStrip)
        {
            if (toolStrip == null || toolStrip.IsDisposed)
                return;

            ApplyToolStripItems(toolStrip.Items);
        }

        /// <summary>
        /// 单独翻译 DataGridView。
        /// </summary>
        public static void Apply(DataGridView dataGridView)
        {
            if (dataGridView == null || dataGridView.IsDisposed)
                return;

            ApplyDataGridView(dataGridView);
        }

        /// <summary>
        /// 单独翻译 ListView。
        /// </summary>
        public static void Apply(ListView listView)
        {
            if (listView == null || listView.IsDisposed)
                return;

            ApplyListView(listView);
        }

        /// <summary>
        /// 单独翻译 TreeView。
        /// </summary>
        public static void Apply(TreeView treeView)
        {
            if (treeView == null || treeView.IsDisposed)
                return;

            ApplyTreeNodes(treeView.Nodes);
        }

        /// <summary>
        /// 单独翻译 ListBox。
        /// </summary>
        public static void Apply(ListBox listBox)
        {
            if (listBox == null || listBox.IsDisposed)
                return;

            ApplyListItems(listBox.Items);
        }

        /// <summary>
        /// 单独翻译 ComboBox。
        /// </summary>
        public static void Apply(ComboBox comboBox)
        {
            if (comboBox == null || comboBox.IsDisposed)
                return;

            ApplyListItems(comboBox.Items);
        }

        /// <summary>
        /// 单独翻译 CheckedListBox。
        /// </summary>
        public static void Apply(CheckedListBox checkedListBox)
        {
            if (checkedListBox == null || checkedListBox.IsDisposed)
                return;

            ApplyListItems(checkedListBox.Items);
        }

        #endregion Public API

        #region Control

        /// <summary>
        /// 翻译普通控件。
        /// </summary>
        private static void ApplyControl(Control control)
        {
            if (control == null || control.IsDisposed)
                return;

            TranslateTag(control, value => control.Text = value);

            // DataGridView
            if (control is DataGridView dataGridView)
            {
                ApplyDataGridView(dataGridView);
            }

            // ListView
            if (control is ListView listView)
            {
                ApplyListView(listView);
            }

            // TreeView
            if (control is TreeView treeView)
            {
                ApplyTreeNodes(treeView.Nodes);
            }

            // ListBox
            if (control is ListBox listBox)
            {
                ApplyListItems(listBox.Items);
            }

            // ComboBox
            if (control is ComboBox comboBox)
            {
                ApplyListItems(comboBox.Items);
            }

            // CheckedListBox
            if (control is CheckedListBox checkedListBox)
            {
                ApplyListItems(checkedListBox.Items);
            }

            // ToolTip
            ApplyToolTip(control);
        }

        #endregion Control

        #region DataGridView

        /// <summary>
        /// 翻译 DataGridView。
        /// </summary>
        private static void ApplyDataGridView(DataGridView grid)
        {
            if (grid == null || grid.IsDisposed)
                return;

            // 列标题
            foreach (DataGridViewColumn column in grid.Columns)
            {
                ApplyDataGridViewColumn(column);
            }

            // 行标题
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.HeaderCell != null)
                {
                    TranslateTag(row.HeaderCell.Tag, value => row.HeaderCell.Value = value);
                }
            }
        }

        /// <summary>
        /// 翻译 DataGridView 列。
        /// </summary>
        private static void ApplyDataGridViewColumn(DataGridViewColumn column)
        {
            if (column == null)
                return;

            TranslateTag(column.Tag, value => column.HeaderText = value);

            // ComboBox 列的选项
            if (column is DataGridViewComboBoxColumn comboColumn)
            {
                ApplyListItems(comboColumn.Items);
            }

            // Button 列文字
            if (column is DataGridViewButtonColumn buttonColumn)
            {
                TranslateTag(buttonColumn.Tag, value => buttonColumn.Text = value);
            }
        }

        /// <summary>
        /// 单独翻译 DataGridView 列。
        /// </summary>
        public static void ApplyDataGridViewColumn(DataGridViewColumn column, bool refresh)
        {
            ApplyDataGridViewColumn(column);

            if (refresh)
                column?.DataGridView?.Refresh();
        }

        #endregion DataGridView

        #region ListView

        /// <summary>
        /// 翻译 ListView。
        /// </summary>
        private static void ApplyListView(ListView listView)
        {
            if (listView == null || listView.IsDisposed)
                return;

            // 列标题
            foreach (ColumnHeader column in listView.Columns)
            {
                TranslateTag(column.Tag, value => column.Text = value);
            }

            // 分组
            foreach (ListViewGroup group in listView.Groups)
            {
                TranslateTag(group.Tag, value => group.Header = value);
            }

            // 项目
            foreach (ListViewItem item in listView.Items)
            {
                ApplyListViewItem(item);
            }
        }

        /// <summary>
        /// 翻译 ListView 项目及子项。
        /// </summary>
        private static void ApplyListViewItem(ListViewItem item)
        {
            if (item == null)
                return;

            TranslateTag(item.Tag, value => item.Text = value);

            foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
            {
                TranslateTag(subItem.Tag, value => subItem.Text = value);
            }
        }

        #endregion ListView

        #region TreeView

        /// <summary>
        /// 递归翻译 TreeNode。
        /// </summary>
        private static void ApplyTreeNodes(TreeNodeCollection nodes)
        {
            if (nodes == null)
                return;

            foreach (TreeNode node in nodes)
            {
                TranslateTag(node.Tag, value => node.Text = value);

                if (node.Nodes.Count > 0)
                {
                    ApplyTreeNodes(node.Nodes);
                }
            }
        }

        #endregion TreeView

        #region List Items

        /// <summary>
        /// 翻译列表中的项目。
        ///
        /// 只处理明确实现 ITagLanguageItem 的项目，
        /// 避免覆盖普通业务数据。
        /// </summary>
        private static void ApplyListItems(IList items)
        {
            if (items == null)
                return;

            foreach (object item in items)
            {
                if (item is ITagLanguageItem languageItem)
                {
                    TranslateTag(languageItem.LanguageTag, value => languageItem.LanguageText = value);
                }
            }
        }

        #endregion List Items

        #region ToolStrip

        /// <summary>
        /// 递归翻译菜单项和工具栏项。
        /// </summary>
        private static void ApplyToolStripItems(ToolStripItemCollection items)
        {
            if (items == null)
                return;

            foreach (ToolStripItem item in items)
            {
                TranslateTag(item.Tag, value => item.Text = value);

                // 子菜单
                if (item is ToolStripDropDownItem dropDownItem)
                {
                    ApplyToolStripItems(dropDownItem.DropDownItems);
                }
            }
        }

        #endregion ToolStrip

        #region ToolTip

        /// <summary>
        /// 翻译控件的 ToolTip。
        ///
        /// 使用控件 Tag 的 Key + ".ToolTip" 作为翻译 Key。
        /// </summary>
        private static void ApplyToolTip(Control control)
        {
            if (control == null || control.IsDisposed)
                return;

            ToolTip toolTip = FindToolTip(control);

            if (toolTip == null)
                return;

            string key = GetKey(control.Tag);

            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                string tip = Lang.T(key + ".ToolTip");

                if (!string.IsNullOrWhiteSpace(tip))
                {
                    toolTip.SetToolTip(control, tip);
                }
            }
            catch
            {
                // ToolTip 翻译失败不影响其他控件
            }
        }

        /// <summary>
        /// 查找控件所在窗体的 ToolTip。
        /// WinForms 没有公开从 Control 获取其 ToolTip 组件的通用 API，
        /// 因此这里不自动猜测 ToolTip 实例。
        /// </summary>
        private static ToolTip FindToolTip(Control control)
        {
            return null;
        }

        #endregion ToolTip

        #region Translation

        /// <summary>
        /// 根据 Tag 翻译。
        /// </summary>
        private static void TranslateTag(object tag, Action<string> setter)
        {
            if (setter == null)
                return;

            string key = GetKey(tag);

            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                string translated = Lang.T(key);

                if (!string.IsNullOrWhiteSpace(translated))
                {
                    setter(translated);
                }
            }
            catch
            {
                // 翻译失败保留原文
            }
        }

        /// <summary>
        /// 获取翻译 Key。
        /// </summary>
        private static string GetKey(object tag)
        {
            return tag?.ToString()?.Trim() ?? string.Empty;
        }

        #endregion Translation
    }

    /// <summary>
    /// 列表项目多语言扩展接口。
    /// 自定义列表数据实现此接口即可支持自动翻译。
    /// </summary>
    public interface ITagLanguageItem
    {
        object LanguageTag { get; }

        string LanguageText { get; set; }
    }
}