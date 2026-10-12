import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './context/AuthContext';
import { Navbar } from './components/Navbar';
import { ErrorBoundary } from './components/ErrorBoundary';
import { HomePage } from './pages/HomePage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { TierListViewPage } from './pages/TierListViewPage';
import { CreateTierListPage } from './pages/CreateTierListPage';
import { TierListEditorPage } from './pages/TierListEditorPage';
import { TemplatePage } from './pages/TemplatePage';
import { TemplateEditorPage } from './pages/TemplateEditorPage';
import { AdminPage } from './pages/AdminPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

export const App: React.FC = () => {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <BrowserRouter>
            <div className="app-shell">
              <Navbar />
              <main className="app-main">
                <Routes>
                  <Route path="/" element={<HomePage />} />
                  <Route path="/login" element={<LoginPage />} />
                  <Route path="/register" element={<RegisterPage />} />
                  <Route path="/tierlists/new" element={<CreateTierListPage />} />
                  <Route path="/templates/new" element={<TemplateEditorPage />} />
                  <Route path="/templates/:id/edit" element={<TemplateEditorPage />} />
                  <Route path="/t/:id" element={<TemplatePage />} />
                  <Route path="/l/:id" element={<TierListViewPage />} />
                  <Route path="/tierlists/:id" element={<TierListViewPage />} />
                  <Route path="/tierlists/:id/edit" element={<TierListEditorPage />} />
                  <Route path="/admin" element={<AdminPage />} />
                </Routes>
              </main>
              <footer className="app-footer">
                <p>Tierverse &copy; {new Date().getFullYear()}</p>
              </footer>
            </div>
          </BrowserRouter>
        </AuthProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  );
};

export default App;
