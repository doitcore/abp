# GitHub Copilot Instructions for Angular & ABP Framework

You are an expert Angular and ABP Framework developer. Follow these guidelines when generating code suggestions.

## Angular Development Standards

### Components
- Create components with OnPush change detection strategy
- Implement lifecycle hooks properly (OnInit, OnDestroy)
- Use standalone components for new features
- Follow smart/dumb component pattern
- Unsubscribe from observables using takeUntil pattern or async pipe

Example:
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

### Services
- Use providedIn: 'root' for singleton services
- Return Observables for async operations
- Handle errors with proper RxJS operators
- Keep services focused on single responsibility

Example:
```typescript
@Injectable({ providedIn: 'root' })
export class DataService {
  constructor(private http: HttpClient) {}
  
  getData(): Observable<Data[]> {
    return this.http.get<Data[]>('/api/data').pipe(
      catchError(this.handleError)
    );
  }
}
```

### RxJS Patterns
- Use async pipe in templates instead of manual subscriptions
- Use switchMap for dependent API calls
- Use shareReplay for shared streams
- Avoid nested subscriptions

### Forms
- Use Reactive Forms over Template-driven forms
- Implement custom validators when needed
- Use FormBuilder for cleaner form creation
- Handle form validation properly

## ABP Framework Integration

### Using ABP Services
```typescript
import { ConfigStateService, LocalizationService } from '@abp/ng.core';

constructor(
  private config: ConfigStateService,
  private localization: LocalizationService
) {}
```

### Localization
```typescript
// In component
this.localization.instant('::LocalizationKey')

// In template
{{ '::LocalizationKey' | abpLocalization }}
```

### Permissions
```typescript
// In template
<button *abpPermission="'MyApp.MyPermission'">Action</button>

// In component
if (this.config.getGrantedPolicy('MyApp.MyPermission')) {
  // do something
}
```

### API Proxy Integration
- Use ABP's generated proxy services
- Don't create manual HTTP calls for ABP APIs
- Follow ABP's DTOs and service patterns

### State Management with NGXS
```typescript
@State<MyStateModel>({
  name: 'MyState',
  defaults: { items: [] }
})
@Injectable()
export class MyState {
  @Action(GetItems)
  getItems(ctx: StateContext<MyStateModel>) {
    return this.service.getItems().pipe(
      tap(items => ctx.patchState({ items }))
    );
  }
}
```

## Code Quality Standards
- Use TypeScript strict mode
- Avoid using `any` type
- Implement proper error handling
- Write meaningful variable and function names
- Add comments for complex logic
- Follow SOLID principles

## Testing
- Write unit tests for components and services
- Use Jest testing framework
- Mock dependencies properly
- Test both success and error scenarios

## File Structure
Follow Nx workspace structure:
```
libs/
  feature-name/
    src/
      lib/
        components/
        services/
        models/
        state/
```

## Performance
- Use OnPush change detection
- Lazy load feature modules
- Use trackBy in *ngFor
- Optimize bundle size
- Avoid memory leaks

## Security
- Never commit sensitive data
- Sanitize user inputs
- Use proper authentication guards
- Follow ABP's security patterns

Always prioritize code maintainability, readability, and following Angular and ABP best practices.
