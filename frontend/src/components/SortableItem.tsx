import React, { useState } from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';

export interface TierItemData {
  id: string;
  imageUrl: string;
  caption?: string | null;
}

export const TierItemView: React.FC<{
  item: TierItemData;
  isDragging?: boolean;
  readOnly?: boolean;
  showCaption?: boolean;
}> = ({ item, isDragging = false, readOnly = false, showCaption = false }) => {
  const [imgError, setImgError] = useState(false);

  return (
    <div
      className={`tier-item ${isDragging ? 'is-dragging' : ''} ${readOnly ? 'read-only' : ''}`}
      title={item.caption || undefined}
    >
      {!imgError ? (
        <img
          src={item.imageUrl}
          alt={item.caption || 'Item'}
          referrerPolicy="no-referrer"
          onError={(e) => {
            const target = e.target as HTMLImageElement;
            if (!target.dataset.triedProxy && item.imageUrl.startsWith('http')) {
              target.dataset.triedProxy = 'true';
              target.src = `/images/proxy?url=${encodeURIComponent(item.imageUrl)}`;
            } else {
              setImgError(true);
            }
          }}
        />
      ) : (
        <div className="tier-item-fallback">
          {item.caption || 'Item'}
        </div>
      )}

      {showCaption && item.caption && (
        <div className="tier-item-caption" title={item.caption}>
          {item.caption}
        </div>
      )}
    </div>
  );
};

export const SortableItem: React.FC<{
  item: TierItemData;
  disabled?: boolean;
  showCaption?: boolean;
}> = ({ item, disabled = false, showCaption = false }) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: item.id,
    disabled,
  });

  const style: React.CSSProperties = {
    transform: CSS.Translate.toString(transform),
    transition,
    opacity: isDragging ? 0.6 : 1,
  };

  return (
    <div ref={setNodeRef} style={style} {...attributes} {...listeners}>
      <TierItemView item={item} isDragging={isDragging} showCaption={showCaption} />
    </div>
  );
};

export const ItemOverlay: React.FC<{ item: TierItemData; showCaption?: boolean }> = ({ item, showCaption = false }) => {
  return (
    <div style={{ opacity: 0.6 }}>
      <TierItemView item={item} isDragging showCaption={showCaption} />
    </div>
  );
};
