import React from 'react';
import './NodePalette.css';

const NodePalette = ({ onAddNode }) => {
    const nodeTypes = [
        { type: 'StartNode', label: 'Start', icon: '▶', color: '#4CAF50' },
        { type: 'EndNode', label: 'End', icon: '⏹', color: '#F44336' },
        { type: 'MessageNode', label: 'Message', icon: '💬', color: '#2196F3' },
        { type: 'QuestionNode', label: 'Question', icon: '❓', color: '#FF9800' },
        { type: 'APICallNode', label: 'API Call', icon: '🌐', color: '#9C27B0' },
        { type: 'ConditionNode', label: 'Condition', icon: '🔀', color: '#FFC107' },
        { type: 'FunctionNode', label: 'LLM/Agent', icon: '🤖', color: '#00BCD4' },
        { type: 'StateUpdateNode', label: 'State Update', icon: '💾', color: '#795548' },
        { type: 'DecisionSplitNode', label: 'Decision', icon: '⚡', color: '#E91E63' }
    ];

    return (
        <div className="node-palette">
            <h3>Node Palette</h3>
            <div className="node-list">
                {nodeTypes.map(nodeType => (
                    <div
                        key={nodeType.type}
                        className="palette-node"
                        style={{ backgroundColor: nodeType.color }}
                        onClick={() => onAddNode(nodeType.type)}
                        title={`Add ${nodeType.label} node`}
                    >
                        <span className="node-icon">{nodeType.icon}</span>
                        <span className="node-label">{nodeType.label}</span>
                    </div>
                ))}
            </div>
            <div className="palette-instructions">
                <p>Click a node to add it to the canvas</p>
                <p>Drag nodes to reposition</p>
                <p>Click output port and drag to input port to connect</p>
            </div>
        </div>
    );
};

export default NodePalette;
