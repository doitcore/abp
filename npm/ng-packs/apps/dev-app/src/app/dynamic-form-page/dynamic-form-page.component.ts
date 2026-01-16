import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { DynamicFormComponent, FormFieldConfig } from '@abp/ng.components/dynamic-form';
import { FormConfigService } from './form-config.service';

@Component({
    selector: 'app-dynamic-form-page',
    templateUrl: './dynamic-form-page.component.html',
    imports: [DynamicFormComponent],
})
export class DynamicFormPageComponent implements OnInit {
    @ViewChild(DynamicFormComponent, { static: false }) dynamicFormComponent: DynamicFormComponent;
    protected readonly formConfigService = inject(FormConfigService);

    formFields: FormFieldConfig[] = [];

    ngOnInit() {
        this.formConfigService.getFormConfig().subscribe(config => {
            this.formFields = config;
        });
    }

    submit(val) {
        console.log('submit', val);
        this.dynamicFormComponent.resetForm();
    }
}
