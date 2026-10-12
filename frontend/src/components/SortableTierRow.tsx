import React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import type { CreateTierRequest } from '../types';

interface SortableTierRowProps {
  id: string;
  tier: CreateTierRequest;
  index: number;
  canDelete: boolean;
  onUpdate: (index: number, field: keyof CreateTierRequest, value: string) => void;
  onRemove: (index: number) => void;
}

export const SortableTierRow: React.FC<SortableTierRowProps> = ({
  id,
  tier,
  index,
  canDelete,
  onUpdate,
  onRemove,
}) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    setActivatorNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.6 : 1,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className="tier-edit-row"
      {...attributes}
    >
      {/* Drag handle button */}
      <button
        type="button"
        ref={setActivatorNodeRef}
        {...listeners}
        className="drag-handle-btn"
        title="Drag to reorder"
      >
        Drag
      </button>

      {/* Tier name input */}
      <input
        type="text"
        value={tier.name}
        maxLength={30}
        onChange={(e) => onUpdate(index, 'name', e.target.value)}
        placeholder="Tier name"
      />

      {/* Native color input */}
      <input
        type="color"
        value={tier.color}
        onChange={(e) => onUpdate(index, 'color', e.target.value)}
        title="Tier color"
      />

      {/* Remove button */}
      {canDelete && (
        <button
          type="button"
          onClick={() => onRemove(index)}
          className="delete-btn"
        >
          Remove
        </button>
      )}
    </div>
  );
};
