# Agentic AI Workflow Builder

A complete workflow builder system with visual editor and execution engine.

## Architecture

- **Backend**: .NET Core 8 Web API with workflow orchestration engine
- **Frontend**: React application with HTML5 Canvas workflow editor
- **Database**: Entity Framework Core with In-Memory database (configurable for SQL Server)

## Project Structure

```
workflow-ui/
├── server/                 # .NET Core 8 Backend
│   ├── src/
│   │   ├── Controllers/    # API endpoints
│   │   ├── Models/         # Data models and node executors
│   │   ├── Services/       # Business logic services
│   │   ├── Data/           # EF Core DbContext
│   │   ├── DTOs/           # Data transfer objects
│   │   └── Middleware/     # Error handling
│   ├── Program.cs
│   └── appsettings.json
│
└── client/                 # React Frontend
    ├── src/
    │   ├── components/     # React components
    │   ├── utils/          # Utilities
    │   ├── App.jsx
    │   └── index.jsx
    ├── package.json
    └── vite.config.js
```

## Features

### Backend Features
- ✅ Workflow CRUD operations
- ✅ Workflow validation
- ✅ Workflow execution engine
- ✅ Version management and publishing
- ✅ Node executors for all node types:
  - MessageNode - Display messages
  - QuestionNode - Prompt for user input with validation
  - APICallNode - Make HTTP requests
  - ConditionNode - Branching logic
  - FunctionNode - LLM/Agent actions (placeholder)
  - StateUpdateNode - Update workflow state
- ✅ Swagger/OpenAPI documentation
- ✅ CORS enabled for React frontend
- ✅ Global error handling
- ✅ Audit logging

### Frontend Features
- ✅ Visual workflow canvas with HTML5 Canvas
- ✅ Node palette with all node types
- ✅ Drag-and-drop node positioning
- ✅ Connection creation between nodes
- ✅ Dynamic property inspector
- ✅ Save/Load workflows
- ✅ Export/Import JSON
- ✅ Workflow validation
- ✅ Workflow execution

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm

### Backend Setup

1. Navigate to the server directory:
```bash
cd server
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Run the backend:
```bash
dotnet run
```

The API will start at `http://localhost:5000` with Swagger UI at the root.

### Frontend Setup

1. Navigate to the client directory:
```bash
cd client
```

2. Install npm packages:
```bash
npm install
```

Note: The `@clientio/rappid` package requires a commercial license. For development, you can:
- Use the trial version
- Or the current implementation uses HTML5 Canvas instead

3. Run the development server:
```bash
npm run dev
```

The frontend will start at `http://localhost:3000`.

## API Endpoints

### Workflow Management
- `POST /api/workflow` - Create workflow
- `GET /api/workflow/{id}` - Get workflow
- `PUT /api/workflow/{id}` - Update workflow
- `DELETE /api/workflow/{id}` - Delete workflow
- `GET /api/workflow/list` - List all workflows
- `POST /api/workflow/{id}/validate` - Validate workflow
- `POST /api/workflow/{id}/publish` - Publish version

### Workflow Execution
- `POST /api/workflow/execute` - Execute workflow

## Workflow Node Types

1. **StartNode** - Entry point of the workflow
2. **EndNode** - Exit point of the workflow
3. **MessageNode** - Display messages to users
4. **QuestionNode** - Prompt for user input with validation
5. **APICallNode** - Make HTTP requests to external APIs
6. **ConditionNode** - Evaluate conditions for branching
7. **FunctionNode** - Execute LLM/Agent actions
8. **StateUpdateNode** - Update workflow state variables
9. **DecisionSplitNode** - Split execution into multiple paths

## Database Configuration

By default, the application uses an in-memory database for development.

To use SQL Server, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WorkflowEngine;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Then run migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Usage Example

1. Start the backend server
2. Start the frontend application
3. Click on node types in the palette to add them to the canvas
4. Drag nodes to reposition them
5. Click on a node's output port (right side) and drag to another node's input port (left side) to create connections
6. Select a node to edit its properties in the inspector panel
7. Click "Save" to save the workflow to the backend
8. Click "Validate" to check for errors
9. Click "Execute" to run the workflow

## Development Notes

- The backend uses an in-memory database by default, so data is lost on restart
- The frontend uses a simplified canvas implementation instead of RappidJS
- LLM/Agent function nodes have placeholder implementation
- JWT authentication is configured but not enforced (add authentication as needed)

## Next Steps

- Implement actual LLM integration in FunctionNodeExecutor
- Add user authentication and authorization
- Implement real-time workflow execution monitoring
- Add workflow templates
- Implement undo/redo functionality
- Add mini-map navigation
- Implement zoom controls
- Add workflow sharing and collaboration features
