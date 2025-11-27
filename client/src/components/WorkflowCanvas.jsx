import React, { useEffect, useRef, useState } from 'react';
import * as joint from 'jointjs';
import { AgentShape, ToolShape } from '../shapes/AgentShapes';
import './WorkflowCanvas.css'; // We'll create this next

const WorkflowCanvas = ({ graph: initialGraphData, onGraphChange }) => {
    const canvasRef = useRef(null);
    const paperRef = useRef(null);
    const graphRef = useRef(null);

    useEffect(() => {
        if (!canvasRef.current) return;

        // 1. Initialize Graph
        const graph = new joint.dia.Graph();
        graphRef.current = graph;

        // 2. Initialize Paper
        const paper = new joint.dia.Paper({
            el: canvasRef.current,
            model: graph,
            width: '100%',
            height: '100%',
            gridSize: 10,
            drawGrid: true,
            background: {
                color: '#f5f5f5'
            },
            defaultLink: new joint.shapes.standard.Link({
                attrs: {
                    line: {
                        stroke: '#333',
                        strokeWidth: 2
                    }
                }
            })
        });
        paperRef.current = paper;

        // 3. Load initial data if any
        if (initialGraphData && initialGraphData.cells) {
            graph.fromJSON(initialGraphData);
        }

        // 4. Listen for changes
        graph.on('change add remove', () => {
            if (onGraphChange) {
                onGraphChange(graph.toJSON());
            }
        });

        // 5. Handle resizing
        const resizeObserver = new ResizeObserver(() => {
            if (canvasRef.current) {
                paper.setDimensions(canvasRef.current.clientWidth, canvasRef.current.clientHeight);
            }
        });
        resizeObserver.observe(canvasRef.current);

        return () => {
            resizeObserver.disconnect();
            paper.remove();
        };
    }, []);

    // Update graph if initialGraphData changes externally (e.g. load)
    useEffect(() => {
        if (graphRef.current && initialGraphData && initialGraphData.cells) {
            // Avoid infinite loop if the change came from us
            // For simplicity, we might just reload if it's significantly different
            // or rely on a separate "load" trigger. 
            // Here we'll just check if it's empty to avoid wiping work in progress
            if (graphRef.current.getCells().length === 0) {
                graphRef.current.fromJSON(initialGraphData);
            }
        }
    }, [initialGraphData]);

    const handleDragStart = (event, type) => {
        event.dataTransfer.setData('nodeType', type);
    };

    const handleDrop = (event) => {
        event.preventDefault();
        const nodeType = event.dataTransfer.getData('nodeType');
        const rect = canvasRef.current.getBoundingClientRect();
        const x = event.clientX - rect.left;
        const y = event.clientY - rect.top;

        let cell;
        if (nodeType === 'agent') {
            cell = new AgentShape({
                position: { x, y },
                size: { width: 100, height: 60 },
                attrs: {
                    headerText: { text: 'New Agent' },
                    label: { text: 'Description' }
                }
            });
        } else if (nodeType === 'tool') {
            cell = new ToolShape({
                position: { x, y },
                size: { width: 100, height: 40 },
                attrs: {
                    label: { text: 'New Tool' }
                }
            });
        }

        if (cell) {
            graphRef.current.addCell(cell);
        }
    };

    const handleDragOver = (event) => {
        event.preventDefault();
    };

    return (
        <div className="workflow-builder-container">
            <div className="stencil-sidebar">
                <h3>Components</h3>
                <div
                    className="stencil-item agent"
                    draggable
                    onDragStart={(e) => handleDragStart(e, 'agent')}
                >
                    Agent Node
                </div>
                <div
                    className="stencil-item tool"
                    draggable
                    onDragStart={(e) => handleDragStart(e, 'tool')}
                >
                    Tool Node
                </div>
            </div>
            <div
                className="paper-container"
                ref={canvasRef}
                onDrop={handleDrop}
                onDragOver={handleDragOver}
            />
        </div>
    );
};

export default WorkflowCanvas;
