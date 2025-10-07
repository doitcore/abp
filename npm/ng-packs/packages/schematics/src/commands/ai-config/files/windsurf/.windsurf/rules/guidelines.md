# Windsurf AI Development Guidelines - Angular & ABP Framework

## Project Context
This is an enterprise-grade Angular application built on the ABP Framework, using Nx for workspace management and NGXS for state management. Follow these comprehensive guidelines to generate production-ready code.

---

## 🎯 Core Development Principles

### 1. Type Safety First
- **Always** use TypeScript strict mode
- **Never** use `any` type - use `unknown` if type is truly unknown
- Define interfaces and types for all data structures
- Use proper generic types

### 2. Performance Optimization
- Use OnPush change detection strategy by default
- Implement lazy loading for feature modules
- Use trackBy with *ngFor directives
- Leverage async pipe for observables
- Avoid memory leaks with proper cleanup

### 3. Code Maintainability
- Follow SOLID principles
- Write self-documenting code with clear naming
- Keep functions small (<20 lines ideally)
- Add JSDoc comments for complex logic
- Use meaningful variable and function names

### 4. Security
- Validate all user inputs
- Sanitize data when necessary (DomSanitizer)
- Use ABP's permission system
- Never expose sensitive data in client code
- Follow OWASP security guidelines

### 5. Accessibility
- Include ARIA attributes
- Support keyboard navigation
- Use semantic HTML
- Follow WCAG 2.1 AA standards

---

## 📦 Angular Component Architecture

### Standard Component Structure

```typescript
import { 
  ChangeDetectionStrategy, 
  Component, 
  OnDestroy, 
  OnInit, 
  inject 
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-feature-name',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './feature-name.component.html',
  styleUrls: ['./feature-name.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FeatureNameComponent implements OnInit, OnDestroy {
  // Use inject() function (Angular 14+)
  private readonly dataService = inject(DataService);
  private readonly destroy$ = new Subject<void>();

  // Observable streams with $ suffix
  data$ = this.dataService.getData();

  ngOnInit(): void {
    // Initialization logic
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### Component Best Practices
✅ **DO:**
- Use OnPush change detection
- Implement OnDestroy for cleanup
- Use standalone components for new code
- Prefer async pipe over manual subscriptions
- Use readonly for immutable properties
- Use inject() function for dependency injection

❌ **DON'T:**
- Put business logic in components
- Mutate @Input() properties
- Forget to unsubscribe from observables
- Use function calls in templates
- Use nested subscriptions

---

## 🔧 Service Development

### Service Pattern

```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, retry, shareReplay, throwError } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DataService {
  private readonly http = inject(HttpClient);
  private readonly cache$ = new Map<string, Observable<any>>();

  getData(id: string): Observable<Data> {
    // Implement caching
    if (!this.cache$.has(id)) {
      this.cache$.set(
        id,
        this.http.get<Data>(`/api/data/${id}`).pipe(
          retry(2),
          catchError(this.handleError),
          shareReplay(1)
        )
      );
    }
    return this.cache$.get(id)!;
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('Service error:', error);
    // Log to monitoring service here
    return throwError(() => new Error('Operation failed. Please try again.'));
  }
}
```

### Service Best Practices
- Use `providedIn: 'root'` for singleton services
- Return Observables for async operations
- Implement proper error handling
- Use caching strategies when appropriate
- Keep services focused (Single Responsibility)

---

## 🌊 RxJS Patterns & Operators

### Operator Decision Matrix

| Operator | Use Case | Behavior |
|----------|----------|----------|
| **switchMap** | Search, navigation | Cancels previous, emits latest |
| **mergeMap** | Parallel operations | Runs all concurrently |
| **concatMap** | Sequential operations | Maintains order, waits for completion |
| **exhaustMap** | Form submission, clicks | Ignores new until current completes |

### Subscription Management

```typescript
export class ExampleComponent implements OnDestroy {
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    // Pattern 1: takeUntil
    this.service.getData()
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => this.handleData(data));

    // Pattern 2: take(1) for single emission
    this.service.getConfig()
      .pipe(take(1))
      .subscribe(config => this.config = config);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### Template Usage (Preferred)

```typescript
// Component
data$ = this.service.getData().pipe(
  catchError(error => {
    this.handleError(error);
    return of([]);
  })
);

// Template
<div *ngIf="data$ | async as data">
  {{ data.name }}
</div>
```

---

## 🏗️ ABP Framework Integration

### 1. Localization System

