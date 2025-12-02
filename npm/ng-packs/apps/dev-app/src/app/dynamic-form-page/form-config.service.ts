import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FormFieldConfig } from '@abp/ng.components/dynamic-form';

@Injectable({
    providedIn: 'root',
})
export class FormConfigService {
    protected readonly http = inject(HttpClient);

    getFormConfig(): Observable<FormFieldConfig[]> {
        return this.http.get<FormFieldConfig[]>('/assets/form-config.json');
    }
}
