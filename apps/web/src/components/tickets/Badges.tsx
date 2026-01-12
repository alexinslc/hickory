'use client';

interface CategoryBadgeProps {
  name: string;
  color?: string;
}

export function CategoryBadge({ name, color }: CategoryBadgeProps) {
  const bgColor = color || '#6366f1'; // Default blue

  return (
    <span
      className="inline-flex items-center rounded-md px-2.5 py-0.5 text-xs font-medium text-white"
      style={{ backgroundColor: bgColor }}
    >
      {name}
    </span>
  );
}

interface TagBadgeProps {
  name: string;
  color?: string;
  onRemove?: () => void;
}

export function TagBadge({ name, color, onRemove }: TagBadgeProps) {
  const bgColor = color || '#e0e7ff'; // Default light blue
  const textColor = color ? '#1e1b4b' : '#4338ca'; // Dark blue for default

  return (
    <span
      className="inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium"
      style={{ backgroundColor: bgColor, color: textColor }}
    >
      {name}
      {onRemove && (
        <button
          type="button"
          onClick={onRemove}
          className="inline-flex items-center justify-center hover:opacity-70 rounded-full p-0.5"
        >
          <svg className="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
          <span className="sr-only">Remove {name}</span>
        </button>
      )}
    </span>
  );
}

interface TagListProps {
  tags: string[];
  onRemove?: (tag: string) => void;
}

export function TagList({ tags, onRemove }: TagListProps) {
  if (tags.length === 0) return null;

  return (
    <div className="flex flex-wrap gap-1">
      {tags.map((tag) => (
        <TagBadge
          key={tag}
          name={tag}
          onRemove={onRemove ? () => onRemove(tag) : undefined}
        />
      ))}
    </div>
  );
}
