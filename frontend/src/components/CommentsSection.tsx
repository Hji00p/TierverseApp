import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { commentsApi } from '../api/comments';
import { useAuth } from '../context/AuthContext';
import { VoteButtons } from './VoteButtons';
import { Link } from 'react-router-dom';

interface CommentsSectionProps {
  tierListId: string;
}

export const CommentsSection: React.FC<CommentsSectionProps> = ({ tierListId }) => {
  const { user, isModerator } = useAuth();
  const queryClient = useQueryClient();
  const [commentText, setCommentText] = useState('');
  const [page, setPage] = useState(1);

  // Track reply state
  const [replyToComment, setReplyToComment] = useState<{
    id: string;
    parentCommentId: string;
    authorUserName: string;
  } | null>(null);
  const [replyText, setReplyText] = useState('');

  // Track expanded replies per comment
  const [expandedCommentIds, setExpandedCommentIds] = useState<Set<string>>(new Set());

  const toggleReplies = (commentId: string) => {
    setExpandedCommentIds((prev) => {
      const next = new Set(prev);
      if (next.has(commentId)) {
        next.delete(commentId);
      } else {
        next.add(commentId);
      }
      return next;
    });
  };

  const { data, isLoading } = useQuery({
    queryKey: ['comments', tierListId, page],
    queryFn: () => commentsApi.getByTierListId(tierListId, page),
  });

  const createMutation = useMutation({
    mutationFn: ({ text, parentCommentId }: { text: string; parentCommentId?: string }) =>
      commentsApi.create(tierListId, text, parentCommentId),
    onSuccess: () => {
      setCommentText('');
      setReplyToComment(null);
      setReplyText('');
      queryClient.invalidateQueries({ queryKey: ['comments', tierListId] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (commentId: string) => commentsApi.delete(commentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comments', tierListId] });
    },
  });

  const voteMutation = useMutation({
    mutationFn: ({ commentId, value }: { commentId: string; value: number }) =>
      commentsApi.vote(commentId, value),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comments', tierListId] });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!commentText.trim()) return;
    createMutation.mutate({ text: commentText.trim() });
  };

  const handleReplySubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!replyText.trim() || !replyToComment) return;
    const parentId = replyToComment.parentCommentId;
    setExpandedCommentIds((prev) => new Set(prev).add(parentId));
    createMutation.mutate({
      text: replyText.trim(),
      parentCommentId: parentId,
    });
  };

  const handleDelete = (commentId: string, isReply: boolean) => {
    const msg = isReply
      ? 'Delete this reply?'
      : 'Delete this comment and all its replies?';
    if (window.confirm(msg)) {
      deleteMutation.mutate(commentId);
    }
  };

  return (
    <div className="comments-section">
      <h2>Comments {data ? `(${data.totalCount})` : ''}</h2>

      {user ? (
        <form onSubmit={handleSubmit} className="comment-form">
          <textarea
            rows={3}
            value={commentText}
            onChange={(e) => setCommentText(e.target.value)}
            placeholder="Write a comment..."
            maxLength={2000}
          />
          <div className="form-actions">
            <button
              type="submit"
              disabled={!commentText.trim() || createMutation.isPending}
              className="btn-primary"
            >
              {createMutation.isPending ? 'Posting...' : 'Post comment'}
            </button>
          </div>
        </form>
      ) : (
        <p className="guest-hint">
          <Link to="/login">Log in</Link> or <Link to="/register">register</Link> to leave a comment.
        </p>
      )}

      {isLoading ? (
        <p className="loading-text">Loading comments...</p>
      ) : data?.items.length === 0 ? (
        <p className="text-muted">No comments yet.</p>
      ) : (
        <div className="comments-list">
          {data?.items.map((comment) => {
            const canDelete =
              !comment.isDeleted && (user?.id === comment.authorId || isModerator);

            const formattedDate = new Date(comment.createdAt).toLocaleDateString(undefined, {
              month: 'short',
              day: 'numeric',
              year: 'numeric',
            });

            return (
              <div key={comment.id} className="comment-item">
                <div className="comment-header">
                  <span className="comment-author">{comment.authorUserName}</span>
                  <span className="text-muted">{formattedDate}</span>
                  {canDelete && (
                    <button
                      type="button"
                      onClick={() => handleDelete(comment.id, false)}
                      disabled={deleteMutation.isPending}
                      className="link-btn delete-btn"
                    >
                      Delete
                    </button>
                  )}
                </div>

                <div className="comment-body">
                  <p className={comment.isDeleted ? 'text-muted italic' : ''}>
                    {comment.text}
                  </p>
                </div>

                {!comment.isDeleted && (
                  <div className="comment-actions">
                    <VoteButtons
                      likesCount={comment.likesCount}
                      dislikesCount={comment.dislikesCount}
                      userVote={comment.userVote}
                      onVote={async (value) => {
                        await voteMutation.mutateAsync({ commentId: comment.id, value });
                      }}
                      disabled={voteMutation.isPending}
                    />

                    {user && (
                      <button
                        type="button"
                        onClick={() => {
                          if (replyToComment?.id === comment.id) {
                            setReplyToComment(null);
                            setReplyText('');
                          } else {
                            setReplyToComment({
                              id: comment.id,
                              parentCommentId: comment.id,
                              authorUserName: comment.authorUserName,
                            });
                            setReplyText('');
                          }
                        }}
                      >
                        Reply
                      </button>
                    )}
                  </div>
                )}

                {/* Inline reply form for root comment */}
                {replyToComment?.id === comment.id && (
                  <form onSubmit={handleReplySubmit} className="reply-form">
                    <p className="text-muted">Replying to {replyToComment.authorUserName}</p>
                    <textarea
                      rows={2}
                      autoFocus
                      value={replyText}
                      onChange={(e) => setReplyText(e.target.value)}
                      placeholder="Write a reply..."
                      maxLength={2000}
                    />
                    <div className="form-actions">
                      <button
                        type="button"
                        onClick={() => {
                          setReplyToComment(null);
                          setReplyText('');
                        }}
                      >
                        Cancel
                      </button>
                      <button
                        type="submit"
                        disabled={!replyText.trim() || createMutation.isPending}
                      >
                        {createMutation.isPending ? 'Replying...' : 'Reply'}
                      </button>
                    </div>
                  </form>
                )}

                {/* Threaded replies */}
                {comment.replies && comment.replies.length > 0 && (() => {
                  const totalReplies = comment.replies.length;
                  const isExpanded = expandedCommentIds.has(comment.id);
                  const displayedReplies =
                    isExpanded || totalReplies <= 1
                      ? comment.replies
                      : comment.replies.slice(0, 1);

                  return (
                    <div className="replies-list">
                      {displayedReplies.map((reply) => {
                        const canDeleteReply =
                          !reply.isDeleted && (user?.id === reply.authorId || isModerator);
                        const replyDate = new Date(reply.createdAt).toLocaleDateString(undefined, {
                          month: 'short',
                          day: 'numeric',
                          year: 'numeric',
                        });

                        return (
                          <div key={reply.id} className="reply-item">
                            <div className="comment-header">
                              <span className="comment-author">{reply.authorUserName}</span>
                              <span className="text-muted">{replyDate}</span>
                              {canDeleteReply && (
                                <button
                                  type="button"
                                  onClick={() => handleDelete(reply.id, true)}
                                  disabled={deleteMutation.isPending}
                                  className="link-btn delete-btn"
                                >
                                  Delete
                                </button>
                              )}
                            </div>

                            <div className="comment-body">
                              <p className={reply.isDeleted ? 'text-muted italic' : ''}>
                                {reply.text}
                              </p>
                            </div>

                            {!reply.isDeleted && (
                              <div className="comment-actions">
                                <VoteButtons
                                  likesCount={reply.likesCount}
                                  dislikesCount={reply.dislikesCount}
                                  userVote={reply.userVote}
                                  onVote={async (value) => {
                                    await voteMutation.mutateAsync({ commentId: reply.id, value });
                                  }}
                                  disabled={voteMutation.isPending}
                                />

                                {user && (
                                  <button
                                    type="button"
                                    onClick={() => {
                                      if (replyToComment?.id === reply.id) {
                                        setReplyToComment(null);
                                        setReplyText('');
                                      } else {
                                        setReplyToComment({
                                          id: reply.id,
                                          parentCommentId: comment.id,
                                          authorUserName: reply.authorUserName,
                                        });
                                        setReplyText(`@${reply.authorUserName} `);
                                      }
                                    }}
                                  >
                                    Reply
                                  </button>
                                )}
                              </div>
                            )}

                            {replyToComment?.id === reply.id && (
                              <form onSubmit={handleReplySubmit} className="reply-form">
                                <p className="text-muted">Replying to {replyToComment.authorUserName}</p>
                                <textarea
                                  rows={2}
                                  autoFocus
                                  value={replyText}
                                  onChange={(e) => setReplyText(e.target.value)}
                                  placeholder="Write a reply..."
                                  maxLength={2000}
                                />
                                <div className="form-actions">
                                  <button
                                    type="button"
                                    onClick={() => {
                                      setReplyToComment(null);
                                      setReplyText('');
                                    }}
                                  >
                                    Cancel
                                  </button>
                                  <button
                                    type="submit"
                                    disabled={!replyText.trim() || createMutation.isPending}
                                  >
                                    {createMutation.isPending ? 'Replying...' : 'Reply'}
                                  </button>
                                </div>
                              </form>
                            )}
                          </div>
                        );
                      })}

                      {totalReplies > 1 && (
                        <div style={{ marginTop: 8 }}>
                          <button
                            type="button"
                            onClick={() => toggleReplies(comment.id)}
                            className="link-btn"
                          >
                            {isExpanded
                              ? 'Hide replies'
                              : `Show ${totalReplies - 1} more ${
                                  totalReplies - 1 === 1 ? 'reply' : 'replies'
                                }`}
                          </button>
                        </div>
                      )}
                    </div>
                  );
                })()}
              </div>
            );
          })}

          {data && data.totalPages > 1 && (
            <div className="pagination-row">
              <button
                type="button"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={!data.hasPreviousPage}
              >
                Previous
              </button>
              <span className="text-muted">
                Page {data.pageNumber} of {data.totalPages}
              </span>
              <button
                type="button"
                onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                disabled={!data.hasNextPage}
              >
                Next
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
