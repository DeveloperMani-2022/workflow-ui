import React, { useState, useEffect, useCallback } from 'react';
import WorkflowCanvas from './components/WorkflowCanvas';
import Toolbar from './components/Toolbar';
import { workflowAPI } from './utils/apiClient';
import './App.css';

function App() {
    const [graphData, setGraphData] = useState({ cells: [] });
    const [workflowId, setWorkflowId] = useState(null);
    const [workflowName, setWorkflowName] = useState('Untitled Workflow');
    const [validationResult, setValidationResult] = useState(null);
    const [executionResult, setExecutionResult] = useState(null);

    // Callback when the graph changes in the canvas
    const handleGraphChange = useCallback((newGraphData) => {
        setGraphData(newGraphData);
    }, []);

    const handleSave = async () => {
        try {
            const graphJson = JSON.stringify(graphData);

            if (workflowId) {
                await workflowAPI.updateWorkflow(workflowId, {
                    Name: workflowName,
                    GraphJson: graphJson
                });
                alert('Workflow updated successfully!');
            } else {
                const result = await workflowAPI.createWorkflow({
                    Name: workflowName,
                    Description: 'Created from Agentic AI Builder',
                    GraphJson: graphJson
                });
                setWorkflowId(result.Id);
                alert('Workflow created successfully!');
            }
        } catch (error) {
            console.error('Error saving workflow:', error);
            alert('Error saving workflow: ' + error.message);
        }
    };

    const handleValidate = async () => {
        if (!workflowId) {
            alert('Please save the workflow first');
            return;
        }

        try {
            const result = await workflowAPI.validateWorkflow(workflowId);
            setValidationResult(result);

            if (result.IsValid) {
                alert('Workflow is valid!');
            } else {
                alert(`Validation failed with ${result.Errors.length} errors`);
            }
        } catch (error) {
            console.error('Error validating workflow:', error);
            alert('Error validating workflow: ' + error.message);
        }
    };

    const handleExecute = async () => {
        if (!workflowId) {
            alert('Please save the workflow first');
            return;
        }

        try {
            const result = await workflowAPI.executeWorkflow({
                WorkflowId: workflowId,
                InitialState: {},
                UserId: 'test-user'
            });

            setExecutionResult(result);

            if (result.Success) {
                if (result.IsComplete) {
                    alert('Workflow executed successfully!');
                } else if (result.RequiresUserInput) {
                    const userInput = prompt(result.Message);
                    if (userInput) {
                        const continueResult = await workflowAPI.executeWorkflow({
                            WorkflowId: workflowId,
                            SessionId: result.SessionId,
                            UserInput: userInput,
                            UserId: 'test-user'
                        });
                        setExecutionResult(continueResult);
                    }
                }
            } else {
                alert('Execution failed: ' + result.ErrorMessage);
            }
        } catch (error) {
            console.error('Error executing workflow:', error);
            alert('Error executing workflow: ' + error.message);
        }
    };

    const handleExport = () => {
        const dataStr = JSON.stringify(graphData, null, 2);
        const dataUri = 'data:application/json;charset=utf-8,' + encodeURIComponent(dataStr);
        const exportFileDefaultName = `${workflowName}.json`;

        const linkElement = document.createElement('a');
        linkElement.setAttribute('href', dataUri);
        linkElement.setAttribute('download', exportFileDefaultName);
        linkElement.click();
    };

    const handleImport = (event) => {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                try {
                    const importedGraph = JSON.parse(e.target.result);
                    setGraphData(importedGraph);
                    alert('Workflow imported successfully!');
                } catch (error) {
                    alert('Error importing workflow: ' + error.message);
                }
            };
            reader.readAsText(file);
        }
    };

    return (
        <div className="app">
            <header className="app-header">
                <h1>Agentic AI Workflow Builder</h1>
                <input
                    type="text"
                    value={workflowName}
                    onChange={(e) => setWorkflowName(e.target.value)}
                    className="workflow-name-input"
                    placeholder="Workflow Name"
                />
            </header>

            <div className="app-content">
                <div className="main-area">
                    <Toolbar
                        onSave={handleSave}
                        onValidate={handleValidate}
                        onExecute={handleExecute}
                        onExport={handleExport}
                        onImport={handleImport}
                    />

                    <WorkflowCanvas
                        graph={graphData}
                        onGraphChange={handleGraphChange}
                    />
                </div>
            </div>

            {validationResult && (
                <div className="validation-panel">
                    <h3>Validation Results</h3>
                    <p>Valid: {validationResult.IsValid ? 'Yes' : 'No'}</p>
                    {validationResult.Errors && validationResult.Errors.length > 0 && (
                        <div>
                            <h4>Errors:</h4>
                            <ul>
                                {validationResult.Errors.map((error, idx) => (
                                    <li key={idx}>{error.Message}</li>
                                ))}
                            </ul>
                        </div>
                    )}
                </div>
            )}

            {executionResult && (
                <div className="execution-panel">
                    <h3>Execution Results</h3>
                    <p>Success: {executionResult.Success ? 'Yes' : 'No'}</p>
                    <p>Complete: {executionResult.IsComplete ? 'Yes' : 'No'}</p>
                    {executionResult.Message && <p>Message: {executionResult.Message}</p>}
                    {executionResult.ErrorMessage && <p>Error: {executionResult.ErrorMessage}</p>}
                </div>
            )}
        </div>
    );
}

export default App;

