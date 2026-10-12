import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminApi } from '../api/admin';
import { useAuth } from '../context/AuthContext';
import { Navigate } from 'react-router-dom';

export const AdminPage: React.FC = () => {
  const { user, isAdmin } = useAuth();
  const queryClient = useQueryClient();

  const [searchInput, setSearchInput] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [page, setPage] = useState(1);
  const [selectedRoles, setSelectedRoles] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);

  if (!isAdmin) {
    return <Navigate to="/" replace />;
  }

  const { data, isLoading, isError } = useQuery({
    queryKey: ['adminUsers', searchTerm, page],
    queryFn: () => adminApi.getUsers(searchTerm, page, 20),
  });

  const updateRoleMutation = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) =>
      adminApi.updateUserRole(userId, role),
    onSuccess: (_, variables) => {
      setMessage(`User role updated to ${variables.role}.`);
      queryClient.invalidateQueries({ queryKey: ['adminUsers'] });
      setTimeout(() => setMessage(null), 3000);
    },
    onError: () => {
      setMessage('Failed to update role.');
    },
  });

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchTerm(searchInput.trim());
    setPage(1);
  };

  const handleRoleSelectChange = (userId: string, role: string) => {
    setSelectedRoles((prev) => ({ ...prev, [userId]: role }));
  };

  const handleSaveRole = (userId: string, currentRole: string) => {
    const roleToSave = selectedRoles[userId] || currentRole;
    updateRoleMutation.mutate({ userId, role: roleToSave });
  };

  return (
    <div className="page-container">
      <h1>Admin: User Management</h1>

      {message && <p className="admin-msg">{message}</p>}

      {/* Search Input */}
      <form onSubmit={handleSearchSubmit} className="search-row" style={{ maxWidth: 400, marginBottom: 16 }}>
        <input
          type="text"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder="Search by username or email..."
        />
        <button type="submit">Search</button>
      </form>

      {isLoading ? (
        <p className="loading-text">Loading users...</p>
      ) : isError ? (
        <p className="error-text">Failed to load users.</p>
      ) : (
        <>
          <div className="items-table-wrap">
            <table className="items-table">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Email</th>
                  <th>Registered</th>
                  <th>Role</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {data?.items.map((u) => {
                  const currentRole = u.roles[0] || 'User';
                  const isCurrentLoggedUser = u.id === user?.id;
                  const activeSelectedRole = selectedRoles[u.id] ?? currentRole;

                  return (
                    <tr key={u.id}>
                      <td><strong>{u.userName}</strong></td>
                      <td className="text-muted">{u.email || '—'}</td>
                      <td className="text-muted">
                        {new Date(u.createdAt).toLocaleDateString()}
                      </td>
                      <td>
                        <select
                          value={activeSelectedRole}
                          disabled={isCurrentLoggedUser || updateRoleMutation.isPending}
                          onChange={(e) => handleRoleSelectChange(u.id, e.target.value)}
                          style={{ width: 'auto', padding: '4px 6px' }}
                        >
                          <option value="User">User</option>
                          <option value="Moderator">Moderator</option>
                          <option value="Admin">Admin</option>
                        </select>
                      </td>
                      <td>
                        <button
                          type="button"
                          onClick={() => handleSaveRole(u.id, currentRole)}
                          disabled={
                            isCurrentLoggedUser ||
                            updateRoleMutation.isPending ||
                            activeSelectedRole === currentRole
                          }
                          style={{ padding: '4px 10px', fontSize: 13 }}
                        >
                          Save
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
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
        </>
      )}
    </div>
  );
};
