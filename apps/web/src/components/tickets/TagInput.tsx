'use client';

import { useState, useRef, useEffect } from 'react';
import { useGetAllTags } from '@/lib/queries/tags';

interface TagInputProps {
  value: string[];
  onChange: (tags: string[]) => void;
  disabled?: boolean;
  error?: string;
}

export function TagInput({ value, onChange, disabled, error }: TagInputProps) {
  const [input, setInput] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const inputRef = useRef<HTMLInputElement>(null);
  const { data: allTags } = useGetAllTags();

  // Filter suggestions based on input
  const suggestions = allTags
    ?.filter(
      (tag) =>
        tag.name.toLowerCase().includes(input.toLowerCase()) &&
        !value.includes(tag.name)
    )
    .slice(0, 10) || [];

  // Add a tag
  const addTag = (tagName: string) => {
    const trimmed = tagName.trim();
    if (trimmed && !value.includes(trimmed)) {
      onChange([...value, trimmed]);
    }
    setInput('');
    setShowSuggestions(false);
    setFocusedIndex(-1);
  };

  // Remove a tag
  const removeTag = (tagName: string) => {
    onChange(value.filter((t) => t !== tagName));
  };

  // Handle keyboard navigation
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      if (focusedIndex >= 0 && suggestions[focusedIndex]) {
        addTag(suggestions[focusedIndex].name);
      } else if (input.trim()) {
        addTag(input);
      }
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      setFocusedIndex((prev) => Math.min(prev + 1, suggestions.length - 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setFocusedIndex((prev) => Math.max(prev - 1, -1));
    } else if (e.key === 'Escape') {
      setShowSuggestions(false);
      setFocusedIndex(-1);
    } else if (e.key === 'Backspace' && !input && value.length > 0) {
      // Remove last tag when backspace on empty input
      removeTag(value[value.length - 1]);
    }
  };

  // Close suggestions when clicking outside
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (inputRef.current && !inputRef.current.contains(e.target as Node)) {
        setShowSuggestions(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className="space-y-2">
      <label htmlFor="tags" className="block text-sm font-medium text-gray-700">
        Tags
      </label>
      <div
        className={`
          relative mt-1 flex flex-wrap gap-2 rounded-md border border-gray-300 p-2
          focus-within:border-blue-500 focus-within:ring-1 focus-within:ring-blue-500
          ${disabled ? 'bg-gray-100 cursor-not-allowed' : 'bg-white'}
          ${error ? 'border-red-300' : ''}
        `}
      >
        {/* Selected tags */}
        {value.map((tag) => (
          <span
            key={tag}
            className="inline-flex items-center gap-1 rounded-full bg-blue-100 px-3 py-1 text-sm font-medium text-blue-800"
          >
            {tag}
            {!disabled && (
              <button
                type="button"
                onClick={() => removeTag(tag)}
                className="inline-flex items-center justify-center hover:bg-blue-200 rounded-full p-0.5"
              >
                <svg className="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
                <span className="sr-only">Remove {tag}</span>
              </button>
            )}
          </span>
        ))}

        {/* Input field */}
        <input
          ref={inputRef}
          id="tags"
          type="text"
          value={input}
          onChange={(e) => {
            setInput(e.target.value);
            setShowSuggestions(true);
            setFocusedIndex(-1);
          }}
          onFocus={() => setShowSuggestions(true)}
          onKeyDown={handleKeyDown}
          disabled={disabled}
          placeholder={value.length === 0 ? 'Type to add tags...' : ''}
          className="flex-1 min-w-[150px] border-0 p-0 focus:ring-0 disabled:bg-gray-100 disabled:cursor-not-allowed"
        />

        {/* Autocomplete suggestions */}
        {showSuggestions && suggestions.length > 0 && (
          <div className="absolute left-0 right-0 top-full z-10 mt-1 max-h-60 overflow-auto rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5">
            {suggestions.map((tag, index) => (
              <button
                key={tag.id}
                type="button"
                onClick={() => addTag(tag.name)}
                className={`
                  block w-full px-4 py-2 text-left text-sm
                  ${focusedIndex === index ? 'bg-blue-100 text-blue-900' : 'text-gray-900'}
                  hover:bg-blue-50
                `}
              >
                {tag.name}
              </button>
            ))}
          </div>
        )}
      </div>
      {error && <p className="text-sm text-red-600">{error}</p>}
      <p className="text-xs text-gray-500">
        Press Enter to add a tag. Press Backspace on empty input to remove the last tag.
      </p>
    </div>
  );
}
