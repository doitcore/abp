import type { GetPermissionListResultDto, UpdatePermissionsDto } from './models';
import { RestService } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PermissionsService {
  private restService = inject(RestService);

  apiName = 'AbpPermissionManagement';

  get = (providerName: string, providerKey: string) =>
    this.restService.request<any, GetPermissionListResultDto>({
      method: 'GET',
      url: '/api/permission-management/permissions',
      params: { providerName, providerKey },
    },
    { apiName: this.apiName });

  update = (providerName: string, providerKey: string, input: UpdatePermissionsDto) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/permission-management/permissions',
      params: { providerName, providerKey },
      body: input,
    },
    { apiName: this.apiName });
}
