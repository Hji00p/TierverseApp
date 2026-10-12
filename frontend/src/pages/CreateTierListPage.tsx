import React, { useState, useRef } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import {
  DndContext,
  PointerSensor,
  KeyboardSensor,
  useSensor,
  useSensors,
  closestCenter,
} from '@dnd-kit/core';
import type { DragEndEvent } from '@dnd-kit/core';
import {
  SortableContext,
  verticalListSortingStrategy,
  arrayMove,
  sortableKeyboardCoordinates,
} from '@dnd-kit/sortable';
import { tierListsApi } from '../api/tierlists';
import { uploadImage } from '../api/images';
import { useAuth } from '../context/AuthContext';
import { SortableTierRow } from '../components/SortableTierRow';
import type { CreateTierRequest, CreateTierItemRequest } from '../types';

interface TierItemForm extends CreateTierRequest {
  id: string;
}

const DEFAULT_TIERS: TierItemForm[] = [
  { id: 'tier-s', name: 'S', color: '#ff7f82', order: 0 },
  { id: 'tier-a', name: 'A', color: '#ffbf80', order: 1 },
  { id: 'tier-b', name: 'B', color: '#ffe080', order: 2 },
  { id: 'tier-c', name: 'C', color: '#ffff80', order: 3 },
  { id: 'tier-d', name: 'D', color: '#bfff80', order: 4 },
];

