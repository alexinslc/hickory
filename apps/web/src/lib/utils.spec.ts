import { cn } from './utils';

describe('utils', () => {
  describe('cn', () => {
    it('should merge class names correctly', () => {
      const result = cn('text-red-500', 'bg-blue-500');
      expect(result).toBe('text-red-500 bg-blue-500');
    });

    it('should handle conditional classes', () => {
      const condition = false;
      const result = cn('base-class', condition && 'hidden-class', 'visible-class');
      expect(result).toBe('base-class visible-class');
    });

    it('should handle Tailwind class conflicts', () => {
      const result = cn('p-4', 'p-8');
      expect(result).toBe('p-8');
    });

    it('should handle undefined and null values', () => {
      const result = cn('text-sm', undefined, null, 'font-bold');
      expect(result).toBe('text-sm font-bold');
    });

    it('should merge array of classes', () => {
      const result = cn(['text-sm', 'font-bold'], 'text-blue-500');
      expect(result).toBe('text-sm font-bold text-blue-500');
    });
  });
});
