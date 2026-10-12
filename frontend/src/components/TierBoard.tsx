import React from 'react';
import { useDroppable } from '@dnd-kit/core';
import { SortableContext } from '@dnd-kit/sortable';
import { SortableItem, TierItemView } from './SortableItem';
import type { TierItemData } from './SortableItem';

export interface TierBoardTier {
  id: string;
  name: string;
  color: string;
  order?: number;
}

export interface TierBoardProps {
  tiers: TierBoardTier[];
  itemsByTier: Record<string, TierItemData[]>;
  unplacedItems?: TierItemData[];
  readOnly?: boolean;
  boardRef?: React.Ref<HTMLDivElement>;
  showCaptions?: boolean;
}

export function getLabelTextColor(hex: string): string {
  if (!hex) return '#303030';
  const clean = hex.replace('#', '');
  let r = 0, g = 0, b = 0;
  if (clean.length === 3) {
    r = parseInt(clean[0] + clean[0], 16);
    g = parseInt(clean[1] + clean[1], 16);
    b = parseInt(clean[2] + clean[2], 16);
  } else if (clean.length === 6) {
    r = parseInt(clean.substring(0, 2), 16);
    g = parseInt(clean.substring(2, 4), 16);
    b = parseInt(clean.substring(4, 6), 16);
  } else {
    return '#303030';
  }
  const lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;
  return lum < 128 ? '#e6e6e6' : '#303030';
}

const DroppableTierItems: React.FC<{
  tierId: string;
  items: TierItemData[];
  showCaptions?: boolean;
}> = ({ tierId, items, showCaptions = false }) => {
  const { setNodeRef, isOver } = useDroppable({
    id: tierId,
  });

  return (
    <div
      ref={setNodeRef}
      className={`items ${isOver ? 'is-over' : ''}`}
    >
      <SortableContext items={items.map((i) => i.id)}>
        {items.map((item) => (
          <SortableItem key={item.id} item={item} showCaption={showCaptions} />
        ))}
      </SortableContext>
    </div>
  );
};

const DroppableUnplacedPool: React.FC<{
  items: TierItemData[];
  showCaptions?: boolean;
}> = ({ items, showCaptions = false }) => {
  const { setNodeRef, isOver } = useDroppable({
    id: 'unplaced',
  });

  return (
    <div className="unplaced-pool">
      <div className="unplaced-header">Unplaced ({items.length})</div>
      <div
        ref={setNodeRef}
        className={`unplaced-items ${isOver ? 'is-over' : ''}`}
      >
        <SortableContext items={items.map((i) => i.id)}>
          {items.map((item) => (
            <SortableItem key={item.id} item={item} showCaption={showCaptions} />
          ))}
        </SortableContext>
      </div>
    </div>
  );
};

export const TierBoard: React.FC<TierBoardProps> = ({
  tiers,
  itemsByTier,
  unplacedItems,
  readOnly = false,
  boardRef,
  showCaptions = false,
}) => {
  return (
    <div className="tier-board-wrapper">
      <div className="tier-list" ref={boardRef}>
        {tiers.map((tier) => {
          const items = itemsByTier[tier.id] || [];
          const labelColor = getLabelTextColor(tier.color);

          return (
            <div key={tier.id} className="tier">
              <div
                className="label"
                style={{
                  backgroundColor: tier.color,
                  color: labelColor,
                }}
              >
                {tier.name}
              </div>

              {readOnly ? (
                <div className="items">
                  {items.map((item) => (
                    <TierItemView key={item.id} item={item} readOnly showCaption={showCaptions} />
                  ))}
                </div>
              ) : (
                <DroppableTierItems tierId={tier.id} items={items} showCaptions={showCaptions} />
              )}
            </div>
          );
        })}
      </div>

      {/* Unplaced Items Section below the board with 3px black gap */}
      {unplacedItems !== undefined && (
        <>
          {readOnly ? (
            unplacedItems.length > 0 && (
              <div className="unplaced-pool">
                <div className="unplaced-header">Unplaced ({unplacedItems.length})</div>
                <div className="unplaced-items">
                  {unplacedItems.map((item) => (
                    <TierItemView key={item.id} item={item} readOnly showCaption={showCaptions} />
                  ))}
                </div>
              </div>
            )
          ) : (
            <DroppableUnplacedPool items={unplacedItems} showCaptions={showCaptions} />
          )}
        </>
      )}
    </div>
  );
};
