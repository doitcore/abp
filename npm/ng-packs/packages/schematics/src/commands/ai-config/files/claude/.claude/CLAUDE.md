# Angular & ABP Framework Development Rules for Claude

## Project Context
This is an Angular application built with the ABP Framework. Follow these rules to generate high-quality, maintainable code.

## Angular Best Practices

### Component Development
- Use OnPush change detection strategy by default
- Implement OnDestroy and unsubscribe from observables
- Keep components focused on presentation logic
- Use smart/dumb component pattern
- Prefer standalone components in new code
- Use proper TypeScript typing, avoid `any`

### Service Development
- Make services injectable with `providedIn: 'root'` when possible
- Use dependency injection properly
- Handle errors appropriately with RxJS operators
- Return observables for async operations
- Keep services focused on single responsibility

### RxJS Best Practices
- Use proper operators: `switchMap`, `mergeMap`, `concatMap`, `exhaustMap`
- Always unsubscribe using `takeUntil`, `take`, or async pipe
- Avoid nested subscriptions
- Use `shareReplay` for shared streams
- Handle errors with `catchError`

### Template Best Practices
- Use async pipe for observables
- Avoid complex logic in templates
- Use trackBy with *ngFor
- Use proper change detection
- Follow accessibility guidelines (ARIA attributes)

## ABP Framework Specific Rules

### Module Structure
- Follow ABP's modular architecture
- Use feature modules appropriately
- Leverage ABP's configuration services
- Use ABP's localization system

### State Management
- Use ABP's state management patterns
- Leverage NGXS for complex state
- Use ABP's store decorators properly

### API Integration
- Use ABP's generated proxy services
- Follow ABP's REST API conventions
- Handle ABP's error responses
- Use ABP's permission system

### Localization
- Use ABP's localization pipes and services
- Define localization keys in resource files
- Follow ABP's localization naming conventions

### Authentication & Authorization
- Use ABP's auth guards
- Leverage permission directives
- Handle ABP's multi-tenancy

## Code Style
- Follow Angular style guide
- Use meaningful variable and function names
- Add JSDoc comments for complex logic
- Keep functions small and focused
- Use TypeScript strict mode
- Format code with Prettier

## Testing
- Write unit tests for services and components
- Use Jest for testing
- Mock dependencies properly
- Aim for good test coverage
- Test error scenarios

## File Organization
- Follow Nx workspace conventions
- Use proper folder structure
- Group related files together
- Use barrel exports (index.ts)

## Performance
- Lazy load feature modules
- Use OnPush change detection
- Optimize bundle size
- Use production builds
- Implement proper caching strategies

## Security
- Sanitize user inputs
- Use proper Content Security Policy
- Follow OWASP guidelines
- Validate data on client and server

## Git Practices
- Write meaningful commit messages
- Keep commits atomic
- Follow conventional commits
- Create feature branches

When generating code, always consider these rules and the context of the ABP Framework and Angular ecosystem.
