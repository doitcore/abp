import { Environment } from '@abp/ng.core';
import {
  EnvironmentProviders,
  makeEnvironmentProviders,
  Provider,
} from '@angular/core';
import { LOGO_APP_NAME_TOKEN, LOGO_URL_TOKEN } from '../tokens/logo.token';

export enum LpxLogoFeatureKind {
  Options,
}

export interface LpxLogoFeature<KindT extends LpxLogoFeatureKind> {
  ɵkind: KindT;
  ɵproviders: (Provider | EnvironmentProviders)[];
}

function makeLpxLogoFeature<KindT extends LpxLogoFeatureKind>(
  kind: KindT,
  providers: (Provider | EnvironmentProviders)[],
): LpxLogoFeature<KindT> {
  return {
    ɵkind: kind,
    ɵproviders: providers,
  };
}

export function withEnvironmentOptions(
  options = {} as Environment,
): LpxLogoFeature<LpxLogoFeatureKind.Options> {
  const { name, logoUrl } = options.application || {};

  return makeLpxLogoFeature(LpxLogoFeatureKind.Options, [
    {
      provide: LOGO_URL_TOKEN,
      useValue: logoUrl || '',
    },
    {
      provide: LOGO_APP_NAME_TOKEN,
      useValue: name || 'ProjectName',
    },
  ]);
}

export function provideLogo(
  ...features: LpxLogoFeature<LpxLogoFeatureKind>[]
): EnvironmentProviders {
  const providers: (Provider | EnvironmentProviders)[] = [];
  features.forEach(({ ɵproviders }) => providers.push(...ɵproviders));
  return makeEnvironmentProviders(providers);
}
