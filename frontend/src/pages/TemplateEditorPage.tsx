import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import type { DragEndEvent } from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { getTemplateById, createTemplate, updateTemplate } from '../api/templates';
import { uploadImage } from '../api/images';
import { useAuth } from '../context/AuthContext';
import { SortableTierRow } from '../components/SortableTierRow';
import type {
  CreateTemplateRequest,
  CreateTemplateItemModel,
  CreateTierRequest,
} from '../types';

interface TemplateTierForm {
  id: string;
  name: string;
  color: string;
  order: number;
}

const DEFAULT_TIERS: TemplateTierForm[] = [
  { id: '1', name: 'S', color: '#ff7f82', order: 0 },
  { id: '2', name: 'A', color: '#ffbf80', order: 1 },
  { id: '3', name: 'B', color: '#ffe080', order: 2 },
  { id: '4', name: 'C', color: '#ffff80', order: 3 },
  { id: '5', name: 'D', color: '#bfff80', order: 4 },
];

export const TemplateEditorPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const isEditMode = !!id;
  const navigate = useNavigate();
  const { user, isLoading: authLoading } = useAuth();
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!authLoading && !user) {
      navigate('/login');
    }
  }, [user, authLoading, navigate]);

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [visibility, setVisibility] = useState<number>(0); // 0 = Public, 1 = Unlisted
  const [tiers, setTiers] = useState<TemplateTierForm[]>(DEFAULT_TIERS);
  const [items, setItems] = useState<CreateTemplateItemModel[]>([]);

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

  const [manualUrl, setManualUrl] = useState('');
  const [manualCaption, setManualCaption] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: existingTemplate, isLoading: isLoadingExisting } = useQuery({
    queryKey: ['template', id],
    queryFn: () => getTemplateById(id!),
    enabled: isEditMode,
  });

  useEffect(() => {
    if (existingTemplate) {
      setTitle(existingTemplate.title);
      setDescription(existingTemplate.description || '');
      setVisibility(existingTemplate.visibility);
      setTiers(
        existingTemplate.tiers
          .slice()
          .sort((a, b) => a.order - b.order)
          .map((t, idx) => ({
            id: t.id || `tier-${idx + 1}`,
            name: t.name,
            color: t.color,
            order: idx,
          }))
      );
      setItems(
        existingTemplate.items
          .slice()
          .sort((a, b) => a.order - b.order)
          .map((i, idx) => ({
            imageUrl: i.imageUrl,
            caption: i.caption,
            order: idx,
          }))
      );
    }
  }, [existingTemplate]);

  // Tier operations
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

  const handleAddTier = () => {
    const newId = `tier-${Date.now()}`;
    setTiers((prev) => [
      ...prev,
      {
        id: newId,
        name: `Tier ${prev.length + 1}`,
        color: '#ffbf80',
        order: prev.length,
      },
    ]);
  };

  const handleUpdateTier = (
    index: number,
    field: keyof CreateTierRequest,
    value: string
  ) => {
    setTiers((prev) =>
      prev.map((t, idx) => (idx === index ? { ...t, [field]: value } : t))
    );
  };

  const handleRemoveTier = (index: number) => {
    if (tiers.length <= 1) return;
    setTiers((prev) =>
      prev.filter((_, idx) => idx !== index).map((t, idx) => ({ ...t, order: idx }))
    );
  };

  // Item operations
  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    setIsUploading(true);
    setError(null);

    try {
      const uploadedItems: CreateTemplateItemModel[] = [];

      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const url = await uploadImage(file);
        uploadedItems.push({
          imageUrl: url,
          caption: file.name.replace(/\.[^/.]+$/, ''),
          order: items.length + uploadedItems.length,
        });
      }

      setItems((prev) => [...prev, ...uploadedItems]);
    } catch (err: any) {
      console.error('Image upload failed', err);
      setError(
        err.response?.data?.detail ||
          err.response?.data?.message ||
          'Failed to upload image. Please try again.'
      );
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const handleAddManualItem = (e?: React.SyntheticEvent) => {
    if (e) e.preventDefault();
    if (!manualUrl.trim()) return;

    setItems((prev) => [
      ...prev,
      {
        imageUrl: manualUrl.trim(),
        caption: manualCaption.trim() || undefined,
        order: prev.length,
      },
    ]);

    setManualUrl('');
    setManualCaption('');
  };

  const handleRemoveItem = (index: number) => {
    setItems((prev) => prev.filter((_, idx) => idx !== index));
  };

  const handleUpdateItemCaption = (index: number, caption: string) => {
    setItems((prev) =>
      prev.map((item, idx) =>
        idx === index ? { ...item, caption: caption || undefined } : item
      )
    );
  };

  // Submit mutation
  const submitMutation = useMutation({
    mutationFn: async () => {
      setError(null);
      if (!title.trim()) throw new Error('Title is required.');
      if (tiers.length === 0) throw new Error('At least one tier is required.');
      if (items.length === 0) throw new Error('At least one item is required.');

      const payload: CreateTemplateRequest = {
        title: title.trim(),
        description: description.trim() || undefined,
        visibility,
        tiers: tiers.map((t, idx) => ({ ...t, order: idx })),
        items: items.map((i, idx) => ({ ...i, order: idx })),
      };

      if (isEditMode) {
        await updateTemplate(id!, payload);
        return id!;
      } else {
        const res = await createTemplate(payload);
        return res.id;
      }
    },
    onSuccess: (targetId) => {
      navigate(`/t/${targetId}`);
    },
    onError: (err: any) => {
      setError(
        err.response?.data?.detail ||
          err.message ||
          'Failed to save template. Please check all fields.'
      );
    },
  });

  if (isLoadingExisting) {
    return (
      <div className="page-container">
        <p className="loading-text">Loading template details...</p>
      </div>
    );
  }

  return (
    <div className="page-container">
      <h1>{isEditMode ? 'Edit template' : 'Create template'}</h1>

      {error && <p className="form-error">{error}</p>}

      <form
        onSubmit={(e) => {
          e.preventDefault();
          submitMutation.mutate();
        }}
        className="form-stack"
      >
        {/* Title, description, visibility */}
        <div className="form-group">
          <label htmlFor="tpl-title">Title</label>
          <input
            id="tpl-title"
            type="text"
            required
            maxLength={200}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g. CS2 Weapons, Fast Food Chains"
          />
        </div>

        <div className="form-group">
          <label htmlFor="tpl-description">Description (optional)</label>
          <textarea
            id="tpl-description"
            rows={3}
            maxLength={2000}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Provide context or ranking criteria..."
          />
        </div>

        <div className="form-group">
          <label htmlFor="tpl-visibility">Visibility</label>
          <select
            id="tpl-visibility"
            value={visibility}
            onChange={(e) => setVisibility(Number(e.target.value))}
            style={{ width: 'auto' }}
          >
            <option value={0}>Public (appears in search and explore)</option>
            <option value={1}>Unlisted (accessible via link only)</option>
          </select>
        </div>

        {/* Tiers section */}
        <div className="form-section">
          <h2>Tiers ({tiers.length})</h2>
          <p className="text-muted" style={{ fontSize: 13, marginBottom: 8 }}>
            Reorder tiers or customize their names and colors.
          </p>

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

        {/* Items section */}
        <div className="form-section">
          <h2>Template items ({items.length})</h2>
          <p className="text-muted" style={{ fontSize: 13, marginBottom: 8 }}>
            Upload images from your computer or paste image URLs.
          </p>

          {/* Upload Button */}
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
              {isUploading ? 'Uploading...' : 'Add images'}
            </button>
          </div>

          {/* Add item by manual URL */}
          <div className="manual-item-row" style={{ marginTop: 8 }}>
            <input
              type="url"
              value={manualUrl}
              onChange={(e) => setManualUrl(e.target.value)}
              placeholder="Paste image URL (https://...)"
            />
            <input
              type="text"
              value={manualCaption}
              onChange={(e) => setManualCaption(e.target.value)}
              placeholder="Caption (optional)"
              style={{ maxWidth: 200 }}
            />
            <button
              type="button"
              onClick={handleAddManualItem}
              disabled={!manualUrl.trim()}
            >
              Add URL
            </button>
          </div>

          {/* Items grid (96px squares with plain text Remove under each) */}
          {items.length === 0 ? (
            <p className="text-muted" style={{ marginTop: 12 }}>
              No items added yet.
            </p>
          ) : (
            <div className="template-items-grid">
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
                  <input
                    type="text"
                    value={item.caption || ''}
                    onChange={(e) => handleUpdateItemCaption(idx, e.target.value)}
                    placeholder="Caption"
                    className="item-caption-input"
                  />
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

        {/* Submit Actions */}
        <div className="form-actions" style={{ marginTop: 16 }}>
          <button
            type="submit"
            disabled={submitMutation.isPending || !title.trim() || items.length === 0}
            className="btn-primary"
          >
            {submitMutation.isPending
              ? 'Saving...'
              : isEditMode
              ? 'Save changes'
              : 'Create template'}
          </button>
          <Link to="/" className="btn">
            Cancel
          </Link>
        </div>
      </form>
    </div>
  );
};
