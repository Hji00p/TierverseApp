import { Component } from 'react';
import type { ErrorInfo, ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
    errorInfo: null,
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error, errorInfo: null };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Uncaught error in component tree:', error, errorInfo);
    this.setState({ errorInfo });
  }

  public handleReload = () => {
    window.location.reload();
  };

  public handleGoHome = () => {
    window.location.href = '/';
  };

  public render() {
    if (this.state.hasError) {
      return (
        <div className="page-container" style={{ marginTop: 40 }}>
          <h1>Something went wrong</h1>
          <p className="text-muted">
            An unexpected error occurred while rendering the page.
          </p>

          {this.state.error && (
            <p className="form-error" style={{ fontFamily: 'monospace', fontSize: 13 }}>
              {this.state.error.toString()}
            </p>
          )}

          <div className="form-actions" style={{ marginTop: 16 }}>
            <button
              type="button"
              onClick={this.handleReload}
              className="btn-primary"
            >
              Reload page
            </button>
            <button
              type="button"
              onClick={this.handleGoHome}
            >
              Back to home
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
