# App Services vs Domain Services: Deep Dive into Two Core Service Types in ABP Framework

In ABP's layered architecture, we frequently encounter two types of services that appear similar but serve distinctly different purposes: Application Services and Domain Services. Understanding the differences between them is crucial for building clear and maintainable enterprise applications.

## Architectural Positioning

In ABP's layered architecture:

- **Application Services** reside in the application layer and are responsible for coordinating use case execution
- **Domain Services** reside in the domain layer and are responsible for implementing core business logic

This layered design follows Domain-Driven Design (DDD) principles, ensuring clear separation of business logic and system maintainability.

## Application Services: Use Case Orchestrators

### Core Responsibilities

Application Services are stateless services primarily used to implement application use cases. They act as a bridge between the UI layer and domain layer, responsible for:

- **Use Case Orchestration**: Organizing and coordinating multiple domain objects to complete specific business use cases
- **Data Transformation**: Handling conversion between DTOs and domain objects
- **Authorization**: Checking user permissions and access control
- **Transaction Management**: Ensuring operation atomicity

### Design Principles

1. **DTO Boundaries**: Application service methods should only accept and return DTOs, never directly expose domain entities
2. **Use Case Oriented**: Each method should correspond to a clear user use case
3. **Thin Layer Design**: Avoid implementing complex business logic in application services

### Typical Execution Flow

A standard application service method typically follows this pattern:

```csharp
public async Task<BookDto> CreateBookAsync(CreateBookDto input)
{
    // 1. Validate permissions
    await CheckCreatePermissionAsync();
    
    // 2. Get or validate related data
    var author = await _authorRepository.GetAsync(input.AuthorId);
    
    // 3. Call domain service to execute business logic
    var book = await _bookManager.CreateAsync(input.Title, author, input.Price);
    
    // 4. Persist changes
    await _bookRepository.InsertAsync(book);
    
    // 5. Return DTO
    return ObjectMapper.Map<Book, BookDto>(book);
}
```

### Integration Services: Specialized Application Services

It's worth mentioning that ABP also provides a special type of application service—Integration Services. They are used for:

- **Inter-service Communication**: Service-to-service calls in microservice architectures
- **Module Integration**: Inter-module communication in large monolithic applications
- **Internal APIs**: Internal interfaces not exposed to the public

Integration services are marked with the `[IntegrationService]` attribute and use the `/integration-api` route prefix by default, making it easy to implement access control at the gateway layer.

We have a community article dedicated to integration services: [Integration Services Explained — What they are, when to use them, and how they behave](https://abp.io/community/articles/integration-services-explained-what-they-are-when-to-use-lienmsy8)

## Domain Services: Guardians of Business Logic

### Core Responsibilities

Domain Services implement core business logic and are particularly suitable for:

- **Cross-Aggregate Operations**: Business logic that needs to operate on multiple aggregate roots
- **Complex Business Rules**: Complex logic that doesn't fit within a single entity
- **External Dependencies**: Business operations that need to access repositories or external services

### Design Principles

1. **Domain Object Interaction**: Method parameters and return values should be domain objects (entities, value objects)
2. **Business Logic Focus**: Focus on implementing pure business rules
3. **Stateless Design**: Maintain the stateless nature of services

### Implementation Example

```csharp
public class IssueManager : DomainService
{
    private readonly IRepository<Issue, Guid> _issueRepository;
    
    public async Task AssignAsync(Issue issue, AppUser user)
    {
        // Business rule: Check user's unfinished task count
        var openIssueCount = await _issueRepository.GetCountAsync(
            i => i.AssignedUserId == user.Id && !i.IsClosed);
            
        if (openIssueCount >= 3)
        {
            throw new BusinessException("IssueTracking:ConcurrentOpenIssueLimit");
        }
        
        // Execute assignment logic
        issue.AssignedUserId = user.Id;
        issue.AssignedDate = Clock.Now;
    }
}
```

## Key Differences Comparison

| Dimension | Application Services | Domain Services |
|-----------|---------------------|-----------------|
| **Layer Position** | Application Layer | Domain Layer |
| **Primary Responsibility** | Use Case Orchestration | Business Logic Implementation |
| **Data Interaction** | DTOs | Domain Objects |
| **Callers** | UI Layer/Clients | Application Services/Other Domain Services |
| **Authorization** | Responsible for permission checks | No authorization logic |
| **Transaction Management** | Manages transaction boundaries | Participates in transactions but doesn't manage |
| **Naming Convention** | `*AppService` | `*Manager` or `*Service` |

## Collaboration Patterns in Practice

In real-world development, these two types of services typically work together:

```csharp
// Application Service
public class BookAppService : ApplicationService
{
    private readonly BookManager _bookManager;
    private readonly IRepository<Book> _bookRepository;
    
    public async Task<BookDto> UpdatePriceAsync(Guid id, decimal newPrice)
    {
        // Application service handles: permission checks, data retrieval, DTO conversion
        await CheckUpdatePermissionAsync();
        
        var book = await _bookRepository.GetAsync(id);
        
        // Delegate to domain service for business logic
        await _bookManager.ChangePriceAsync(book, newPrice);
        
        await _bookRepository.UpdateAsync(book);
        
        return ObjectMapper.Map<Book, BookDto>(book);
    }
}

// Domain Service
public class BookManager : DomainService
{
    public async Task ChangePriceAsync(Book book, decimal newPrice)
    {
        // Domain service focuses on business rules
        if (newPrice <= 0)
        {
            throw new BusinessException("Book:InvalidPrice");
        }
        
        if (book.IsDiscounted && newPrice > book.OriginalPrice)
        {
            throw new BusinessException("Book:DiscountedPriceCannotExceedOriginal");
        }
        
        book.ChangePrice(newPrice);
    }
}
```

## Best Practice Recommendations

### Application Services
- Create a corresponding application service for each aggregate root
- Use clear naming conventions (e.g., `IBookAppService`)
- Implement standard CRUD operation methods (`GetAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`)
- Avoid inter-application service calls

### Domain Services
- Use the `Manager` suffix for naming (e.g., `BookManager`)
- Only define state-changing methods, avoid query methods
- Throw business exceptions with clear error codes
- Keep methods pure, avoid involving user context

## Summary

Application Services and Domain Services each have their distinct roles in the ABP framework: the former serves as use case orchestrators, handling external interactions and data transformation; the latter serves as implementers of business logic, ensuring proper execution of core rules. Correctly understanding and applying these two service patterns is key to building high-quality ABP applications.

Through clear separation of responsibilities, we can not only build more maintainable code but also flexibly switch between monolithic and microservice architectures—this is precisely the elegance of ABP framework design.

## References

- [Application Services](https://abp.io/docs/latest/framework/architecture/domain-driven-design/application-services)
- [Integration Services](https://abp.io/docs/latest/framework/api-development/integration-services)
- [Domain Services](https://abp.io/docs/latest/framework/architecture/domain-driven-design/domain-services)
