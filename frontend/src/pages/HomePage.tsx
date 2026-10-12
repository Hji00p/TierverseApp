import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { tierListsApi } from '../api/tierlists';
import { getTemplates } from '../api/templates';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

type Tab = 'templates' | 'tierlists';

export const HomePage: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<Tab>('templates');
  const [searchInput, setSearchInput] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [page, setPage] = useState(1);

  const templatesQuery = useQuery({
    queryKey: ['templates', searchTerm, page],
    queryFn: () => getTemplates({ search: searchTerm || undefined, page, pageSize: 20 }),
    enabled: activeTab === 'templates',
  });

  const tierListsQuery = useQuery({
    queryKey: ['tierlists', searchTerm, page],
    queryFn: () => tierListsApi.getAll(searchTerm || undefined, page, 20),
    enabled: activeTab === 'tierlists',
  });

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchTerm(searchInput.trim());
    setPage(1);
  };

  const handleTabChange = (tab: Tab) => {
    setActiveTab(tab);
    setPage(1);
  };

  const currentQuery = activeTab === 'templates' ? templatesQuery : tierListsQuery;
  const { data, isLoading, isError } = currentQuery;

  return (
    <div className="page-container">
      {/* Search Input + Button */}
      <form onSubmit={handleSearchSubmit} className="search-row">
        <input
          type="text"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder={`Search ${activeTab === 'templates' ? 'templates' : 'tier lists'}...`}
        />
        <button type="submit">Search</button>
      </form>

      {/* Tabs and Quick Actions */}
      <div className="home-tabs-bar">
        <div className="tabs-row">
          <button
            type="button"
            className={`tab-btn ${activeTab === 'templates' ? 'active' : ''}`}
            onClick={() => handleTabChange('templates')}
          >
            Templates
          </button>
          <button
            type="button"
            className={`tab-btn ${activeTab === 'tierlists' ? 'active' : ''}`}
            onClick={() => handleTabChange('tierlists')}
          >
            Tier lists
          </button>
        </div>

        {user && (
          <div className="actions-row">
            <Link to="/templates/new" className="btn">
              Create template
            </Link>
            <Link to="/tierlists/new" className="btn">
              Create tier list
            </Link>
          </div>
        )}
      </div>

      {/* Content */}
      {isLoading ? (
        <p className="loading-text">Loading...</p>
      ) : isError ? (
        <p className="error-text">Failed to load. Make sure the backend API is running.</p>
      ) : data?.items.length === 0 ? (
        <div className="empty-text">
          <p>No {activeTab === 'templates' ? 'templates' : 'tier lists'} found.</p>
          {user && (
            <p>
              <Link to={activeTab === 'templates' ? '/templates/new' : '/tierlists/new'}>
                Create {activeTab === 'templates' ? 'a template' : 'a tier list'}
              </Link>
            </p>
          )}
        </div>
      ) : (
        <>
          <div className="items-table-wrap">
            <table className="items-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Author</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {activeTab === 'templates'
                  ? (templatesQuery.data?.items ?? []).map((template) => {
                      const dateStr = template.createdAt
                        ? new Date(template.createdAt).toLocaleDateString()
                        : '—';
                      return (
                        <tr key={template.id}>
                          <td>
                            <Link to={`/t/${template.id}`}>{template.title}</Link>
                          </td>
                          <td className="text-muted">{template.authorUserName || '—'}</td>
                          <td className="text-muted">{dateStr}</td>
                        </tr>
                      );
                    })
                  : (tierListsQuery.data?.items ?? []).map((tierList) => {
                      const dateStr = tierList.createdAt
                        ? new Date(tierList.createdAt).toLocaleDateString()
                        : '—';
                      return (
                        <tr key={tierList.id}>
                          <td>
                            <Link to={`/l/${tierList.id}`}>{tierList.title}</Link>
                          </td>
                          <td className="text-muted">{tierList.authorUserName || '—'}</td>
                          <td className="text-muted">{dateStr}</td>
                        </tr>
                      );
                    })}
              </tbody>
            </table>
          </div>

          {/* Pagination Controls */}
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