const SAMPLE_ITEMS: CreateTierItemRequest[] = [
  { imageUrl: 'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=200&auto=format&fit=crop&q=80', caption: 'Pizza' },
  { imageUrl: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=200&auto=format&fit=crop&q=80', caption: 'Burger' },
  { imageUrl: 'https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=200&auto=format&fit=crop&q=80', caption: 'Sushi' },
  { imageUrl: 'https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=200&auto=format&fit=crop&q=80', caption: 'Tacos' },
  { imageUrl: 'https://images.unsplash.com/photo-1501443762994-82bd5dace89a?w=200&auto=format&fit=crop&q=80', caption: 'Ice Cream' },
  { imageUrl: 'https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=200&auto=format&fit=crop&q=80', caption: 'Coffee' },
];

export const CreateTierListPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [tiers, setTiers] = useState<TierItemForm[]>(DEFAULT_TIERS);
  const [items, setItems] = useState<CreateTierItemRequest[]>([]);
  const [newItemUrl, setNewItemUrl] = useState('');
  const [newItemCaption, setNewItemCaption] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 3,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const createMutation = useMutation({
    mutationFn: tierListsApi.create,
    onSuccess: (tierListId) => {
      navigate(`/tierlists/${tierListId}/edit`);
    },
    onError: (err: any) => {
      const msg = err.response?.data?.message || err.response?.data?.title || 'Failed to create tier list';
      setError(msg);
    },
  });

  if (!user) {
    return (
      <div className="page-container">
        <h1>Login required</h1>
        <p className="text-muted">You need an account to create tier lists.</p>
        <p>
          <Link to="/login">Sign in</Link>
        </p>
      </div>
    );
  }

  const handleAddTier = () => {
    const nextOrder = tiers.length;
    setTiers([
      ...tiers,
      {
        id: `tier-${Date.now()}-${Math.random().toString(36).substring(2, 6)}`,
        name: 'Tier ' + (nextOrder + 1),
        color: '#ffbf80',
        order: nextOrder,
      },
    ]);
  };

  const handleRemoveTier = (index: number) => {
    if (tiers.length <= 1) return;
    const updated = tiers.filter((_, i) => i !== index).map((t, i) => ({ ...t, order: i }));
    setTiers(updated);
  };

  const handleUpdateTier = (index: number, field: keyof CreateTierRequest, value: string) => {
    const updated = [...tiers];
    updated[index] = { ...updated[index], [field]: value };
    setTiers(updated);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;

    const oldIndex = tiers.findIndex((t) => t.id === active.id);
    const newIndex = tiers.findIndex((t) => t.id === over.id);

    if (oldIndex !== -1 && newIndex !== -1) {
      const reordered = arrayMove(tiers, oldIndex, newIndex);
      setTiers(reordered.map((t, idx) => ({ ...t, order: idx })));
    }
  };

  const handleAddItem = (e?: React.SyntheticEvent) => {
    if (e) e.preventDefault();
    if (!newItemUrl.trim()) return;
    setItems((prev) => [
      ...prev,
      { imageUrl: newItemUrl.trim(), caption: newItemCaption.trim() || undefined },
    ]);
    setNewItemUrl('');
    setNewItemCaption('');
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    setIsUploading(true);
    setError(null);

    try {
      const uploadedItems: CreateTierItemRequest[] = [];

      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        try {
          const url = await uploadImage(file);
          uploadedItems.push({
            imageUrl: url,
            caption: file.name.replace(/\.[^/.]+$/, ''),
          });
        } catch (uploadErr) {
          console.warn('Backend upload failed, converting to data url as fallback', uploadErr);
          const dataUrl = await new Promise<string>((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result as string);
            reader.onerror = reject;
            reader.readAsDataURL(file);
          });
          uploadedItems.push({
            imageUrl: dataUrl,
            caption: file.name.replace(/\.[^/.]+$/, ''),
          });
        }
      }

      setItems((prev) => [...prev, ...uploadedItems]);
    } catch (err: any) {
      console.error('Image upload failed', err);
      setError(
        err.response?.data?.detail ||
          err.response?.data?.message ||
          'Failed to process selected image(s).'
      );
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const handleRemoveItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleLoadSampleItems = () => {
    setItems([...items, ...SAMPLE_ITEMS]);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError('Title is required');
      return;
    }
    setError(null);

    createMutation.mutate({
      title: title.trim(),
      description: description.trim() || undefined,
      tiers: tiers.map(({ name, color, order }) => ({ name, color, order })),
      items: items.length > 0 ? items : undefined,
    });
  };

  return (
    <div className="page-container">
      <h1>Create tier list</h1>
      <p className="text-muted" style={{ fontSize: 13, marginBottom: 16 }}>
        Set up title, tiers, and initial items. Arrange the layout in the editor next.
      </p>

      {error && <p className="form-error">{error}</p>}

      <form onSubmit={handleSubmit} className="form-stack">
        <div className="form-group">
          <label htmlFor="ctl-title">Title *</label>
          <input
            id="ctl-title"
            type="text"
            required
            maxLength={100}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g. Best Fast Food Chains"
          />
        </div>

        <div className="form-group">
          <label htmlFor="ctl-description">Description (optional)</label>
          <textarea
            id="ctl-description"
            rows={2}
            maxLength={1000}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Briefly describe what this tier list ranks..."
          />
        </div>

        {/* Tiers Configuration */}
        <div className="form-section">
          <h2>Tiers ({tiers.length})</h2>

          <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragEnd={handleDragEnd}
          >
            <SortableContext
              items={tiers.map((t) => t.id)}
              strategy={verticalListSortingStrategy}
            >
              <div className="tiers-edit-list">
                {tiers.map((tier, index) => (
                  <SortableTierRow
                    key={tier.id}
                    id={tier.id}
                    tier={tier}
                    index={index}
                    canDelete={tiers.length > 1}
                    onUpdate={handleUpdateTier}
                    onRemove={handleRemoveTier}
                  />
                ))}
              </div>
            </SortableContext>
          </DndContext>

          <button
            type="button"
            onClick={handleAddTier}
            style={{ marginTop: 8 }}
          >
            Add tier
          </button>
        </div>

        {/* Items Configuration */}
        <div className="form-section">
          <h2>Initial items ({items.length})</h2>

          <div className="upload-controls-row">
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleFileUpload}
              accept="image/*"
              multiple
              className="hidden"
            />
            <button
              type="button"
              disabled={isUploading}
              onClick={() => fileInputRef.current?.click()}
            >
              {isUploading ? 'Uploading...' : 'Upload images'}
            </button>
            <button
              type="button"
              onClick={handleLoadSampleItems}
            >
              Add sample items
            </button>
          </div>

          <div className="manual-item-row" style={{ marginTop: 8 }}>
            <input
              type="url"
              value={newItemUrl}
              onChange={(e) => setNewItemUrl(e.target.value)}
              placeholder="Image URL (https://...)"
            />
            <input
              type="text"
              value={newItemCaption}
              onChange={(e) => setNewItemCaption(e.target.value)}
              placeholder="Caption (optional)"
              maxLength={50}
              style={{ maxWidth: 200 }}
            />
            <button
              type="button"
              onClick={handleAddItem}
              disabled={!newItemUrl.trim()}
            >
              Add
            </button>
          </div>

          {items.length === 0 ? (
            <p className="text-muted" style={{ marginTop: 12 }}>
              No items added yet.
            </p>
          ) : (
            <div className="template-items-grid" style={{ marginTop: 12 }}>
              {items.map((item, idx) => (
                <div key={idx} className="template-item-cell">
                  <div className="item-square">
                    <img
                      src={item.imageUrl}
                      alt={item.caption || 'Item'}
                      title={item.caption || undefined}
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        if (!target.dataset.triedProxy && item.imageUrl.startsWith('http')) {
                          target.dataset.triedProxy = 'true';
                          target.src = `/images/proxy?url=${encodeURIComponent(item.imageUrl)}`;
                        }
                      }}
                    />
                  </div>
                  {item.caption && <span className="text-muted" style={{ fontSize: 12 }}>{item.caption}</span>}
                  <button
                    type="button"
                    onClick={() => handleRemoveItem(idx)}
                    className="link-btn delete-btn"
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Submit */}
        <div className="form-actions" style={{ marginTop: 16 }}>
          <button
            type="submit"
            disabled={createMutation.isPending || !title.trim()}
            className="btn-primary"
          >
            {createMutation.isPending ? 'Creating...' : 'Create & open editor'}
          </button>
          <Link to="/" className="btn">
            Cancel
          </Link>
        </div>
      </form>
    </div>
  );
};
