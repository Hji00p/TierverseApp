import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface VoteButtonsProps {
  likesCount: number;
  dislikesCount: number;
  userVote?: number | null;
  onVote: (value: number) => Promise<unknown> | void;
  disabled?: boolean;
}

export const VoteButtons: React.FC<VoteButtonsProps> = ({
  likesCount,
  dislikesCount,
  userVote,
  onVote,
  disabled = false,
}) => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const handleVote = async (targetValue: number) => {
    if (!user) {
      navigate('/login');
      return;
    }

    // If clicking same vote, remove vote by sending 0
    const newValue = userVote === targetValue ? 0 : targetValue;
    await onVote(newValue);
  };

  return (
    <div className="vote-buttons-row">
      <button
        type="button"
        onClick={() => handleVote(1)}
        disabled={disabled}
        className={`vote-btn ${userVote === 1 ? 'active' : ''}`}
      >
        Like {likesCount}
      </button>

      <button
        type="button"
        onClick={() => handleVote(-1)}
        disabled={disabled}
        className={`vote-btn ${userVote === -1 ? 'active' : ''}`}
      >
        Dislike {dislikesCount}
      </button>
    </div>
  );
};
