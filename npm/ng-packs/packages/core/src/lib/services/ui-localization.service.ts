import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, distinctUntilChanged, switchMap, of } from 'rxjs';
import { map, catchError, shareReplay, tap } from 'rxjs/operators';
import { loadTranslations } from '@angular/localize';
import { ABP } from '../models/common';
import { LocalizationService } from './localization.service';
import { SessionStateService } from './session-state.service';
import { CORE_OPTIONS } from '../tokens/options.token';

export interface LocalizationResource {
  [resourceName: string]: Record<string, string>;
}

/**
 * Service for managing UI localizations via @angular/localize
 * Automatically loads localization files based on selected language
 * Merges with backend localizations (UI > Backend priority)
 */
@Injectable({ providedIn: 'root' })
export class UILocalizationService {
  private http = inject(HttpClient);
  private localizationService = inject(LocalizationService);
  private sessionState = inject(SessionStateService);
  private options = inject(CORE_OPTIONS);

  // Yüklenen localization'lar (culture -> resourceName -> texts)
  private loadedLocalizations$ = new BehaviorSubject<Record<string, LocalizationResource>>({});

  // Current language
  private currentLanguage$ = this.sessionState.getLanguage$();

  constructor() {
    const uiLocalization = this.options.uiLocalization;
    if (uiLocalization?.enabled) {
      // Dil değiştiğinde otomatik yükle
      this.subscribeToLanguageChanges();
    }
  }

  /**
   * Dil değişikliğini dinle ve localization dosyasını yükle
   */
  private subscribeToLanguageChanges() {
    this.currentLanguage$
      .pipe(
        distinctUntilChanged(),
        switchMap(culture => this.loadLocalizationFile(culture)),
        shareReplay(1),
      )
      .subscribe();
  }

  /**
   * Seçilen dil için localization dosyasını yükle
   * Format: /assets/localization/{culture}.json
   * JSON format: { "ResourceName": { "Key": "Value" } }
   */
  private loadLocalizationFile(culture: string) {
    const config = this.options.uiLocalization;
    if (!config?.enabled) return of(null);

    const basePath = config.basePath || '/assets/localization';
    const url = `${basePath}/${culture}.json`;

    return this.http.get<LocalizationResource>(url).pipe(
      catchError(() => {
        // Dosya yoksa sessizce devam et (backend'den gelecek)
        return of(null);
      }),
      tap(data => {
        if (data) {
          this.processLocalizationData(culture, data);
        }
      }),
    );
  }

  /**
   * Localization verisini işle:
   * 1. @angular/localize'a ekle (loadTranslations)
   * 2. ABP LocalizationService'e ekle (addLocalization)
   */
  private processLocalizationData(culture: string, data: LocalizationResource) {
    // 1. @angular/localize'a ekle
    const loadTranslationsMap: Record<string, string> = {};
    Object.entries(data).forEach(([resourceName, texts]) => {
      Object.entries(texts).forEach(([key, value]) => {
        loadTranslationsMap[`${resourceName}::${key}`] = value;
      });
    });
    loadTranslations(loadTranslationsMap);

    // 2. ABP LocalizationService'e ekle
    const abpFormat: ABP.Localization[] = [
      {
        culture,
        resources: Object.entries(data).map(([resourceName, texts]) => ({
          resourceName,
          texts,
        })),
      },
    ];
    this.localizationService.addLocalization(abpFormat);

    // 3. Cache'e ekle
    const current = this.loadedLocalizations$.value;
    current[culture] = data;
    this.loadedLocalizations$.next(current);
  }

  /**
   * Manuel olarak localization ekle (runtime'da)
   */
  addAngularLocalizeLocalization(
    culture: string,
    resourceName: string,
    translations: Record<string, string>,
  ): void {
    // @angular/localize'a ekle
    const loadTranslationsMap: Record<string, string> = {};
    Object.entries(translations).forEach(([key, value]) => {
      loadTranslationsMap[`${resourceName}::${key}`] = value;
    });
    loadTranslations(loadTranslationsMap);

    // ABP LocalizationService'e ekle
    const abpFormat: ABP.Localization[] = [
      {
        culture,
        resources: [
          {
            resourceName,
            texts: translations,
          },
        ],
      },
    ];
    this.localizationService.addLocalization(abpFormat);

    // Cache'e ekle
    const current = this.loadedLocalizations$.value;
    if (!current[culture]) {
      current[culture] = {};
    }
    if (!current[culture][resourceName]) {
      current[culture][resourceName] = {};
    }
    current[culture][resourceName] = {
      ...current[culture][resourceName],
      ...translations,
    };
    this.loadedLocalizations$.next(current);
  }

  /**
   * Yüklenen localization'ları al
   */
  getLoadedLocalizations(culture?: string): LocalizationResource {
    const lang = culture || this.sessionState.getLanguage();
    return this.loadedLocalizations$.value[lang] || {};
  }
}
