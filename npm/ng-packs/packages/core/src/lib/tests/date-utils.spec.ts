import { TestBed } from '@angular/core/testing';
import { ConfigStateService } from '../services';
import { getShortDateFormat, getShortDateShortTimeFormat, getShortTimeFormat } from '../utils';

const dateTimeFormat = {
  calendarAlgorithmType: 'SolarCalendar',
  dateSeparator: '/',
  dateTimeFormatLong: 'dddd, MMMM d, yyyy',
  fullDateTimePattern: 'dddd, MMMM d, yyyy h:mm:ss tt',
  longTimePattern: 'h:mm:ss tt',
  shortDatePattern: 'M/d/yyyy',
  shortTimePattern: 'h:mm tt',
};

describe('Date Utils', () => {
  let config: ConfigStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: ConfigStateService,
          useValue: {
            getDeep: jest.fn(),
          },
        },
      ],
    });
    config = TestBed.inject(ConfigStateService);
  });

  describe('#getShortDateFormat', () => {
    test('should get the short date format from ConfigStateService and return it', () => {
      const getDeepSpy = jest.spyOn(config, 'getDeep');
      getDeepSpy.mockReturnValueOnce(dateTimeFormat);

      expect(getShortDateFormat(config)).toBe('M/d/yyyy');
      expect(getDeepSpy).toHaveBeenCalledWith('localization.currentCulture.dateTimeFormat');
    });
  });

  describe('#getShortTimeFormat', () => {
    test('should get the short time format from ConfigStateService and return it', () => {
      const getDeepSpy = jest.spyOn(config, 'getDeep');
      getDeepSpy.mockReturnValueOnce(dateTimeFormat);

      expect(getShortTimeFormat(config)).toBe('h:mm a');
      expect(getDeepSpy).toHaveBeenCalledWith('localization.currentCulture.dateTimeFormat');
    });

    test('should handle null shortTimePattern', () => {
      const getDeepSpy = jest.spyOn(config, 'getDeep');
      getDeepSpy.mockReturnValueOnce({ ...dateTimeFormat, shortTimePattern: null });

      expect(getShortTimeFormat(config)).toBeUndefined();
    });

    test('should handle undefined shortTimePattern', () => {
      const getDeepSpy = jest.spyOn(config, 'getDeep');
      getDeepSpy.mockReturnValueOnce({ ...dateTimeFormat, shortTimePattern: undefined });

      expect(getShortTimeFormat(config)).toBeUndefined();
    });
  });

  describe('#getShortDateShortTimeFormat', () => {
    test('should get the short date time format from ConfigStateService and return it', () => {
      const getDeepSpy = jest.spyOn(config, 'getDeep');
      getDeepSpy.mockReturnValueOnce(dateTimeFormat);

      expect(getShortDateShortTimeFormat(config)).toBe('M/d/yyyy h:mm a');
      expect(getDeepSpy).toHaveBeenCalledWith('localization.currentCulture.dateTimeFormat');
    });

    test('should handle null shortTimePattern', () => {
      const getDeepSpy = jest.spyOn(config, 'getDeep');
      getDeepSpy.mockReturnValueOnce({ ...dateTimeFormat, shortTimePattern: null });

      expect(getShortDateShortTimeFormat(config)).toBe('M/d/yyyy undefined');
    });

    test('should handle undefined shortTimePattern', () => {
      const getDeepSpy = jest.spyOn(config, 'getDeep');
      getDeepSpy.mockReturnValueOnce({ ...dateTimeFormat, shortTimePattern: undefined });

      expect(getShortDateShortTimeFormat(config)).toBe('M/d/yyyy undefined');
    });
  });
});
