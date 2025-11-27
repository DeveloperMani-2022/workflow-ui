import React, { useState, useEffect } from 'react';
import './NodeInspector.css';

const NodeInspector = ({ selectedNode, onNodeUpdate }) => {
    const [formData, setFormData] = useState({});

    useEffect(() => {
        if (selectedNode) {
            setFormData(selectedNode.data || {});
        }
    }, [selectedNode]);

    if (!selectedNode) {
        return (
            <div className="node-inspector">
                <h3>Node Inspector</h3>
                <p className="no-selection">Select a node to edit its properties</p>
            </div>
        );
    }

    const handleChange = (field, value) => {
        const newData = { ...formData, [field]: value };
        setFormData(newData);
        onNodeUpdate(selectedNode.id, { data: newData });
    };

    const handleLabelChange = (value) => {
        onNodeUpdate(selectedNode.id, { label: value });
    };

    const renderFields = () => {
        switch (selectedNode.type) {
            case 'MessageNode':
                return (
                    <div className="form-group">
                        <label>Message Text:</label>
                        <textarea
                            value={formData.messageText || ''}
                            onChange={(e) => handleChange('messageText', e.target.value)}
                            rows={4}
                            placeholder="Enter message text..."
                        />
                        <small>Supports template variables: {'{variableName}'}</small>
                    </div>
                );

            case 'QuestionNode':
                return (
                    <>
                        <div className="form-group">
                            <label>Prompt Text:</label>
                            <textarea
                                value={formData.promptText || ''}
                                onChange={(e) => handleChange('promptText', e.target.value)}
                                rows={3}
                                placeholder="Enter question prompt..."
                            />
                        </div>
                        <div className="form-group">
                            <label>State Key:</label>
                            <input
                                type="text"
                                value={formData.stateKey || ''}
                                onChange={(e) => handleChange('stateKey', e.target.value)}
                                placeholder="userResponse"
                            />
                            <small>Variable name to store the answer</small>
                        </div>
                        <div className="form-group">
                            <label>
                                <input
                                    type="checkbox"
                                    checked={formData.validationRules?.required || false}
                                    onChange={(e) => handleChange('validationRules', {
                                        ...formData.validationRules,
                                        required: e.target.checked
                                    })}
                                />
                                Required
                            </label>
                        </div>
                    </>
                );

            case 'APICallNode':
                return (
                    <>
                        <div className="form-group">
                            <label>API URL:</label>
                            <input
                                type="text"
                                value={formData.apiUrl || ''}
                                onChange={(e) => handleChange('apiUrl', e.target.value)}
                                placeholder="https://api.example.com/endpoint"
                            />
                        </div>
                        <div className="form-group">
                            <label>HTTP Method:</label>
                            <select
                                value={formData.method || 'GET'}
                                onChange={(e) => handleChange('method', e.target.value)}
                            >
                                <option value="GET">GET</option>
                                <option value="POST">POST</option>
                                <option value="PUT">PUT</option>
                                <option value="PATCH">PATCH</option>
                                <option value="DELETE">DELETE</option>
                            </select>
                        </div>
                        <div className="form-group">
                            <label>Response State Key:</label>
                            <input
                                type="text"
                                value={formData.responseStateKey || ''}
                                onChange={(e) => handleChange('responseStateKey', e.target.value)}
                                placeholder="apiResponse"
                            />
                        </div>
                    </>
                );

            case 'ConditionNode':
                return (
                    <div className="form-group">
                        <label>Condition Branches:</label>
                        <textarea
                            value={JSON.stringify(formData.branches || [], null, 2)}
                            onChange={(e) => {
                                try {
                                    const branches = JSON.parse(e.target.value);
                                    handleChange('branches', branches);
                                } catch (err) {
                                    // Invalid JSON, ignore
                                }
                            }}
                            rows={6}
                            placeholder='[{"condition": "age > 18", "port": "true", "label": "Adult"}]'
                        />
                        <small>JSON array of condition branches</small>
                    </div>
                );

            case 'FunctionNode':
                return (
                    <>
                        <div className="form-group">
                            <label>Instruction Prompt:</label>
                            <textarea
                                value={formData.instructionPrompt || ''}
                                onChange={(e) => handleChange('instructionPrompt', e.target.value)}
                                rows={4}
                                placeholder="Enter LLM instruction..."
                            />
                        </div>
                        <div className="form-group">
                            <label>Model Name:</label>
                            <input
                                type="text"
                                value={formData.modelName || 'gpt-4'}
                                onChange={(e) => handleChange('modelName', e.target.value)}
                                placeholder="gpt-4"
                            />
                        </div>
                        <div className="form-group">
                            <label>Input Mapping (JSON):</label>
                            <textarea
                                value={JSON.stringify(formData.inputMapping || {}, null, 2)}
                                onChange={(e) => {
                                    try {
                                        const mapping = JSON.parse(e.target.value);
                                        handleChange('inputMapping', mapping);
                                    } catch (err) {
                                        // Invalid JSON
                                    }
                                }}
                                rows={3}
                                placeholder='{"userQuery": "question"}'
                            />
                        </div>
                    </>
                );

            case 'StateUpdateNode':
                return (
                    <div className="form-group">
                        <label>State Updates (JSON):</label>
                        <textarea
                            value={JSON.stringify(formData.updates || {}, null, 2)}
                            onChange={(e) => {
                                try {
                                    const updates = JSON.parse(e.target.value);
                                    handleChange('updates', updates);
                                } catch (err) {
                                    // Invalid JSON
                                }
                            }}
                            rows={4}
                            placeholder='{"counter": 1, "status": "active"}'
                        />
                        <small>Key-value pairs to update in workflow state</small>
                    </div>
                );

            default:
                return <p>No configuration needed for this node type.</p>;
        }
    };

    return (
        <div className="node-inspector">
            <h3>Node Inspector</h3>

            <div className="node-info">
                <div className="form-group">
                    <label>Node Type:</label>
                    <input type="text" value={selectedNode.type} disabled />
                </div>

                <div className="form-group">
                    <label>Node Label:</label>
                    <input
                        type="text"
                        value={selectedNode.label}
                        onChange={(e) => handleLabelChange(e.target.value)}
                        placeholder="Node label"
                    />
                </div>
            </div>

            <div className="node-properties">
                <h4>Properties</h4>
                {renderFields()}
            </div>
        </div>
    );
};

export default NodeInspector;
