'use client';

import { useState } from 'react';
import { useGetAllCategories } from '@/lib/queries/categories';
import { useGetAllTags } from '@/lib/queries/tags';

export interface TicketFilters {
  categoryId?: string;
  tags: string[];
}

interface TicketFilterProps {
  filters: TicketFilters;
  onFiltersChange: (filters: TicketFilters) => void;
}

export function TicketFilter({ filters, onFiltersChange }: TicketFilterProps) {
  const { data: categories, isLoading: categoriesLoading } = useGetAllCategories();
  const { data: tags, isLoading: tagsLoading } = useGetAllTags();
  const [tagInput, setTagInput] = useState('');

  const handleCategoryChange = (categoryId: string) => {
    onFiltersChange({
      ...filters,
      categoryId: categoryId || undefined,
    });
  };

  const handleAddTag = (tagName: string) => {
    const trimmed = tagName.trim();
    if (trimmed && !filters.tags.includes(trimmed)) {
      onFiltersChange({
        ...filters,
        tags: [...filters.tags, trimmed],
      });
    }
    setTagInput('');
  };

  const handleRemoveTag = (tagName: string) => {
    onFiltersChange({
      ...filters,
      tags: filters.tags.filter((t) => t !== tagName),
    });
  };

  const handleClearFilters = () => {
    onFiltersChange({
      categoryId: undefined,
      tags: [],
    });
    setTagInput('');
  };

  const hasActiveFilters = filters.categoryId || filters.tags.length > 0;

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4 space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-gray-900">Filters</h3>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={handleClearFilters}
            className="text-sm text-blue-600 hover:text-blue-500"
          >
            Clear all
          </button>
        )}
      </div>

      {/* Category filter */}
      <div className="space-y-2">
        <label htmlFor="filter-category" className="block text-sm font-medium text-gray-700">
          Category
        </label>
        <select
          id="filter-category"
          value={filters.categoryId || ''}
          onChange={(e) => handleCategoryChange(e.target.value)}
          disabled={categoriesLoading}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
        >
          <option value="">All Categories</option>
          {categories?.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
      </div>

      {/* Tag filter */}
      <div className="space-y-2">
        <label htmlFor="filter-tags" className="block text-sm font-medium text-gray-700">
          Tags
        </label>
        <div className="flex gap-2">
          <input
            id="filter-tags"
            type="text"
            value={tagInput}
            onChange={(e) => setTagInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault();
                handleAddTag(tagInput);
              }
            }}
            placeholder="Add tag filter..."
            disabled={tagsLoading}
            className="block flex-1 rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
          />
          <button
            type="button"
            onClick={() => handleAddTag(tagInput)}
            disabled={!tagInput.trim() || tagsLoading}
            className="inline-flex items-center px-3 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:bg-gray-300 disabled:cursor-not-allowed"
          >
            Add
          </button>
        </div>

        {/* Available tags (quick add) */}
        {tags && tags.length > 0 && (
          <div className="flex flex-wrap gap-1 pt-1">
            {tags
              .filter((tag) => !filters.tags.includes(tag.name))
              .slice(0, 10)
              .map((tag) => (
                <button
                  key={tag.id}
                  type="button"
                  onClick={() => handleAddTag(tag.name)}
                  className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-700 hover:bg-gray-200"
                >
                  {tag.name}
                </button>
              ))}
          </div>
        )}

        {/* Selected tag filters */}
        {filters.tags.length > 0 && (
          <div className="flex flex-wrap gap-1 pt-2">
            {filters.tags.map((tag) => (
              <span
                key={tag}
                className="inline-flex items-center gap-1 rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-800"
              >
                {tag}
                <button
                  type="button"
                  onClick={() => handleRemoveTag(tag)}
                  className="inline-flex items-center justify-center hover:bg-blue-200 rounded-full p-0.5"
                >
                  <svg className="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                  <span className="sr-only">Remove {tag}</span>
                </button>
              </span>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
