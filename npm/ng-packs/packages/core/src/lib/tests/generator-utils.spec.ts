import { generateHash, generatePassword, uuid } from '../utils/generator-utils';
import { ConfigStateService } from '../services';

describe('GeneratorUtils', () => {
  describe('#uuid', () => {
    test('should generate a uuid', () => {
      const result = uuid();
      expect(typeof result).toBe('string');
      expect(result).toHaveLength(36);
      expect(result).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/);
    });

    test('should generate different uuids', () => {
      const uuid1 = uuid();
      const uuid2 = uuid();
      expect(uuid1).not.toBe(uuid2);
    });
  });

  describe('#generateHash', () => {
    test('should generate a hash', () => {
      const hash = generateHash('some content \n with second line');
      expect(hash).toBe(1112440527);
    });

    test('should generate consistent hash for same input', () => {
      const input = 'test string';
      const hash1 = generateHash(input);
      const hash2 = generateHash(input);
      expect(hash1).toBe(hash2);
    });

    test('should generate different hashes for different inputs', () => {
      const hash1 = generateHash('test1');
      const hash2 = generateHash('test2');
      expect(hash1).not.toBe(hash2);
    });
  });

  describe('#generatePassword', () => {
    const lowers = 'abcdefghjkmnpqrstuvwxyz';
    const uppers = 'ABCDEFGHJKMNPQRSTUVWXYZ';
    const numbers = '23456789';
    const specials = '!*_#/+-.';

    test.each`
      passedPasswordLength | actualPasswordLength
      ${Infinity}          | ${128}
      ${129}               | ${128}
      ${10}                | ${10}
      ${7}                 | ${7}
      ${6}                 | ${6}
      ${5}                 | ${5}
      ${4}                 | ${4}
      ${2}                 | ${4}
      ${0}                 | ${4}
      ${undefined}         | ${8}
    `(
      'should generate password with length $actualPasswordLength when passed $passedPasswordLength',
      ({ passedPasswordLength, actualPasswordLength }) => {
        const password = generatePassword(undefined, passedPasswordLength);
        expect(password).toHaveLength(actualPasswordLength);
        

        expect(hasChar(lowers, password)).toBe(true);
        expect(hasChar(uppers, password)).toBe(true);
        expect(hasChar(numbers, password)).toBe(true);
        expect(hasChar(specials, password)).toBe(true);
      },
    );

    test('should generate different passwords', () => {
      const password1 = generatePassword(undefined, 8);
      const password2 = generatePassword(undefined, 8);
      expect(password1).not.toBe(password2);
    });

    test('should generate password with injector', () => {
      const mockConfigState = {
        getSettings: jest.fn().mockReturnValue({
          'Abp.Identity.Password.RequiredLength': '12'
        })
      };

      const mockInjector = {
        get: jest.fn().mockReturnValue(mockConfigState)
      };

      const password = generatePassword(mockInjector as any);
      expect(password).toHaveLength(12);
      expect(mockInjector.get).toHaveBeenCalledWith(ConfigStateService);
    });
  });
});

function hasChar(charSet: string, password: string): boolean {
  return charSet.split('').some(char => password.indexOf(char) > -1);
}