```typescript
// Component
import { LocalizationService } from '@abp/ng.core';

export class MyComponent {
  private readonly localization = inject(LocalizationService);

  readonly texts = {
    title: this.localization.instant('::PageTitle'),
    save: this.localization.instant('::Save'),
    cancel: this.localization.instant('::Cancel')
  };
}

// Template
<h1>{{ '::PageTitle' | abpLocalization }}</h1>
<button>{{ '::Save' | abpLocalization }}</button>
```

### 2. Permission System

```typescript
// Template
<button 
  *abpPermission="'BookStore.Books.Create'"
  (click)="createBook()">
  {{ '::NewBook' | abpLocalization }}
</button>

// Component
import { ConfigStateService } from '@abp/ng.core';

export class BookListComponent {
  private readonly config = inject(ConfigStateService);

  canEdit$ = this.config.getGrantedPolicy$('BookStore.Books.Edit');
  canDelete$ = this.config.getGrantedPolicy$('BookStore.Books.Delete');

  checkPermission(): boolean {
    return this.config.getGrantedPolicy('BookStore.Books.Create');
  }
}
```

### 3. API Proxy Services

```typescript
// ✅ ALWAYS use generated proxy services
import { BookService } from '@proxy/books';
import { GetBooksInput } from '@proxy/books/models';

export class BookListComponent {
  private readonly bookService = inject(BookService);

  books$ = this.bookService.getList({
    maxResultCount: 10,
    skipCount: 0
  });

  createBook(input: CreateBookDto): void {
    this.bookService.create(input).pipe(
      take(1),
      catchError(this.handleError)
    ).subscribe(() => this.refreshList());
  }
}

// ❌ DON'T create manual HTTP calls for ABP APIs
```

### 4. State Management with NGXS

```typescript
// Actions
export class GetBooks {
  static readonly type = '[Books] Get Books';
  constructor(public payload: GetBooksInput) {}
}

export class CreateBook {
  static readonly type = '[Books] Create Book';
  constructor(public payload: CreateBookDto) {}
}

// State
export interface BooksStateModel {
  books: BookDto[];
  loading: boolean;
  error: string | null;
  totalCount: number;
}

@State<BooksStateModel>({
  name: 'books',
  defaults: {
    books: [],
    loading: false,
    error: null,
    totalCount: 0
  }
})
@Injectable()
export class BooksState {
  private readonly bookService = inject(BookService);

  @Selector()
  static books(state: BooksStateModel): BookDto[] {
    return state.books;
  }

  @Selector()
  static loading(state: BooksStateModel): boolean {
    return state.loading;
  }

  @Selector()
  static totalCount(state: BooksStateModel): number {
    return state.totalCount;
  }

  @Action(GetBooks)
  getBooks(ctx: StateContext<BooksStateModel>, action: GetBooks) {
    ctx.patchState({ loading: true, error: null });
    
    return this.bookService.getList(action.payload).pipe(
      tap(response => {
        ctx.patchState({
          books: response.items,
          totalCount: response.totalCount,
          loading: false
        });
      }),
      catchError(error => {
        ctx.patchState({ 
          loading: false, 
          error: error.message 
        });
        return throwError(() => error);
      })
    );
  }

  @Action(CreateBook)
  createBook(ctx: StateContext<BooksStateModel>, action: CreateBook) {
    return this.bookService.create(action.payload).pipe(
      tap(book => {
        const state = ctx.getState();
        ctx.patchState({
          books: [...state.books, book],
          totalCount: state.totalCount + 1
        });
      })
    );
  }
}
```

### 5. Multi-Tenancy Support

```typescript
import { ConfigStateService } from '@abp/ng.core';

export class TenantAwareComponent {
  private readonly config = inject(ConfigStateService);

  get currentTenant() {
    return this.config.getOne('currentTenant');
  }

  get isTenantContext(): boolean {
    return !!this.currentTenant?.id;
  }
}
```

---

## 📝 Reactive Forms

### Form Implementation

```typescript
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomValidators } from './validators';

export class BookFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  
  bookForm!: FormGroup;

  ngOnInit(): void {
    this.bookForm = this.fb.group({
      name: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(128)
      ]],
      type: ['', Validators.required],
      publishDate: ['', [
        Validators.required,
        CustomValidators.notFutureDate
      ]],
      price: [0, [
        Validators.required,
        Validators.min(0),
        Validators.max(999999.99)
      ]],
      description: ['', Validators.maxLength(1000)]
    });
  }

  onSubmit(): void {
    if (this.bookForm.valid) {
      const formValue = this.bookForm.getRawValue();
      this.submitForm(formValue);
    } else {
      this.markFormGroupTouched(this.bookForm);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }
}
```

---

## 🎨 Template Best Practices

