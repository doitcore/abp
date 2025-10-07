# Gemini AI - Angular & ABP Framework Development Guidelines

## Project Overview
This is an enterprise Angular application using the ABP Framework with Nx workspace structure and NGXS for state management.

## Angular Development Standards

### Component Architecture
Always create components with:
- OnPush change detection strategy
- Proper lifecycle hook implementation (OnDestroy for cleanup)
- Standalone components for new features
- TypeScript strict typing (avoid `any`)

```typescript
@Component({
  selector: 'app-example',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule]
})
export class ExampleComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### Service Development
- Use `providedIn: 'root'` for singleton services
- Return Observables for async operations
- Implement proper error handling with RxJS operators
- Use dependency injection properly

```typescript
@Injectable({ providedIn: 'root' })
export class DataService {
  constructor(private http: HttpClient) {}
  
  getData(): Observable<Data[]> {
    return this.http.get<Data[]>('/api/data').pipe(
      retry(2),
      catchError(this.handleError),
      shareReplay(1)
    );
  }
}
```

### RxJS Patterns
- **switchMap**: For search/navigation (cancels previous)
- **mergeMap**: For parallel operations
- **concatMap**: For sequential operations
- **exhaustMap**: For form submissions (ignores new until complete)

Always unsubscribe using:
- `takeUntil(this.destroy$)` pattern
- `async` pipe in templates (preferred)
- `take(1)` for single emissions

### Template Best Practices
```html
<!-- ✅ DO: Use async pipe -->
<div *ngIf="data$ | async as data">
  <div *ngFor="let item of data.items; trackBy: trackById">
    {{ item.name }}
  </div>
</div>

<!-- ❌ DON'T: Manual subscription -->
```

## ABP Framework Integration

### Localization
```typescript
// Component
this.localization.instant('::LocalizationKey')

// Template
{{ '::WelcomeMessage' | abpLocalization }}
```

### Permissions
```typescript
// Template
<button *abpPermission="'MyApp.Books.Create'">Create</button>

// Component
canEdit$ = this.config.getGrantedPolicy$('MyApp.Books.Edit');
```

### API Proxy Services
Always use ABP's generated proxy services instead of manual HTTP calls:

```typescript
import { BookService } from '@proxy/books';

constructor(private bookService: BookService) {}

getBooks(): Observable<PagedResultDto<BookDto>> {
  return this.bookService.getList({ maxResultCount: 10 });
}
```

### State Management (NGXS)
```typescript
@State<BooksStateModel>({
  name: 'books',
  defaults: { books: [], loading: false }
})
@Injectable()
export class BooksState {
  @Selector()
  static getBooks(state: BooksStateModel) {
    return state.books;
  }

  @Action(GetBooks)
  getBooks(ctx: StateContext<BooksStateModel>) {
    ctx.patchState({ loading: true });
    return this.bookService.getList().pipe(
      tap(response => {
        ctx.patchState({ 
          books: response.items,
          loading: false 
        });
      })
    );
  }
}
```

## Code Quality Standards

### TypeScript
- Use strict mode
- Avoid `any` type
- Use interfaces and types properly
- Implement proper null checks

### Testing
- Write unit tests with Jest
- Mock dependencies properly
- Test both success and error scenarios
- Aim for good coverage

### Performance
- Use OnPush change detection
- Lazy load feature modules
- Implement trackBy for lists
- Use production builds
- Avoid memory leaks

### Security
- Sanitize user inputs
- Use Angular's built-in XSS protection
- Validate on client and server
- Follow ABP's security patterns
- Never expose sensitive data

## File Structure (Nx Workspace)
```
libs/
  feature-name/
    src/
      lib/
        components/
        services/
        models/
        state/
        guards/
```

## Common Patterns

### Smart/Dumb Components
```typescript
// Smart (Container)
@Component({
  template: `
    <app-list 
      [items]="items$ | async"
      (itemSelected)="onSelect($event)">
    </app-list>
  `
})
export class ContainerComponent {
  items$ = this.store.select(getItems);
  constructor(private store: Store) {}
}

// Dumb (Presentational)
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListComponent {
  @Input() items: Item[] = [];
  @Output() itemSelected = new EventEmitter<Item>();
}
```

### Reactive Forms
```typescript
form = this.fb.group({
  name: ['', [Validators.required, Validators.minLength(3)]],
  email: ['', [Validators.required, Validators.email]]
});
```

## Best Practices Checklist
✅ OnPush change detection
✅ Proper unsubscription
✅ Async pipe in templates
✅ TypeScript strict typing
✅ Error handling
✅ Unit tests
✅ Localization (no hardcoded strings)
✅ Permission checks
✅ Accessibility attributes
✅ Performance optimization

## Anti-Patterns to Avoid
❌ Using `any` type
❌ Manual subscriptions without cleanup
❌ Logic in templates
❌ Nested subscriptions
❌ Mutating state directly
❌ Missing error handling
❌ Hardcoded strings

## Resources
- Angular Style Guide: https://angular.io/guide/styleguide
- ABP Documentation: https://docs.abp.io
- RxJS Operators: https://rxjs.dev/guide/operators

Always prioritize code maintainability, readability, and following Angular and ABP Framework best practices.
