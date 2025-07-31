import { Component, ComponentRef, NgModule } from '@angular/core';
import { createServiceFactory, SpectatorService } from '@ngneat/spectator';
import { ContentProjectionService } from '../services';
import { PROJECTION_STRATEGY } from '../strategies';

describe('ContentProjectionService', () => {
  @Component({ template: '<div class="foo">bar</div>', standalone: true })
  class TestComponent {}

  @Component({ 
    template: '<div class="context">{{ contextData }}</div>', 
    standalone: true 
  })
  class ContextComponent {
    contextData: string = '';
  }

  @NgModule({
    imports: [TestComponent, ContextComponent],
  })
  class TestModule {}

  let componentRef: ComponentRef<TestComponent>;
  let spectator: SpectatorService<ContentProjectionService>;
  const createService = createServiceFactory({
    service: ContentProjectionService,
    imports: [TestModule],
  });

  beforeEach(() => (spectator = createService()));

  afterEach(() => {
    if (componentRef) {
      componentRef.destroy();
    }
    const elements = document.querySelectorAll('ng-component');
    elements.forEach(el => el.remove());
  });

  describe('#projectContent', () => {
    it('should call injectContent of given projectionStrategy and return what it returns for AppendComponentToBody', () => {
      const strategy = PROJECTION_STRATEGY.AppendComponentToBody(TestComponent);
      componentRef = spectator.service.projectContent(strategy);
      const foo = document.querySelector('body > ng-component > div.foo');

      expect(componentRef).toBeInstanceOf(ComponentRef);
      expect(foo.textContent).toBe('bar');
    });

    it('should handle component with context for AppendComponentToBody', () => {
      const strategy = PROJECTION_STRATEGY.AppendComponentToBody(
        ContextComponent, 
        { contextData: 'context test' }
      );
      componentRef = spectator.service.projectContent(strategy);
      
      const contextDiv = document.querySelector('body > ng-component > div.context');
      expect(componentRef).toBeInstanceOf(ComponentRef);
      expect(contextDiv.textContent).toBe('context test');
    });

    it('should return ComponentRef when projecting component', () => {
      const strategy = PROJECTION_STRATEGY.AppendComponentToBody(TestComponent);
      const result = spectator.service.projectContent(strategy);
      
      expect(result).toBeInstanceOf(ComponentRef);
      expect(result.componentType).toBe(TestComponent);
    });

    it('should work with different projection strategies', () => {
      const appendStrategy = PROJECTION_STRATEGY.AppendComponentToBody(TestComponent);
      const appendResult = spectator.service.projectContent(appendStrategy);
      
      expect(appendResult).toBeInstanceOf(ComponentRef);
      
      appendResult.destroy();
      
      const contextStrategy = PROJECTION_STRATEGY.AppendComponentToBody(
        ContextComponent, 
        { contextData: 'test context' }
      );
      const contextResult = spectator.service.projectContent(contextStrategy);
      
      expect(contextResult).toBeInstanceOf(ComponentRef);
      expect(contextResult.componentType).toBe(ContextComponent);
    });
  });
});
