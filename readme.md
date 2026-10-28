# NetWinI18n

**A Lightweight JSON-Based Localization Library for Windows Forms**

NetWinI18n is a lightweight multilingual localization library for C# Windows Forms applications. It uses JSON language files, supports runtime language switching, and provides a simple API for translating forms, controls, menus, and other UI elements.

Designed for both legacy Windows desktop applications and modern .NET WinForms projects.

## Features

- **Multi-framework support** — Designed for .NET Framework and modern .NET Windows Forms.
- **JSON-based translations** — Simple, readable, and easy-to-maintain language files.
- **Runtime language switching** — Change the current language without restarting the application.
- **Automatic control translation** — Translate controls by assigning translation keys to their `Tag` property.
- **Menu translation** — Support for `MenuStrip`, `ContextMenuStrip`, `ToolStrip`, and nested menu items.
- **Fallback language** — Automatically fall back to the default language when a translation is missing.
- **External language files** — Load translations from JSON files outside the application.
- **Embedded language resources** — Include language files in the NuGet package.
- **Case-insensitive keys** — Translation keys are matched without case sensitivity.
- **Easy integration** — No need to replace your existing WinForms UI architecture.

## Installation

Install the package using the NuGet Package Manager:

```powershell
Install-Package NetWinI18n
```

Or using the .NET CLI:

```bash
dotnet add package NetWinI18n
```

You can also search for `NetWinI18n` in Visual Studio's NuGet Package Manager.

## Supported Frameworks

NetWinI18n 2.0.0 targets the following frameworks:

| Target Framework | Description |
|---|---|
| .NET Framework 4.5 | Legacy WinForms applications |
| .NET Framework 4.6 | WinForms applications |
| .NET Framework 4.7.2 | WinForms applications |
| .NET Framework 4.8 | WinForms applications |
| .NET 6 Windows | Modern WinForms applications |
| .NET 7 Windows | Modern WinForms applications |
| .NET 8 Windows | Modern WinForms applications |
| .NET 9 Windows | Modern WinForms applications |
| .NET 10 Windows | Modern WinForms applications |

The package must be tested against the target framework and Windows environment used by your application. Framework availability depends on the installed SDK and targeting packs.

## Quick Start

### 1. Import the namespace

```csharp
using NetWinI18n;
```

For WinForms projects, you will also typically use:

```csharp
using System.Windows.Forms;
```

### 2. Set translation keys

Set the `Tag` property of each control to its translation key.

```csharp
btnSave.Tag = "Common.Save";
btnCancel.Tag = "Common.Cancel";
lblTitle.Tag = "Main.Title";
```

You can also configure these values in the Visual Studio Properties window.

### 3. Apply translations

Call `LanguageHelper.Apply(this)` after `InitializeComponent()` has completed, typically in the form's `Load` event.

```csharp
using System;
using System.Windows.Forms;
using NetWinI18n;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        Load += MainForm_Load;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        LanguageHelper.Apply(this);
    }
}
```

The library translates controls whose `Tag` properties contain valid translation keys.

## Language Files

Create JSON files for each language.

Example project structure:

```text
YourApplication/
├── MainForm.cs
├── Program.cs
└── Languages/
    ├── zh-CN.json
    ├── zh-TW.json
    ├── en-US.json
    └── vi-VN.json
```

### Simplified Chinese — `zh-CN.json`

```json
{
  "Common.Save": "保存",
  "Common.Cancel": "取消",
  "Common.Delete": "删除",
  "Main.Title": "系统主界面",
  "Menu.File": "文件"
}
```

### English — `en-US.json`

```json
{
  "Common.Save": "Save",
  "Common.Cancel": "Cancel",
  "Common.Delete": "Delete",
  "Main.Title": "Main Dashboard",
  "Menu.File": "File"
}
```

### Traditional Chinese — `zh-TW.json`

```json
{
  "Common.Save": "儲存",
  "Common.Cancel": "取消",
  "Common.Delete": "刪除",
  "Main.Title": "系統主畫面",
  "Menu.File": "檔案"
}
```

### Vietnamese — `vi-VN.json`

```json
{
  "Common.Save": "Lưu",
  "Common.Cancel": "Hủy",
  "Common.Delete": "Xóa",
  "Main.Title": "Giao diện chính",
  "Menu.File": "Tệp"
}
```

Translation keys should be consistent across all language files.

## Runtime Language Switching

Use `LocalizationManager.SetLanguage()` to change the active language.

```csharp
LocalizationManager.SetLanguage("en-US");

LanguageHelper.Apply(this);
```

