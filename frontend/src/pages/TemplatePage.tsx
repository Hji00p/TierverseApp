import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  KeyboardSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import type { DragStartEvent, DragOverEvent, DragEndEvent } from '@dnd-kit/core';
import { arrayMove, sortableKeyboardCoordinates } from '@dnd-kit/sortable';
import { getTemplateById, deleteTemplate } from '../api/templates';
import { tierListsApi } from '../api/tierlists';
import { useAuth } from '../context/AuthContext';
import { TierBoard } from '../components/TierBoard';
import { ItemOverlay } from '../components/SortableItem';
import { toPng } from 'html-to-image';
import type { Tier, TierItem, CreateTierListRequest } from '../types';

interface ExtendedItem extends TierItem {
  isLocal?: boolean;
}

export const TemplatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const boardRef = useRef<HTMLDivElement>(null);

  const [tiers, setTiers] = useState<Tier[]>([]);
  const [containers, setContainers] = useState<Record<string, string[]>>({
    unplaced: [],
  });
  const [itemsMap, setItemsMap] = useState<Record<string, ExtendedItem>>({});
  const [activeId, setActiveId] = useState<string | null>(null);

  const [showTierEditing, setShowTierEditing] = useState(false);
  const [showCaptions, setShowCaptions] = useState(false);
  const [isExporting, setIsExporting] = useState(false);
  const [copiedShare, setCopiedShare] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

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

  const { data: template, isLoading, isError } = useQuery({
    queryKey: ['template', id],
    queryFn: () => getTemplateById(id!),
    enabled: !!id,
  });

  const draftKey = template ? `tierverse_draft_${user?.id || 'guest'}_${template.id}` : null;

  useEffect(() => {
    if (!template || !draftKey) return;

    const savedDraft = localStorage.getItem(draftKey);

    if (savedDraft) {
      try {
        const parsed = JSON.parse(savedDraft);
        if (parsed.tiers && parsed.containers && parsed.items) {
          setTiers(parsed.tiers);
          setContainers(parsed.containers);
          setItemsMap(parsed.items);
          return;
        }
      } catch (e) {
        console.warn('Failed to parse draft from localStorage', e);
      }
    }

    resetToTemplate();
  }, [template, user?.id]);

  useEffect(() => {
    if (!template || !draftKey || tiers.length === 0) return;

    const draftData = {
      tiers,
      containers,
      items: itemsMap,
      savedAt: Date.now(),
    };

    try {
      localStorage.setItem(draftKey, JSON.stringify(draftData));
    } catch (e) {
      console.warn('Failed to save draft to localStorage', e);
    }
  }, [tiers, containers, itemsMap, template, draftKey]);

  const resetToTemplate = () => {
    if (!template) return;

    const initialTiers: Tier[] = template.tiers
      .slice()
      .sort((a, b) => a.order - b.order)
      .map((t, idx) => ({
        id: t.id,
        name: t.name,
        color: t.color,
        order: idx,
      }));

    const map: Record<string, ExtendedItem> = {};
    const unplacedIds: string[] = [];
    const newContainers: Record<string, string[]> = {
      unplaced: [],
    };

    initialTiers.forEach((t) => {
      newContainers[t.id] = [];
    });

    template.items
      .slice()
      .sort((a, b) => a.order - b.order)
      .forEach((item) => {
        map[item.id] = {
          id: item.id,
          imageUrl: item.imageUrl,
          caption: item.caption,
          position: item.order,
          tierId: null,
          isLocal: false,
        };
        unplacedIds.push(item.id);
      });

    newContainers.unplaced = unplacedIds;

    setTiers(initialTiers);
    setItemsMap(map);
    setContainers(newContainers);
    if (draftKey) {
      localStorage.removeItem(draftKey);
    }
  };

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

    if (
      !activeContainer ||
      !overContainer ||
      activeContainer === overContainer
    ) {
      return;
    }

    setContainers((prev) => {
      const activeItems = prev[activeContainer];
      const overItems = prev[overContainer];

      const activeIndex = activeItems.indexOf(activeIdStr);
      let overIndex = overItems.indexOf(overIdStr);

      let newIndex: number;
      if (overIdStr in prev) {
        newIndex = overItems.length;
      } else {
        const isBelowOverItem =
          over &&
          active.rect.current.translated &&
          active.rect.current.translated.top >
            over.rect.top + over.rect.height;

        const modifier = isBelowOverItem ? 1 : 0;
        newIndex =
          overIndex >= 0 ? overIndex + modifier : overItems.length;
      }

      return {
        ...prev,
        [activeContainer]: activeItems.filter((item) => item !== activeIdStr),
        [overContainer]: [
          ...overItems.slice(0, newIndex),
          activeItems[activeIndex],
          ...overItems.slice(newIndex),
        ],
      };
    });
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    const activeIdStr = String(active.id);

    if (!over) {
      setActiveId(null);
      return;
    }

    const overIdStr = String(over.id);
    const activeContainer = findContainer(activeIdStr);
    const overContainer = findContainer(overIdStr);

    if (!activeContainer || !overContainer) {
      setActiveId(null);
      return;
    }

    const activeIndex = containers[activeContainer].indexOf(activeIdStr);
    const overIndex = containers[overContainer].indexOf(overIdStr);

    if (activeContainer === overContainer) {
      if (activeIndex !== overIndex && overIndex !== -1) {
        setContainers((prev) => ({
          ...prev,
          [overContainer]: arrayMove(
            prev[overContainer],
            activeIndex,
            overIndex
          ),
        }));
      }
    } else {
      setContainers((prev) => {
        const activeItems = prev[activeContainer];
        const overItems = prev[overContainer];

        return {
          ...prev,
          [activeContainer]: activeItems.filter((i) => i !== activeIdStr),
          [overContainer]: [...overItems, activeIdStr],
        };
      });
    }

    setActiveId(null);
  };

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    Array.from(files).forEach((file) => {
      const reader = new FileReader();
      reader.onload = (event) => {
        const dataUrl = event.target?.result as string;
        if (!dataUrl) return;

        const newItemId = `local_${Date.now()}_${Math.random()
          .toString(36)
          .substring(2, 7)}`;

        const newItem: ExtendedItem = {
          id: newItemId,
          imageUrl: dataUrl,
          caption: file.name.replace(/\.[^/.]+$/, ''),
          position: 0,
          tierId: null,
          isLocal: true,
        };

        setItemsMap((prev) => ({ ...prev, [newItemId]: newItem }));
        setContainers((prev) => ({
          ...prev,
          unplaced: [newItemId, ...(prev.unplaced || [])],
        }));
      };
      reader.readAsDataURL(file);
    });

    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleAddTier = () => {
    const newId = `tier_${Date.now()}`;
    const newTier: Tier = {
      id: newId,
      name: `Tier ${tiers.length + 1}`,
      color: '#ffbf80',
      order: tiers.length,
    };

    setTiers((prev) => [...prev, newTier]);
    setContainers((prev) => ({
      ...prev,
      [newId]: [],
    }));
  };

  const handleUpdateTier = (id: string, updates: Partial<Tier>) => {
    setTiers((prev) =>
      prev.map((t) => (t.id === id ? { ...t, ...updates } : t))
    );
  };

  const handleDeleteTier = (tierId: string) => {
    if (tiers.length <= 1) return;

    const itemsInTier = containers[tierId] || [];

    setContainers((prev) => {
      const copy = { ...prev };
      delete copy[tierId];
      return {
        ...copy,
        unplaced: [...copy.unplaced, ...itemsInTier],
      };
    });

    setTiers((prev) => prev.filter((t) => t.id !== tierId));
  };

  const handleMoveTier = (index: number, direction: 'up' | 'down') => {
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    if (targetIndex < 0 || targetIndex >= tiers.length) return;

    setTiers((prev) => arrayMove(prev, index, targetIndex));
  };

  const handleShareLink = async () => {
    try {
      await navigator.clipboard.writeText(window.location.href);
      setCopiedShare(true);
      setTimeout(() => setCopiedShare(false), 2000);
    } catch {
      setErrorMessage('Could not copy link to clipboard.');
    }
  };

  // Download PNG Export
  const handleDownloadPng = async () => {
    if (!boardRef.current) return;
    setIsExporting(true);
    setErrorMessage(null);

    const imgElements = Array.from(boardRef.current.querySelectorAll('img'));
    const originalSrcs = imgElements.map((img) => img.src);

    try {
      await Promise.all(
        imgElements.map(async (img) => {
          const currentSrc = img.src;
          if (!currentSrc || currentSrc.startsWith('data:')) return;

          try {
            const res = await fetch(currentSrc, { mode: 'cors' });
            if (res.ok) {
              const blob = await res.blob();
              img.src = await new Promise<string>((resolve) => {
                const reader = new FileReader();
                reader.onloadend = () => resolve(reader.result as string);
                reader.readAsDataURL(blob);
              });
              return;
            }
          } catch {
            // fallback to proxy
          }

          try {
            const proxyUrl = `/images/proxy?url=${encodeURIComponent(currentSrc)}`;
            const res = await fetch(proxyUrl);
            if (res.ok) {
              const blob = await res.blob();
              img.src = await new Promise<string>((resolve) => {
                const reader = new FileReader();
                reader.onloadend = () => resolve(reader.result as string);
                reader.readAsDataURL(blob);
              });
              return;
            }
          } catch {
            // failed
          }

          img.src =
            'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAA=';
        })
      );

      await Promise.all(
        imgElements.map((img) =>
          img.decode ? img.decode().catch(() => {}) : Promise.resolve()
        )
      );

      const dataUrl = await toPng(boardRef.current, {
        cacheBust: false,
        pixelRatio: 2,
        backgroundColor: '#191917',
      });

      const cleanTitle = (template?.title || 'tier-list')
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '');

      const link = document.createElement('a');
      link.download = `${cleanTitle}-ranked.png`;
      link.href = dataUrl;
      link.click();
    } catch (err) {
      console.error('PNG export error', err);
      setErrorMessage('Could not export PNG. Please try again.');
    } finally {
      imgElements.forEach((img, idx) => {
        img.src = originalSrcs[idx];
      });
      setIsExporting(false);
    }
  };

  const saveToProfileMutation = useMutation({
    mutationFn: async () => {
      if (!template) throw new Error('No template');

      const payload: CreateTierListRequest = {
        title: template.title,
        description: template.description || undefined,
        templateId: template.id,
        tiers: tiers.map((t, idx) => ({
          id: t.id,
          name: t.name,
          color: t.color,
          order: idx,
        })),
        layout: {
          tiers: tiers.map((t) => ({
            tierId: t.id,
            itemIds: (containers[t.id] || []).filter((itemId) => !itemId.startsWith('local_')),
          })),
          unplacedItemIds: (containers.unplaced || []).filter(
            (itemId) => !itemId.startsWith('local_')
          ),
        },
      };

      return await tierListsApi.create(payload);
    },
    onSuccess: (newId) => {
      if (draftKey) {
        localStorage.removeItem(draftKey);
      }
      navigate(`/l/${newId}`);
    },
    onError: (err: any) => {
      setErrorMessage(err.response?.data?.detail || 'Failed to save tier list.');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async () => {
      if (!template) return;
      await deleteTemplate(template.id);
    },
    onSuccess: () => {
      localStorage.removeItem(`tierverse_draft_${template?.id}`);
      navigate('/');
    },
    onError: (err: any) => {
      setErrorMessage(err.response?.data?.detail || 'Failed to delete template.');
    },
  });

  const isOwner = Boolean(user && template && user.id === template.authorId);
  const isModOrAdmin = Boolean(
    user && (user.roles?.includes('Moderator') || user.roles?.includes('Admin'))
  );
  const canDelete = isOwner || isModOrAdmin;
  const canEdit = isOwner || isModOrAdmin;

  if (isLoading) {
    return (
      <div className="page-text">
        <p>Loading...</p>
      </div>
    );
  }

  if (isError || !template) {
    return (
      <div className="page-text">
        <h1>Template not found</h1>
        <p>This template may have been deleted or the link is incorrect.</p>
        <Link to="/">Back to Home</Link>
      </div>
    );
  }

  const activeItem = activeId ? itemsMap[activeId] : null;

  return (
    <div className="page-container-full">
      {/* Title as h1 (text only) */}
      <h1>{template.title}</h1>
      {template.description && <p style={{ color: 'var(--text-muted)' }}>{template.description}</p>}

      {errorMessage && <p className="form-error">{errorMessage}</p>}

      {/* Board & Unplaced Pool */}
      <DndContext
        sensors={sensors}
        onDragStart={handleDragStart}
        onDragOver={handleDragOver}
        onDragEnd={handleDragEnd}
      >
        <TierBoard
          tiers={tiers}
          itemsByTier={tiers.reduce<Record<string, ExtendedItem[]>>((acc, tier) => {
            const itemIds = containers[tier.id] || [];
            acc[tier.id] = itemIds.map((id) => itemsMap[id]).filter(Boolean);
            return acc;
          }, {})}
          unplacedItems={(containers.unplaced || [])
            .map((id) => itemsMap[id])
            .filter(Boolean)}
          boardRef={boardRef}
          showCaptions={showCaptions}
        />

        <DragOverlay dropAnimation={null}>
          {activeItem ? <ItemOverlay item={activeItem} showCaption={showCaptions} /> : null}
        </DragOverlay>
      </DndContext>

      {/* Plain toolbar row under it */}
      <div className="toolbar-row">
        <button
          type="button"
          onClick={() => setShowCaptions(!showCaptions)}
        >
          {showCaptions ? 'Hide captions' : 'Show captions'}
        </button>

        <button
          type="button"
          onClick={handleDownloadPng}
          disabled={isExporting}
        >
          {isExporting ? 'Exporting...' : 'Download PNG'}
        </button>

        <button type="button" onClick={resetToTemplate}>
          Reset
        </button>

        <button type="button" onClick={handleShareLink}>
          {copiedShare ? 'Link copied' : 'Share link'}
        </button>

        <button type="button" onClick={() => fileInputRef.current?.click()}>
          Add own items
        </button>

        <button type="button" onClick={() => setShowTierEditing(!showTierEditing)}>
          {showTierEditing ? 'Hide tier editor' : 'Edit tiers'}
        </button>

        {user ? (
          <button
            type="button"
            className="btn-primary"
            onClick={() => saveToProfileMutation.mutate()}
            disabled={saveToProfileMutation.isPending}
          >
            {saveToProfileMutation.isPending ? 'Saving...' : 'Save to my profile'}
          </button>
        ) : (
          <p className="guest-hint">Log in to save this tier list to your profile.</p>
        )}

        {canEdit && (
          <Link to={`/templates/${template.id}/edit`} className="btn">
            Edit template
          </Link>
        )}

        {canDelete && (
          <button
            type="button"
            onClick={() => {
              if (confirm('Are you sure you want to delete this template?')) {
                deleteMutation.mutate();
              }
            }}
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? 'Deleting...' : 'Delete template'}
          </button>
        )}
      </div>

      {/* Tier editing panel under the board */}
      {showTierEditing && (
        <div className="tier-edit-panel">
          <h2>Edit Tiers</h2>
          {tiers.map((tier, idx) => (
            <div key={tier.id} className="tier-edit-row">
              <input
                type="text"
                value={tier.name}
                onChange={(e) => handleUpdateTier(tier.id, { name: e.target.value })}
                placeholder="Tier name"
              />
              <input
                type="color"
                value={tier.color}
                onChange={(e) => handleUpdateTier(tier.id, { color: e.target.value })}
                title="Tier color"
              />
              <button
                type="button"
                onClick={() => handleMoveTier(idx, 'up')}
                disabled={idx === 0}
              >
                Up
              </button>
              <button
                type="button"
                onClick={() => handleMoveTier(idx, 'down')}
                disabled={idx === tiers.length - 1}
              >
                Down
              </button>
              <button
                type="button"
                onClick={() => handleDeleteTier(tier.id)}
                disabled={tiers.length <= 1}
              >
                Remove
              </button>
            </div>
          ))}

          <div style={{ marginTop: 8 }}>
            <button type="button" onClick={handleAddTier}>
              Add tier
            </button>
          </div>
        </div>
      )}

      {/* Hidden file input for adding own items */}
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileUpload}
        accept="image/*"
        multiple
        style={{ display: 'none' }}
      />
    </div>
  );
};
