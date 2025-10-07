# Junie AI Guidelines - Angular & ABP Framework

## Introduction
You are assisting with an Angular application built on the ABP Framework. Follow these guidelines to generate high-quality, maintainable code that adheres to best practices.

## Core Principles
1. **Type Safety**: Use TypeScript strict mode, avoid `any`
2. **Performance**: OnPush change detection, lazy loading
3. **Maintainability**: Clean, readable, well-documented code
4. **Security**: Input validation, proper authentication/authorization
5. **Accessibility**: WCAG 2.1 compliance, ARIA attributes

## Angular Component Guidelines

### Component Structure
```typescript
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-feature',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './feature.component.html',
  styleUrls: ['./feature.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FeatureComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    // Initialization
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### Key Requirements
- ✅ Use OnPush change detection
- ✅ Implement OnDestroy for cleanup
- ✅ Prefer standalone components
- ✅ Use proper TypeScript types
- ✅ Follow smart/dumb component pattern

## Service Development

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

  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('Service error:', error);
    return throwError(() => new Error('Operation failed'));
  }
}
```

## RxJS Best Practices

### Operator Selection
| Operator | Use Case | Example |
|----------|----------|---------|
| `switchMap` | Search, navigation (cancel previous) | Search input |
| `mergeMap` | Parallel operations | Batch API calls |
| `concatMap` | Sequential operations | Ordered processing |
| `exhaustMap` | Ignore until complete | Form submission |

### Subscription Management
```typescript
// ✅ BEST: Use async pipe
data$ = this.service.getData();

// ✅ GOOD: Use takeUntil
this.service.getData()
  .pipe(takeUntil(this.destroy$))
  .subscribe(data => this.handleData(data));

// ❌ BAD: No unsubscription
this.service.getData().subscribe(data => this.data = data);
```

## ABP Framework Integration

### Localization
```typescript
// Service injection
constructor(private localization: LocalizationService) {}

// Usage in component
getTranslation(key: string): string {
  return this.localization.instant(`::${key}`);
}

// Template usage
{{ '::PageTitle' | abpLocalization }}
```

### Permission System
```typescript
// Directive in template
<button *abpPermission="'MyApp.Books.Create'">Create Book</button>

// Check in component
canEdit(): boolean {
  return this.config.getGrantedPolicy('MyApp.Books.Edit');
}

// Observable permission
canEdit$ = this.config.getGrantedPolicy$('MyApp.Books.Edit');
```

### API Proxy Services
```typescript
// ✅ DO: Use generated proxy
import { BookService } from '@proxy/books';

constructor(private bookService: BookService) {}

loadBooks(): void {
  this.books$ = this.bookService.getList({ maxResultCount: 10 });
}

// ❌ DON'T: Manual HTTP calls for ABP APIs
```

### State Management (NGXS)
```typescript
// Actions
export class LoadBooks {
  static readonly type = '[Books] Load Books';
}

// State
@State<BooksStateModel>({
  name: 'books',
  defaults: { books: [], loading: false }
})
@Injectable()
export class BooksState {
  constructor(private bookService: BookService) {}

  @Selector()
  static books(state: BooksStateModel) {
    return state.books;
  }

  @Action(LoadBooks)
  loadBooks(ctx: StateContext<BooksStateModel>) {
    ctx.patchState({ loading: true });
    return this.bookService.getList().pipe(
      tap(response => ctx.patchState({ 
        books: response.items, 
        loading: false 
      }))
    );
  }
}
```

## Forms

### Reactive Forms
```typescript
export class FormComponent implements OnInit {
  form: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      age: [null, [Validators.min(0), Validators.max(120)]]
    });
  }

  onSubmit(): void {
    if (this.form.valid) {
      const formData = this.form.getRawValue();
      this.submitData(formData);
    }
  }
}
```

## Template Best Practices

```html
<!-- ✅ Use async pipe -->
<div *ngIf="users$ | async as users">
  <div *ngFor="let user of users; trackBy: trackByUserId">
    <h3>{{ user.name }}</h3>
    <p>{{ user.email }}</p>
  </div>
</div>

<!-- ✅ Accessibility -->
<button 
  type="button"
  [attr.aria-label]="'::Close' | abpLocalization"
  (click)="close()">
  <i class="fa fa-times" aria-hidden="true"></i>
</button>

<!-- ✅ Localization -->
<h1>{{ '::WelcomeMessage' | abpLocalization }}</h1>
```

## Testing

```typescript
describe('BookService', () => {
  let service: BookService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BookService]
    });
    service = TestBed.inject(BookService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should fetch books', () => {
    const mockBooks = [{ id: 1, title: 'Test' }];
    
    service.getList().subscribe(books => {
      expect(books).toEqual(mockBooks);
    });

    const req = httpMock.expectOne('/api/app/books');
    req.flush(mockBooks);
  });
});
```

## Performance Optimization

### Change Detection
- Use OnPush strategy everywhere possible
- Avoid function calls in templates
- Use pure pipes
- Implement trackBy for lists

### Lazy Loading
```typescript
const routes: Routes = [
  {
    path: 'books',
    loadChildren: () => import('./books/books.module')
      .then(m => m.BooksModule)
  }
];
```

### Bundle Optimization
- Use standalone components
- Implement lazy loading
- Use dynamic imports
- Tree-shake unused code

## Security Best Practices

1. **Input Validation**: Always validate user input
2. **Sanitization**: Use DomSanitizer when needed
3. **XSS Prevention**: Leverage Angular's built-in protection
4. **Authentication**: Use ABP's auth system
5. **Authorization**: Check permissions properly
6. **Data Protection**: Never expose sensitive data in client

## Code Quality Checklist

Before submitting code, ensure:
- [ ] TypeScript strict mode enabled
- [ ] No `any` types used
- [ ] OnPush change detection applied
- [ ] Proper unsubscription implemented
- [ ] Error handling in place
- [ ] Unit tests written
- [ ] Localization keys used (no hardcoded text)
- [ ] Permission checks added
- [ ] Accessibility attributes included
- [ ] Performance optimized

## File Organization (Nx Workspace)

```
libs/
  feature-name/
    src/
      lib/
        components/
          component-name/
            component-name.component.ts
            component-name.component.html
            component-name.component.scss
            component-name.component.spec.ts
        services/
        models/
        state/
        guards/
        pipes/
        directives/
      index.ts (public API)
```

## Common Patterns

### Smart/Dumb Components
- **Smart**: Container with business logic, state management
- **Dumb**: Presentational with @Input/@Output, OnPush

### Service Layer
- **API Services**: Backend communication
- **Business Services**: Business logic
- **Utility Services**: Helper functions

## Anti-Patterns to Avoid

❌ Using `any` type
❌ Forgetting to unsubscribe
❌ Complex logic in templates
❌ Nested subscriptions
❌ Direct state mutation
❌ Missing error handling
❌ Hardcoded strings
❌ Skipping unit tests

## Additional Resources

- Angular Style Guide: https://angular.io/guide/styleguide
- ABP Framework Docs: https://docs.abp.io
- RxJS Documentation: https://rxjs.dev
- Nx Documentation: https://nx.dev
- NGXS Documentation: https://www.ngxs.io

Follow these guidelines consistently to produce high-quality, maintainable Angular applications with ABP Framework.
