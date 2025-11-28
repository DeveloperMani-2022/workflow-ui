-- Workflow Engine Database Schema

-- Create Workflows table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Workflows')
BEGIN
    CREATE TABLE Workflows (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        GraphJson NVARCHAR(MAX) NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        ModifiedDate DATETIME2 NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedBy NVARCHAR(100) NOT NULL,
        IsPublished BIT NOT NULL DEFAULT 0
    );
END
GO

-- Create WorkflowVersions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WorkflowVersions')
BEGIN
    CREATE TABLE WorkflowVersions (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        WorkflowId UNIQUEIDENTIFIER NOT NULL,
        VersionNumber NVARCHAR(50) NOT NULL,
        GraphJson NVARCHAR(MAX) NOT NULL,
        PublishedDate DATETIME2 NOT NULL,
        PublishedBy NVARCHAR(100) NOT NULL,
        ReleaseNotes NVARCHAR(MAX),
        IsActive BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_WorkflowVersions_Workflows FOREIGN KEY (WorkflowId) 
            REFERENCES Workflows(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_WorkflowVersions_Version UNIQUE (WorkflowId, VersionNumber)
    );
    
    CREATE INDEX IX_WorkflowVersions_WorkflowId ON WorkflowVersions(WorkflowId);
    CREATE INDEX IX_WorkflowVersions_IsActive ON WorkflowVersions(IsActive);
END
GO

-- Create WorkflowAuditLogs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WorkflowAuditLogs')
BEGIN
    CREATE TABLE WorkflowAuditLogs (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        WorkflowId UNIQUEIDENTIFIER NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        Timestamp DATETIME2 NOT NULL,
        UserId NVARCHAR(100) NOT NULL,
        Details NVARCHAR(MAX),
        CONSTRAINT FK_WorkflowAuditLogs_Workflows FOREIGN KEY (WorkflowId) 
            REFERENCES Workflows(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_WorkflowAuditLogs_WorkflowId ON WorkflowAuditLogs(WorkflowId);
    CREATE INDEX IX_WorkflowAuditLogs_Timestamp ON WorkflowAuditLogs(Timestamp);
END
GO
