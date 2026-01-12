'use client';

import { useGetAllCategories } from '@/lib/queries/categories';

interface CategorySelectorProps {
  value?: string;
  onChange: (categoryId: string | undefined) => void;
  disabled?: boolean;
  error?: string;
}

export function CategorySelector({ value, onChange, disabled, error }: CategorySelectorProps) {
  const { data: categories, isLoading } = useGetAllCategories();

  return (
    <div className="space-y-2">
      <label htmlFor="category" className="block text-sm font-medium text-gray-700">
        Category
      </label>
      <select
        id="category"
        value={value || ''}
        onChange={(e) => onChange(e.target.value || undefined)}
        disabled={disabled || isLoading}
        className={`
          mt-1 block w-full rounded-md border-gray-300 shadow-sm
          focus:border-blue-500 focus:ring-blue-500 sm:text-sm
          disabled:bg-gray-100 disabled:cursor-not-allowed
          ${error ? 'border-red-300' : ''}
        `}
      >
        <option value="">-- No Category --</option>
        {categories?.map((category) => (
          <option key={category.id} value={category.id}>
            {category.name}
          </option>
        ))}
      </select>
      {error && <p className="text-sm text-red-600">{error}</p>}
    </div>
  );
}
