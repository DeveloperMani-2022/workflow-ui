import React, { useRef, useEffect, useState } from 'react';
import './Canvas.css';

const Canvas = ({ graph, setGraph, selectedNode, onNodeSelect, onAddEdge }) => {
    const canvasRef = useRef(null);
    const [draggingNode, setDraggingNode] = useState(null);
    const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
    const [connectingFrom, setConnectingFrom] = useState(null);

    useEffect(() => {
        drawCanvas();
    }, [graph, selectedNode]);

    const drawCanvas = () => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // Draw grid
        drawGrid(ctx, canvas.width, canvas.height);

        // Draw edges
        graph.edges.forEach(edge => {
            const sourceNode = graph.nodes.find(n => n.id === edge.source);
            const targetNode = graph.nodes.find(n => n.id === edge.target);
            if (sourceNode && targetNode) {
                drawEdge(ctx, sourceNode, targetNode);
            }
        });

        // Draw nodes
        graph.nodes.forEach(node => {
            drawNode(ctx, node, node.id === selectedNode?.id);
        });
    };

    const drawGrid = (ctx, width, height) => {
        const gridSize = 20;
        ctx.strokeStyle = '#e0e0e0';
        ctx.lineWidth = 0.5;

        for (let x = 0; x <= width; x += gridSize) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, height);
            ctx.stroke();
        }

        for (let y = 0; y <= height; y += gridSize) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(width, y);
            ctx.stroke();
        }
    };

    const drawNode = (ctx, node, isSelected) => {
        const { x, y } = node.position;
        const width = 150;
        const height = 60;

        // Node background
        ctx.fillStyle = getNodeColor(node.type);
        ctx.fillRect(x, y, width, height);

        // Border
        ctx.strokeStyle = isSelected ? '#2196F3' : '#333';
        ctx.lineWidth = isSelected ? 3 : 1;
        ctx.strokeRect(x, y, width, height);

        // Label
        ctx.fillStyle = '#fff';
        ctx.font = '14px Arial';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(node.label, x + width / 2, y + height / 2);

        // Connection ports
        ctx.fillStyle = '#fff';
        ctx.strokeStyle = '#333';
        ctx.lineWidth = 1;

        // Input port (left)
        if (node.type !== 'StartNode') {
            ctx.beginPath();
            ctx.arc(x, y + height / 2, 5, 0, 2 * Math.PI);
            ctx.fill();
            ctx.stroke();
        }

        // Output port (right)
        if (node.type !== 'EndNode') {
            ctx.beginPath();
            ctx.arc(x + width, y + height / 2, 5, 0, 2 * Math.PI);
            ctx.fill();
            ctx.stroke();
        }
    };

    const drawEdge = (ctx, sourceNode, targetNode) => {
        const sourceX = sourceNode.position.x + 150;
        const sourceY = sourceNode.position.y + 30;
        const targetX = targetNode.position.x;
        const targetY = targetNode.position.y + 30;

        ctx.strokeStyle = '#666';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(sourceX, sourceY);

        // Bezier curve for smooth connection
        const controlX1 = sourceX + 50;
        const controlX2 = targetX - 50;
        ctx.bezierCurveTo(controlX1, sourceY, controlX2, targetY, targetX, targetY);
        ctx.stroke();

        // Arrow head
        const angle = Math.atan2(targetY - sourceY, targetX - sourceX);
        ctx.beginPath();
        ctx.moveTo(targetX, targetY);
        ctx.lineTo(targetX - 10 * Math.cos(angle - Math.PI / 6), targetY - 10 * Math.sin(angle - Math.PI / 6));
        ctx.lineTo(targetX - 10 * Math.cos(angle + Math.PI / 6), targetY - 10 * Math.sin(angle + Math.PI / 6));
        ctx.closePath();
        ctx.fillStyle = '#666';
        ctx.fill();
    };

    const getNodeColor = (nodeType) => {
        const colors = {
            'StartNode': '#4CAF50',
            'EndNode': '#F44336',
            'MessageNode': '#2196F3',
            'QuestionNode': '#FF9800',
            'APICallNode': '#9C27B0',
            'ConditionNode': '#FFC107',
            'FunctionNode': '#00BCD4',
            'StateUpdateNode': '#795548',
            'DecisionSplitNode': '#E91E63'
        };
        return colors[nodeType] || '#607D8B';
    };

    const handleMouseDown = (e) => {
        const rect = canvasRef.current.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        // Check if clicking on a node
        const clickedNode = graph.nodes.find(node => {
            const nodeX = node.position.x;
            const nodeY = node.position.y;
            return x >= nodeX && x <= nodeX + 150 && y >= nodeY && y <= nodeY + 60;
        });

        if (clickedNode) {
            // Check if clicking on output port
            const outputPortX = clickedNode.position.x + 150;
            const outputPortY = clickedNode.position.y + 30;
            const distToPort = Math.sqrt((x - outputPortX) ** 2 + (y - outputPortY) ** 2);

            if (distToPort < 10 && clickedNode.type !== 'EndNode') {
                setConnectingFrom(clickedNode.id);
            } else {
                onNodeSelect(clickedNode);
                setDraggingNode(clickedNode.id);
                setDragOffset({
                    x: x - clickedNode.position.x,
                    y: y - clickedNode.position.y
                });
            }
        } else {
            onNodeSelect(null);
        }
    };

    const handleMouseMove = (e) => {
        if (draggingNode) {
            const rect = canvasRef.current.getBoundingClientRect();
            const x = e.clientX - rect.left - dragOffset.x;
            const y = e.clientY - rect.top - dragOffset.y;

            setGraph(prevGraph => ({
                ...prevGraph,
                nodes: prevGraph.nodes.map(node =>
                    node.id === draggingNode
                        ? { ...node, position: { x, y } }
                        : node
                )
            }));
        }
    };

    const handleMouseUp = (e) => {
        if (connectingFrom) {
            const rect = canvasRef.current.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            // Check if releasing on a node's input port
            const targetNode = graph.nodes.find(node => {
                if (node.id === connectingFrom || node.type === 'StartNode') return false;
                const inputPortX = node.position.x;
                const inputPortY = node.position.y + 30;
                const dist = Math.sqrt((x - inputPortX) ** 2 + (y - inputPortY) ** 2);
                return dist < 15;
            });

            if (targetNode) {
                onAddEdge(connectingFrom, targetNode.id);
            }

            setConnectingFrom(null);
        }

        setDraggingNode(null);
    };

    return (
        <div className="canvas-container">
            <canvas
                ref={canvasRef}
                width={1200}
                height={800}
                onMouseDown={handleMouseDown}
                onMouseMove={handleMouseMove}
                onMouseUp={handleMouseUp}
                className="workflow-canvas"
            />
        </div>
    );
};

export default Canvas;
