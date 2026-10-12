import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  KeyboardSensor,
  useSensor,
  useSensors,
  closestCenter,
} from '@dnd-kit/core';
import type { DragStartEvent, DragOverEvent, DragEndEvent } from '@dnd-kit/core';
import { arrayMove, sortableKeyboardCoordinates } from '@dnd-kit/sortable';
import { tierListsApi } from '../api/tierlists';
import { useAuth } from '../context/AuthContext';
import { TierBoard } from '../components/TierBoard';
import { ItemOverlay } from '../components/SortableItem';
import type { Tier, TierItem, SaveLayoutRequest } from '../types';

export const TierListEditorPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();

  const { data: tierList, isLoading, isError } = useQuery({
    queryKey: ['tierlist', id],
    queryFn: () => tierListsApi.getById(id!),
    enabled: !!id,
  });

  const [tiers, setTiers] = useState<Tier[]>([]);
  const [itemsMap, setItemsMap] = useState<Record<string, TierItem>>({});
  const [containers, setContainers] = useState<Record<string, string[]>>({});
  const [initialContainers, setInitialContainers] = useState<Record<string, string[]>>({});
  const [activeId, setActiveId] = useState<string | null>(null);
  const [isDirty, setIsDirty] = useState(false);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [savedSuccess, setSavedSuccess] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [showCaptions, setShowCaptions] = useState(false);

  // Setup sensors for dnd-kit
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 5,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Load tierlist into state
  useEffect(() => {
    if (tierList) {
      setTitle(tierList.title);
      setDescription(tierList.description || '');
      setTiers([...tierList.tiers].sort((a, b) => a.order - b.order));

      const map: Record<string, TierItem> = {};
      const newContainers: Record<string, string[]> = {
        unplaced: [],
      };

      tierList.tiers.forEach((t) => {
        newContainers[t.id] = [];
      });

      tierList.items.forEach((item) => {
        map[item.id] = item;
        if (item.tierId && newContainers[item.tierId]) {
          newContainers[item.tierId].push(item.id);
        } else {
          newContainers.unplaced.push(item.id);
        }
      });

      // Sort items within each tier by position
      tierList.tiers.forEach((t) => {
        newContainers[t.id].sort((a, b) => (map[a]?.position ?? 0) - (map[b]?.position ?? 0));
      });
      newContainers.unplaced.sort((a, b) => (map[a]?.position ?? 0) - (map[b]?.position ?? 0));

      setItemsMap(map);
      setContainers(newContainers);
      setInitialContainers(newContainers);
      setIsDirty(false);
    }
  }, [tierList]);

  // Unsaved changes warning on tab/window close
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault();
        e.returnValue = '';
      }
    };
    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [isDirty]);

  // Find which container currently contains an item or matches container ID
  const findContainer = (itemIdOrContainerId: string): string | undefined => {
    if (itemIdOrContainerId in containers) {
      return itemIdOrContainerId;
    }
    return Object.keys(containers).find((key) =>
      containers[key].includes(itemIdOrContainerId)
    );
  };

  const handleDragStart = (event: DragStartEvent) => {
    setActiveId(String(event.active.id));
  };

  const handleDragOver = (event: DragOverEvent) => {
    const { active, over } = event;
    if (!over) return;

    const activeIdStr = String(active.id);
    const overIdStr = String(over.id);

    const activeContainer = findContainer(activeIdStr);
    const overContainer = findContainer(overIdStr);

    if (!activeContainer || !overContainer || activeContainer === overContainer) {
      return;
    }

    setContainers((prev) => {
      const activeItems = prev[activeContainer] || [];
      const overItems = prev[overContainer] || [];

      const overIndex = overItems.indexOf(overIdStr);

      let newIndex: number;
      if (overIdStr in prev) {
        // Dropping directly onto the container dropzone
        newIndex = overItems.length;
      } else {
        const isBelowOverItem =
          over &&
          active.rect.current.translated &&
          active.rect.current.translated.top > over.rect.top + over.rect.height / 2;
        const modifier = isBelowOverItem ? 1 : 0;
        newIndex = overIndex >= 0 ? overIndex + modifier : overItems.length;
      }

      setIsDirty(true);

      return {
        ...prev,
        [activeContainer]: activeItems.filter((id) => id !== activeIdStr),
        [overContainer]: [
          ...overItems.slice(0, newIndex),
          activeIdStr,
          ...overItems.slice(newIndex),
        ],
      };
    });
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveId(null);

    if (!over) return;

    const activeIdStr = String(active.id);
    const overIdStr = String(over.id);

    const activeContainer = findContainer(activeIdStr);
    const overContainer = findContainer(overIdStr);

    if (!activeContainer || !overContainer) return;

    if (activeContainer === overContainer) {
      const items = containers[activeContainer] || [];
      const oldIndex = items.indexOf(activeIdStr);
      const newIndex = items.indexOf(overIdStr);

      if (oldIndex !== newIndex && newIndex !== -1) {
        setContainers((prev) => ({
          ...prev,
          [activeContainer]: arrayMove(prev[activeContainer], oldIndex, newIndex),
        }));
        setIsDirty(true);
      }
    }
  };

  const saveMutation = useMutation({
    mutationFn: async () => {
      setErrorMessage(null);
      // Update details if modified
      if (
        tierList &&
        (title.trim() !== tierList.title || description.trim() !== (tierList.description || ''))
      ) {
        await tierListsApi.update(id!, {
          title: title.trim(),
          description: description.trim() || undefined,
        });
      }

      // Save layout in one single request
      const layout: SaveLayoutRequest = {
        tiers: tiers.map((tier) => ({
          tierId: tier.id,
          itemIds: containers[tier.id] || [],
        })),
        unplacedItemIds: containers['unplaced'] || [],
      };
      await tierListsApi.saveLayout(id!, layout);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tierlist', id] });
      setInitialContainers(containers);
      setIsDirty(false);
      setSavedSuccess(true);
      setTimeout(() => setSavedSuccess(false), 3500);
    },
    onError: (err: any) => {
      const msg =
        err.response?.data?.message ||
        err.response?.data?.title ||
        'Failed to save tier list layout';
      setErrorMessage(msg);
    },
  });

  const handleReset = () => {
    if (window.confirm('Discard all unsaved changes and reset layout?')) {
      setContainers(initialContainers);
      setIsDirty(false);
    }
  };

  const handleNavigateBack = () => {
    if (isDirty) {
      if (!window.confirm('You have unsaved changes. Are you sure you want to leave?')) {
        return;
      }
    }
    navigate(`/tierlists/${id}`);
  };

  if (isLoading) {
    return (
      <div className="page-container">
        <p className="loading-text">Loading editor...</p>
      </div>
    );
  }

  if (isError || !tierList) {
    return (
      <div className="page-container">
        <h1>Tier list not found</h1>
        <p className="text-muted">This tier list does not exist or may have been deleted.</p>
        <p>
          <Link to="/">Back to explore</Link>
        </p>
      </div>
    );
  }

  const isOwner = user?.id === tierList.authorId;

  if (!isOwner) {
    return (
      <div className="page-container">
        <h1>Permission denied</h1>
        <p className="text-muted">Only the author can modify the layout of this tier list.</p>
        <p>
          <Link to={`/tierlists/${id}`}>View tier list</Link>
        </p>
      </div>
    );
  }

  const activeItem = activeId ? itemsMap[activeId] : null;

  return (
    <div className="page-container-full">
      {/* Action Toolbar */}
      <div className="editor-top-bar">
        <div className="editor-left-actions">
          <button type="button" onClick={handleNavigateBack}>
            Back
          </button>
          {isDirty ? (
            <span style={{ color: 'var(--danger)', fontSize: 13 }}>Unsaved changes</span>
          ) : savedSuccess ? (
            <span style={{ color: '#80ff80', fontSize: 13 }}>Saved successfully</span>
          ) : (
            <span className="text-muted" style={{ fontSize: 13 }}>All changes saved</span>
          )}
        </div>

        <div className="editor-right-actions">
          <button
            type="button"
            onClick={() => setShowCaptions(!showCaptions)}
          >
            {showCaptions ? 'Hide captions' : 'Show captions'}
          </button>

          <Link to={`/tierlists/${id}`} className="btn">
            Preview
          </Link>

          {isDirty && (
            <button
              type="button"
              onClick={handleReset}
              disabled={saveMutation.isPending}
            >
              Reset
            </button>
          )}

          <button
            type="button"
            onClick={() => saveMutation.mutate()}
            disabled={!isDirty || saveMutation.isPending}
            className="btn-primary"
          >
            {saveMutation.isPending ? 'Saving...' : 'Save layout'}
          </button>
        </div>
      </div>

      {errorMessage && <p className="form-error" style={{ margin: '12px 0' }}>{errorMessage}</p>}

      {/* Title & Description inline edit */}
      <div className="editor-details-form">
        <input
          type="text"
          value={title}
          onChange={(e) => {
            setTitle(e.target.value);
            setIsDirty(true);
          }}
          placeholder="Tier list title"
          style={{ fontWeight: 'bold', fontSize: 18 }}
        />
        <textarea
          rows={2}
          value={description}
          onChange={(e) => {
            setDescription(e.target.value);
            setIsDirty(true);
          }}
          placeholder="Tier list description (optional)"
        />
      </div>

      {/* DND Context & Board */}
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragOver={handleDragOver}
        onDragEnd={handleDragEnd}
      >
        <div style={{ marginTop: 16 }}>
          <TierBoard
            tiers={tiers}
            itemsByTier={tiers.reduce<Record<string, any>>((acc, tier) => {
              const itemIds = containers[tier.id] || [];
              acc[tier.id] = itemIds.map((itemId) => itemsMap[itemId]).filter(Boolean);
              return acc;
            }, {})}
            unplacedItems={(containers['unplaced'] || [])
              .map((itemId) => itemsMap[itemId])
              .filter(Boolean)}
            showCaptions={showCaptions}
          />
        </div>

        {/* Drag Overlay */}
        <DragOverlay dropAnimation={null}>
          {activeItem ? <ItemOverlay item={activeItem} showCaption={showCaptions} /> : null}
        </DragOverlay>
      </DndContext>
    </div>
  );
};
