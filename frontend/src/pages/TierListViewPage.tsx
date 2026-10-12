import React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tierListsApi } from '../api/tierlists';
import { useAuth } from '../context/AuthContext';
import { TierBoard } from '../components/TierBoard';
import { VoteButtons } from '../components/VoteButtons';
import { CommentsSection } from '../components/CommentsSection';
import type { TierListDetail, VoteResult } from '../types';

export const TierListViewPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user, isModerator } = useAuth();
  const [showCaptions, setShowCaptions] = React.useState(false);

  const { data: tierList, isLoading, isError } = useQuery<TierListDetail>({
    queryKey: ['tierlist', id],
    queryFn: () => tierListsApi.getById(id!),
    enabled: !!id,
  });

  const voteMutation = useMutation({
    mutationFn: (value: number) => tierListsApi.vote(id!, value),
    onSuccess: (result: VoteResult) => {
      queryClient.setQueryData(['tierlist', id], (old: TierListDetail | undefined) => {
        if (!old) return old;
        return {
          ...old,
          likes: result.likesCount,
          dislikes: result.dislikesCount,
          likesCount: result.likesCount,
          dislikesCount: result.dislikesCount,
          userVote: result.userVote,
        };
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => tierListsApi.delete(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tierlists'] });
      navigate('/');
    },
  });

  if (isLoading) {
    return (
      <div className="page-container">
        <p className="loading-text">Loading tier list...</p>
      </div>
    );
  }

  if (isError || !tierList) {
    return (
      <div className="page-container">
        <h1>Tier list not found</h1>
        <p className="text-muted">This tier list may have been deleted or the link is invalid.</p>
        <p>
          <Link to="/">Back to explore</Link>
        </p>
      </div>
    );
  }

  const isOwner = user?.id === tierList.authorId;
  const canDelete = isOwner || isModerator;

  const formattedDate = new Date(tierList.createdAt).toLocaleDateString(undefined, {
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  });

  const likes = tierList.likes ?? tierList.likesCount ?? 0;
  const dislikes = tierList.dislikes ?? tierList.dislikesCount ?? 0;

  // Group items by tierId
  const itemsByTier: Record<string, typeof tierList.items> = {};
  const unplacedItems: typeof tierList.items = [];

  tierList.items.forEach((item) => {
    if (item.tierId) {
      if (!itemsByTier[item.tierId]) {
        itemsByTier[item.tierId] = [];
      }
      itemsByTier[item.tierId].push(item);
    } else {
      unplacedItems.push(item);
    }
  });

  return (
    <div className="page-container">
      {/* Title & metadata */}
      <h1>{tierList.title}</h1>

      {tierList.description && (
        <p className="tier-description">{tierList.description}</p>
      )}

      <div className="meta-row">
        <span>By <strong>{tierList.authorUserName}</strong></span>
        <span className="text-muted">{formattedDate}</span>
        {tierList.templateId && (
          <span>
            Template: <Link to={`/t/${tierList.templateId}`}>{tierList.templateTitle || 'Rank this'}</Link>
          </span>
        )}
      </div>

      {/* Tier Board (Read-only) */}
      <div style={{ marginTop: 16, marginBottom: 16 }}>
        <TierBoard
          tiers={tierList.tiers.slice().sort((a, b) => a.order - b.order)}
          itemsByTier={itemsByTier}
          unplacedItems={unplacedItems}
          readOnly
          showCaptions={showCaptions}
        />
      </div>

      {/* Toolbar / Actions */}
      <div className="tierlist-actions-bar">
        <button
          type="button"
          onClick={() => setShowCaptions(!showCaptions)}
        >
          {showCaptions ? 'Hide captions' : 'Show captions'}
        </button>

        <VoteButtons
          likesCount={likes}
          dislikesCount={dislikes}
          userVote={tierList.userVote}
          onVote={async (val) => {
            await voteMutation.mutateAsync(val);
          }}
          disabled={voteMutation.isPending}
        />

        {isOwner && (
          <Link to={`/tierlists/${tierList.id}/edit`} className="btn">
            Edit layout
          </Link>
        )}

        {canDelete && (
          <button
            type="button"
            onClick={() => {
              if (window.confirm('Are you sure you want to delete this tier list?')) {
                deleteMutation.mutate();
              }
            }}
            disabled={deleteMutation.isPending}
            className="delete-btn"
          >
            {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
          </button>
        )}
      </div>

      {/* Comments Section */}
      <CommentsSection tierListId={tierList.id} />
    </div>
  );
};
