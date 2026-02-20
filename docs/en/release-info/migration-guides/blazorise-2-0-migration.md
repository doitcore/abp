```json
//[doc-seo]
{
    "Description": "This migration guide provides a comprehensive overview of the necessary code changes when upgrading your ABP solution from Blazorise 1.x to 2.0, ensuring a smooth transition to the latest version."
}
```

# ABP Blazorise 1.x to 2.0 Migration Guide

This document summarizes the required code changes when upgrading ABP solutions from Blazorise 1.x to 2.0.

## 1. Package upgrades

Upgrade Blazorise-related packages to `2.0.0`.

- `Blazorise`
- `Blazorise.Components`
- `Blazorise.DataGrid`
- `Blazorise.Snackbar`
- `Blazorise.Bootstrap5`
- `Blazorise.Icons.FontAwesome`

## 2. Input component renames

Blazorise 2.0 uses new input component names:

- `TextEdit` -> `TextInput`
- `MemoEdit` -> `MemoInput`
- `DateEdit` -> `DateInput`
- `TimeEdit` -> `TimeInput`
- `NumericEdit` -> `NumericInput`

## 3. Binding API normalization to Value/ValueChanged

Migrate old binding/value APIs to the new `Value` model.

- `@bind-Text` -> `@bind-Value`
- `Text` / `TextChanged` -> `Value` / `ValueChanged`
- `@bind-Checked` -> `@bind-Value`
- `Checked` / `CheckedChanged` -> `Value` / `ValueChanged`
- `CheckedValue` / `CheckedValueChanged` -> `Value` / `ValueChanged`
- `@bind-SelectedValue` (for `Select`) -> `@bind-Value`
- `SelectedValue` / `SelectedValueChanged` (for `Select`) -> `Value` / `ValueChanged`

## 4. DatePicker and Select multiple changes

### DatePicker range mode

For `SelectionMode="DateInputSelectionMode.Range"`:

- `ValueChanged` -> `ValuesChanged`

### Select multiple mode

For `<Select Multiple ...>`:

- `Value` -> `Values`
- `ValueChanged` -> `ValuesChanged`
- `@bind-Value` -> `@bind-Values`

### Empty SelectItem type requirement

For empty placeholder items, set explicit `TValue`:

- `<SelectItem></SelectItem>` -> `<SelectItem TValue="string"></SelectItem>` (or another correct type such as `Guid?`)

## 5. DataGrid migration

### 5.1 Page parameter rename

- `CurrentPage` -> `Page` on `DataGrid`

Important: `AbpExtensibleDataGrid` still uses `CurrentPage` (for example ABP v10.2). Do not rename it to `Page`.

### 5.2 DisplayTemplate context type change

Inside `DisplayTemplate`, use `context.Item` instead of directly using `context`.

Typical updates:

- `context.Property` -> `context.Item.Property`
- `Method(context)` -> `Method(context.Item)`
- `() => Method(context)` -> `() => Method(context.Item)`

Important: This change applies to DataGrid template contexts only (`DisplayTemplate` in `DataGridColumn`, `DataGridEntityActionsColumn`, etc.). In non-DataGrid templates (for example `TreeView` `NodeContent`), `context` is already the item and should remain unchanged (for example `DeleteMenuItemAsync(context)`).

### 5.3 Width type change (string -> Fluent sizing)

DataGrid column `Width` moved from plain string to fluent sizing APIs:

- `Width="30px"` -> `Width="Width.Px(30)"`
- `Width="60px"` -> `Width="Width.Px(60)"`
- `Width="50%"` -> `Width="Width.Percent(50)"` or `Width="Width.Is50"`
- `Width="100%"` -> `Width="Width.Is100"`

For dynamic string widths (for example `column.Width`), ABP introduces `BlazoriseFluentSizingParse.Parse(...)` to convert string values into `IFluentSizingStyle`.

```csharp
Width="@BlazoriseFluentSizingParse.Parse(column.Width)" // column.Width is a string
```

## 6. Modal parameter placement changes

`Size` and `Centered` should be placed on `<Modal>`, not on `<ModalContent>`.

- `<ModalContent Size="..." Centered="true">` -> `<Modal Size="..." Centered="true">` + `<ModalContent>`

## 7. Other component parameter changes

- `Dropdown RightAligned="true"` -> `Dropdown EndAligned="true"`
- `Autocomplete MinLength` -> `MinSearchLength`

## 8. Notes from ABP migration implementation

- Keep component-specific behavior in mind. Not every component follows exactly the same rename pattern.
- `Autocomplete` usage can still involve `SelectedValue` / `SelectedValueChanged`, depending on component API.
- `BarDropdown` and `Dropdown` are different components; align parameter names according to the actual component type.

# Reference

This document may not cover all Blazorise 2.0 changes. For completeness, refer to the official migration guide and release notes:

- [Blazorise 2.0 - Release Notes](https://blazorise.com/news/release-notes/200)