For example, a language selection control can be used as follows:

```csharp
private void cmbLanguage_SelectedIndexChanged(
    object sender,
    EventArgs e)
{
    string language = cmbLanguage.SelectedItem?.ToString();

    if (string.IsNullOrWhiteSpace(language))
        return;

    LocalizationManager.SetLanguage(language);

    LanguageHelper.Apply(this);
}
```

The selected language must correspond to a language file or embedded language resource.

**Note:** Changing the language updates the language manager, but does not automatically refresh every open form. Reapply translations to each form that needs to update.

## Configure the Default Language

Set the default language before applying translations.

```csharp
LocalizationManager.DefaultLanguage = "zh-CN";

LocalizationManager.SetLanguage("en-US");
```

If a key is missing from the current language, the library attempts to retrieve it from the default language.

If the key is not found in either language, `GetString()` returns the key itself.

## Get a Translation Directly

Use `LocalizationManager.GetString()` to retrieve translated text in code.

```csharp
string saveText = LocalizationManager.GetString("Common.Save");

MessageBox.Show(saveText);
```

You can also use it for dynamically generated messages:

```csharp
string message = LocalizationManager.GetString("Common.Delete");

MessageBox.Show(message);
```

Example JSON entry:

```json
{
  "Common.Delete": "Delete"
}
```

## Translate Menus and Toolbars

NetWinI18n supports menu items through their `Tag` properties.

```csharp
menuFile.Tag = "Menu.File";
menuSave.Tag = "Common.Save";

contextMenuDelete.Tag = "Common.Delete";

LanguageHelper.Apply(this);
```

Nested menu items can use their own translation keys.

The library can process:

- `MenuStrip`
- `ContextMenuStrip`
- `ToolStrip`
- `StatusStrip`
- Nested `ToolStripMenuItem` entries

## Use External Language Files

External JSON files allow translations to be changed without rebuilding the application.

```csharp
LocalizationManager.SetLanguageDirectory(
    System.IO.Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Languages"));

LocalizationManager.SetLanguage("en-US");

LanguageHelper.Apply(this);
```

The external directory takes priority over embedded resources.

For example:

```text
YourApplication/
├── YourApplication.exe
└── Languages/
    ├── zh-CN.json
    ├── en-US.json
    └── vi-VN.json
```

Ensure the JSON files are copied to the output directory during the build or deployment process.

## Listen for Language Changes

The `LanguageChanged` event can be used to respond to changes in the active language.

```csharp
LocalizationManager.LanguageChanged += (sender, e) =>
{
    // Refresh application UI or update language-dependent data.
};
```

Subscribe to this event when needed and unsubscribe when the subscriber is no longer required.

The event notifies subscribers; it does not automatically translate every open form.

## API Reference

| API | Description |
|---|---|
| `LanguageHelper.Apply(Control parent)` | Translate a form or control hierarchy |
| `LanguageHelper.Apply(ContextMenuStrip menu)` | Translate a context menu |
| `LanguageHelper.Apply(ToolStrip toolStrip)` | Translate a toolbar or menu strip |
| `LocalizationManager.GetString(string key)` | Retrieve translated text |
| `LocalizationManager.SetLanguage(string language)` | Change the active language |
| `LocalizationManager.CurrentLanguage` | Get the current language |
| `LocalizationManager.DefaultLanguage` | Get or set the fallback language |
| `LocalizationManager.SetLanguageDirectory(string directory)` | Configure the external language directory |
| `LocalizationManager.LanguageChanged` | Receive language change notifications |

## Important Notes

1. Set translation keys in the `Tag` property, not in the control's displayed `Text`.
2. Call `LanguageHelper.Apply(this)` after `InitializeComponent()`.
3. Ensure the language files use valid JSON syntax.
4. Ensure the language identifiers match their file names, such as `en-US.json`.
5. External language files must be readable by the application.
6. Controls with specialized item collections, such as `DataGridView`, `ListView`, `TreeView`, and `ComboBox`, may require additional translation logic for their columns, nodes, or items.
7. Third-party UI libraries may require adapters if their controls do not use standard WinForms `Text` behavior.
8. Test the package against each target framework you intend to support.

## Dependencies

- Newtonsoft.Json 13.0.3
- Windows Forms for supported Windows desktop targets

## License

Specify the license for your package before publishing it to NuGet. Add a `LICENSE` file to your repository if applicable.

## Author

**ShengbiYang**

Package: `NetWinI18n`

Version: `2.0.0`
