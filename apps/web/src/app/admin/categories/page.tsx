'use client';

import { useState } from 'react';
import { useGetAllCategories, useCreateCategory } from '@/lib/queries/categories';
import { CreateCategoryCommand } from '@/lib/api-client';

export default function CategoriesPage() {
  const { data: categories, isLoading } = useGetAllCategories();
  const createCategory = useCreateCategory();

  const [isCreating, setIsCreating] = useState(false);
  const [formData, setFormData] = useState<CreateCategoryCommand>({
    name: '',
    description: '',
    displayOrder: 0,
    color: '#6366f1',
  });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormErrors({});

    // Basic validation
    const errors: Record<string, string> = {};
    if (!formData.name.trim()) {
      errors.name = 'Name is required';
    } else if (formData.name.length < 2 || formData.name.length > 100) {
      errors.name = 'Name must be between 2 and 100 characters';
    }
    if (formData.color && !/^#[0-9A-F]{6}$/i.test(formData.color)) {
      errors.color = 'Color must be a valid hex code (e.g., #6366f1)';
    }

    if (Object.keys(errors).length > 0) {
      setFormErrors(errors);
      return;
    }

    try {
      await createCategory.mutateAsync(formData);
      // Reset form
      setFormData({
        name: '',
        description: '',
        displayOrder: 0,
        color: '#6366f1',
      });
      setIsCreating(false);
    } catch (error) {
      console.error('Failed to create category:', error);
      setFormErrors({ submit: 'Failed to create category. Please try again.' });
    }
  };

  const handleCancel = () => {
    setIsCreating(false);
    setFormData({
      name: '',
      description: '',
      displayOrder: 0,
      color: '#6366f1',
    });
    setFormErrors({});
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="md:flex md:items-center md:justify-between mb-8">
        <div className="flex-1 min-w-0">
          <h1 className="text-2xl font-bold text-gray-900">Category Management</h1>
          <p className="mt-2 text-sm text-gray-600">
            Manage ticket categories for your help desk system
          </p>
        </div>
        <div className="mt-4 flex md:mt-0 md:ml-4">
          {!isCreating && (
            <button
              type="button"
              onClick={() => setIsCreating(true)}
              className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Create Category
            </button>
          )}
        </div>
      </div>

      {/* Create form */}
      {isCreating && (
        <div className="bg-white shadow rounded-lg p-6 mb-6">
          <h2 className="text-lg font-medium text-gray-900 mb-4">New Category</h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            {formErrors.submit && (
              <div className="rounded-md bg-red-50 p-4">
                <p className="text-sm text-red-800">{formErrors.submit}</p>
              </div>
            )}

            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                Name *
              </label>
              <input
                type="text"
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className={`mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm ${
                  formErrors.name ? 'border-red-300' : ''
                }`}
                placeholder="e.g., Hardware, Software, Network"
              />
              {formErrors.name && (
                <p className="mt-1 text-sm text-red-600">{formErrors.name}</p>
              )}
            </div>

            <div>
              <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                Description
              </label>
              <textarea
                id="description"
                rows={3}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                placeholder="Optional description for this category"
              />
            </div>

            <div>
              <label htmlFor="displayOrder" className="block text-sm font-medium text-gray-700">
                Display Order
              </label>
              <input
                type="number"
                id="displayOrder"
                value={formData.displayOrder}
                onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 0 })}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                min="0"
              />
              <p className="mt-1 text-sm text-gray-500">
                Lower numbers appear first in lists
              </p>
            </div>

            <div>
              <label htmlFor="color" className="block text-sm font-medium text-gray-700">
                Color
              </label>
              <div className="mt-1 flex items-center gap-2">
                <input
                  type="color"
                  id="color"
                  value={formData.color || '#6366f1'}
                  onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                  className="h-10 w-20 rounded-md border border-gray-300 cursor-pointer"
                />
                <input
                  type="text"
                  value={formData.color || ''}
                  onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                  placeholder="#6366f1"
                  className={`flex-1 rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm ${
                    formErrors.color ? 'border-red-300' : ''
                  }`}
                />
              </div>
              {formErrors.color && (
                <p className="mt-1 text-sm text-red-600">{formErrors.color}</p>
              )}
            </div>

            <div className="flex justify-end gap-3 pt-4">
              <button
                type="button"
                onClick={handleCancel}
                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={createCategory.isPending}
                className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:bg-gray-300 disabled:cursor-not-allowed"
              >
                {createCategory.isPending ? 'Creating...' : 'Create Category'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Categories list */}
      <div className="bg-white shadow overflow-hidden rounded-lg">
        {isLoading ? (
          <div className="p-8 text-center text-gray-500">
            Loading categories...
          </div>
        ) : categories && categories.length > 0 ? (
          <ul className="divide-y divide-gray-200">
            {categories.map((category) => (
              <li key={category.id} className="p-6">
                <div className="flex items-start justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-3 mb-2">
                      <span
                        className="inline-flex items-center rounded-md px-3 py-1 text-sm font-medium text-white"
                        style={{ backgroundColor: category.color || '#6366f1' }}
                      >
                        {category.name}
                      </span>
                      {!category.isActive && (
                        <span className="inline-flex items-center rounded-md bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-600">
                          Inactive
                        </span>
                      )}
                    </div>
                    {category.description && (
                      <p className="text-sm text-gray-600">{category.description}</p>
                    )}
                    <p className="mt-1 text-xs text-gray-500">
                      Display Order: {category.displayOrder}
                    </p>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        ) : (
          <div className="p-8 text-center text-gray-500">
            No categories yet. Create your first category to get started.
          </div>
        )}
      </div>
    </div>
  );
}
