import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const Navbar: React.FC = () => {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <header className="app-header">
      <Link to="/" className="site-title">
        Tierverse
      </Link>

      <nav>
        <Link to="/">Templates</Link>
        <Link to="/tierlists/new">Tier lists</Link>
        {user && <Link to="/templates/new">New template</Link>}
        {isAdmin && <Link to="/admin">Admin</Link>}

        {user ? (
          <>
            <span className="nav-user">{user.userName}</span>
            <button type="button" onClick={handleLogout}>
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login">Login</Link>
            <Link to="/register">Register</Link>
          </>
        )}
      </nav>
    </header>
  );
};
