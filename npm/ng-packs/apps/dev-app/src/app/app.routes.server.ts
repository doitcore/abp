import { RenderMode, ServerRoute } from '@angular/ssr';

export const appServerRoutes: ServerRoute[] = [
  {
    path: '',
    renderMode: RenderMode.Server,
  },
  {
    path: 'account',
    renderMode: RenderMode.Server,
  },
  {
    path: 'identity',
    renderMode: RenderMode.Server,
  },
  {
    path: 'tenant-management',
    renderMode: RenderMode.Server,
  },
  {
    path: 'setting-management',
    renderMode: RenderMode.Server,
  },
  {
    path: '**',
    renderMode: RenderMode.Server,
  },
];
