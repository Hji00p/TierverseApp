# Tierverse

A minimalist, utilitarian tier list social platform. Create customizable templates, rank items via drag-and-drop, export high-resolution PNG boards, share rankings with direct links, and discuss in threaded comment sections.

---

## Tech Stack

### Frontend
- **Framework**: React 19 + TypeScript
- **Build Tool**: Vite
- **State & Server Cache**: TanStack React Query (`@tanstack/react-query`)
- **Routing**: React Router v7
- **Drag & Drop**: `@dnd-kit/core`, `@dnd-kit/sortable`, `@dnd-kit/utilities`
- **Export**: `html-to-image`
- **Styling**: Pure CSS with design tokens, strict neutral palette, 0 component libraries, flat utilitarian design

### Backend
- **Framework**: .NET 9 Web API
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, WebApi)
- **Database**: PostgreSQL 16 (via Entity Framework Core & Npgsql)
- **Authentication**: JWT Bearer tokens + HTTP-only refresh token cookies + ASP.NET Core Identity
- **API Documentation**: Swagger / OpenAPI
- **Testing**: xUnit + FluentAssertions + Moq

---

## Features

- **Shareable Templates**: Create reusable ranking templates with customizable default tiers, labels, custom colors, and image items.
- **Interactive Drag-and-Drop Editor**: Real-time item reordering and placement across tiers and an unplaced pool using `@dnd-kit`.
- **Custom Item Uploads**: Guests and registered users can add local images directly to active boards.
- **Captions Toggle**: Toggle item caption overlays on or off across template rankings, preview boards, and editor layouts.
- **PNG Export**: Export clean `#191917` tier boards with 1:1 screen fidelity (excludes UI chrome, buttons, and pool items).
- **Social Features**: Public exploration feed, template search, upvoting/downvoting (likes/dislikes), and threaded discussions.
- **User Draft Isolation**: Auto-saves in-progress rankings to local storage scoped strictly by user ID, clearing drafts on logout.
- **Role Management**: Admin panel for assigning user roles (`User`, `Moderator`, `Admin`).

---

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (v20+) & `npm`
- [Docker Desktop](https://www.docker.com/) (for PostgreSQL)

### 1. Database Setup
Start the PostgreSQL container:
```bash
docker compose up -d
```
*Default port: `5433` | Database: `tierverse_db` | User: `postgres` | Password: `postgres`*

### 2. Backend Setup
Run EF Core migrations and start the API:
```bash
cd src/TierList.WebApi
dotnet run
```
*API runs on `http://localhost:5194` (Swagger available at `/swagger`).*

### 3. Frontend Setup
In a separate terminal, install dependencies and launch the dev server:
```bash
cd frontend
npm install
npm run dev
```
*Frontend runs on `http://localhost:5173`.*

---

## Project Structure

```
TierverseApp/
├── docker-compose.yml          # PostgreSQL database service
├── src/
│   ├── TierList.Domain/        # Entities, Enums, Value Objects
│   ├── TierList.Application/   # CQRS / Use Cases, DTOs, Interfaces
│   ├── TierList.Infrastructure/# EF Core DbContext, Repositories, Auth Services
│   └── TierList.WebApi/        # Controllers, Middlewares, Program.cs
├── tests/
│   ├── TierList.Domain.Tests/
│   └── TierList.Application.Tests/
└── frontend/
    ├── src/
    │   ├── api/                # Axios API client & endpoints
    │   ├── components/         # TierBoard, SortableItem, CommentsSection, Navbar, etc.
    │   ├── context/            # AuthContext (JWT & session lifecycle)
    │   ├── pages/              # Home, Template, TierListView, Editors, Auth, Admin
    │   ├── index.css           # Global tokens & utilitarian styles
    │   └── types/              # TypeScript models
    └── package.json
```
