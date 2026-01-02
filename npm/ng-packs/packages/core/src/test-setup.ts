import 'zone.js';
import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

// Initialize Angular testing environment
getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting(),
);

// Mock window.location for test environment
Object.defineProperty(window, 'location', {
  value: {
    href: 'http://localhost:4200',
    origin: 'http://localhost:4200',
    pathname: '/',
    search: '',
    hash: '',
  },
  writable: true,
});