```html
<!-- ✅ Async pipe with null check -->
<div *ngIf="books$ | async as books; else loading">
  <div 
    *ngFor="let book of books; trackBy: trackByBookId"
    class="book-item">
    <h3>{{ book.name }}</h3>
    <p>{{ book.publishDate | date:'shortDate' }}</p>
    <p>{{ book.price | currency }}</p>
  </div>
</div>

<ng-template #loading>
  <div class="spinner">Loading...</div>
</ng-template>

<!-- ✅ Accessibility -->
<button
  type="button"
  [attr.aria-label]="'::DeleteBook' | abpLocalization"
  [attr.aria-disabled]="(canDelete$ | async) === false"
  [disabled]="(canDelete$ | async) === false"
  (click)="deleteBook(book)">
  <i class="fa fa-trash" aria-hidden="true"></i>
</button>

<!-- ✅ Permission check -->
<div *abpPermission="'BookStore.Books.Create'">
  <button (click)="createBook()">
    {{ '::NewBook' | abpLocalization }}
  </button>
</div>

<!-- ✅ Localization -->
<h1>{{ '::BookManagement' | abpLocalization }}</h1>
<p>{{ '::BookDescription' | abpLocalization:{ name: book.name } }}</p>
```

---

## 🧪 Testing Strategies

### Component Testing

```typescript
describe('BookListComponent', () => {
  let component: BookListComponent;
  let fixture: ComponentFixture<BookListComponent>;
  let mockBookService: jasmine.SpyObj<BookService>;

  beforeEach(async () => {
    mockBookService = jasmine.createSpyObj('BookService', [
      'getList',
      'create',
      'delete'
    ]);

    await TestBed.configureTestingModule({
      imports: [BookListComponent],
      providers: [
        { provide: BookService, useValue: mockBookService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BookListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load books on init', () => {
    const mockBooks = [
      { id: '1', name: 'Book 1', price: 10 },
      { id: '2', name: 'Book 2', price: 20 }
    ];
    
    mockBookService.getList.and.returnValue(of({
      items: mockBooks,
      totalCount: 2
    }));

    component.ngOnInit();

    expect(mockBookService.getList).toHaveBeenCalled();
  });
});
```

---

## 🚀 Performance Optimization

### 1. Change Detection Strategy
```typescript
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
```

### 2. TrackBy Functions
```typescript
trackByBookId(index: number, book: BookDto): string {
  return book.id;
}
```

### 3. Lazy Loading
```typescript
const routes: Routes = [
  {
    path: 'books',
    loadChildren: () => import('./books/books.routes')
      .then(m => m.BOOKS_ROUTES)
  }
];
```

### 4. Virtual Scrolling (for large lists)
```typescript
<cdk-virtual-scroll-viewport itemSize="50" class="list-viewport">
  <div *cdkVirtualFor="let book of books; trackBy: trackByBookId">
    {{ book.name }}
  </div>
</cdk-virtual-scroll-viewport>
```

---

## 📁 File Structure (Nx Workspace)

```
libs/
  books/
    feature/
      src/
        lib/
          components/
            book-list/
            book-form/
            book-detail/
          services/
          state/
          guards/
          books-feature.routes.ts
        index.ts
    data-access/
      src/
        lib/
          services/
          models/
        index.ts
    ui/
      src/
        lib/
          components/
        index.ts
```

---

## ✅ Quality Checklist

Before committing code, verify:
- [ ] TypeScript strict mode compliance
- [ ] No `any` types
- [ ] OnPush change detection
- [ ] Proper unsubscription
- [ ] Error handling implemented
- [ ] Unit tests written
- [ ] Localization keys used
- [ ] Permission checks added
- [ ] Accessibility attributes included
- [ ] Performance optimized (trackBy, lazy loading)
- [ ] Security validated
- [ ] Code formatted (Prettier)
- [ ] Linting passed

---

## 🚫 Common Anti-Patterns

| Anti-Pattern | Why It's Bad | Better Approach |
|--------------|--------------|-----------------|
| Using `any` | Loses type safety | Use proper types or `unknown` |
| No unsubscribe | Memory leaks | Use takeUntil or async pipe |
| Logic in templates | Hard to test | Move to component/service |
| Nested subscriptions | Hard to maintain | Use RxJS operators |
| Direct state mutation | Breaks change detection | Use immutable patterns |
| Missing error handling | Poor UX | Always handle errors |
| Hardcoded strings | Not localizable | Use localization system |

---

## 📚 Resources

- [Angular Style Guide](https://angular.io/guide/styleguide)
- [ABP Framework Documentation](https://docs.abp.io)
- [RxJS Documentation](https://rxjs.dev)
- [Nx Documentation](https://nx.dev)
- [NGXS Documentation](https://www.ngxs.io)

---

Follow these guidelines to build maintainable, performant, and secure Angular applications with ABP Framework.
