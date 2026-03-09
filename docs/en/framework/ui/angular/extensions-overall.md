```json
//[doc-seo]
{
    "Description": "Discover how to enhance your Angular UI with extensions for actions, data tables, toolbars, and forms in the ABP Framework."
}
```

# Angular UI Extensions

Angular UI extensions system allows you to add a new action to the actions menu, a new column to the data table, a new action to the toolbar of a page, and add a new field to the create and/or edit forms.

See the documents below for the details:

* [Entity Action Extensions](entity-action-extensions.md)
* [Data Table Column (or Entity Prop) Extensions](data-table-column-extensions.md)
* [Page Toolbar Extension](page-toolbar-extensions.md)
* [Dynamic Form (or Form Prop) Extensions](dynamic-form-extensions.md)

##  Extensible Table Component

Using [ngx-datatable](https://github.com/swimlane/ngx-datatable) in extensible table.

````ts
      <abp-extensible-table
         actionsText="Your Action"
         [data]="items"
         [recordsTotal]="totalCount"
         [actionsColumnWidth]="38"
         [actionsTemplate]="customAction"
         [list]="list"
         (tableActivate)="onTableSelect($event)" 
      /> 
````

 * `       actionsText : ` Column name of action column. **Type** : `string`
 * `              data : ` Items shown in your table. **Type** : `Array<any>`
 * `              list : ` Instance of ListService. **Type** : `ListService`
 * `actionsColumnWidth : ` Width of your action column. **Type** : `number`
 * `   actionsTemplate : ` Template of the action (e.g. an `ng-template`). **Type** : `TemplateRef<any>`
 * `      recordsTotal : ` Total count of records. **Type** : `number`
 * `    tableActivate  : ` Output fired when a cell or row is focused via keyboard or mouse click. **Type** : `EventEmitter`

### Multiple Selection

The extensible table supports both single-row and multi-row selection. Use the `selectable` input to enable selection, and `selectionType` to control the selection mode.

````ts
<abp-extensible-table
  [data]="items"
  [recordsTotal]="totalCount"
  [list]="list"
  [selectable]="true"
  [selectionType]="'multiClick'"
  [selected]="selectedRows"
  (selectionChange)="onSelectionChange($event)"
/>
````

When `selectionType` is `'single'`, each row displays a **radio button** and the header does not show a "select all" checkbox. For all other selection types (e.g. `'multiClick'`, `'checkbox'`), each row shows a **checkbox** and the header includes a "select all" checkbox.

 * `        selectable : ` Enables the row selection column. **Type** : `boolean`, **Default** : `false`
 * `   selectionType  : ` Controls the selection mode. Accepts `SelectionType` values such as `'single'`, `'multi'`, `'multiClick'`, `'checkbox'`, or `'cell'`. **Type** : `SelectionType | string`, **Default** : `'multiClick'`
 * `        selected  : ` The currently selected rows. **Type** : `any[]`, **Default** : `[]`
 * ` selectionChange  : ` Output fired when the selection changes. **Type** : `EventEmitter<any[]>`

### Infinite Scroll

The extensible table supports infinite scrolling as an alternative to pagination. When enabled, the table emits a `loadMore` event as the user scrolls near the bottom, allowing you to load additional data on demand. Pagination is hidden while infinite scroll is active.

````ts
<abp-extensible-table
  [data]="items"
  [recordsTotal]="totalCount"
  [list]="list"
  [infiniteScroll]="true"
  [isLoading]="isLoading"
  [tableHeight]="500"
  [scrollThreshold]="10"
  (loadMore)="onLoadMore()"
/>
````

In your component, append newly fetched data to the existing `items` array when `loadMore` fires:

````ts
onLoadMore(): void {
  if (this.isLoading) return;
  this.isLoading = true;
  // fetch next page and append results
  this.myService.getList({ skipCount: this.items.length, maxResultCount: 10 }).subscribe(result => {
    this.items = [...this.items, ...result.items];
    this.isLoading = false;
  });
}
````

> **Note:** When `infiniteScroll` is `true`, set a fixed `tableHeight` so the table has a scrollable viewport. Pagination is automatically hidden.

 * `  infiniteScroll  : ` Enables infinite scroll mode (hides pagination). **Type** : `boolean`, **Default** : `false`
 * `     isLoading   : ` Indicates that more data is being fetched. Prevents duplicate `loadMore` events and shows a loading indicator. **Type** : `boolean`, **Default** : `false`
 * `   tableHeight   : ` Fixed height of the table in pixels when `infiniteScroll` is enabled. **Type** : `number`
 * ` scrollThreshold : ` Distance from the bottom (in pixels) at which `loadMore` is triggered. **Type** : `number`, **Default** : `10`
 * `      loadMore   : ` Output fired when the user scrolls near the bottom of the table (only when `infiniteScroll` is `true` and `isLoading` is `false`). **Type** : `EventEmitter<void>`
