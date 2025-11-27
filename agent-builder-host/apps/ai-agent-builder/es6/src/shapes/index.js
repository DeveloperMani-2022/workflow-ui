import * as joint from 'jointjs';

export class AgentNode extends joint.shapes.standard.Rectangle {
    defaults() {
        return {
            ...super.defaults,
            type: 'agent',
            size: { width: 120, height: 60 },
            attrs: {
                body: {
                    fill: '#4CAF50',
                    stroke: '#388E3C',
                    strokeWidth: 2
                },
                label: {
                    text: 'Agent',
                    fill: '#fff',
                    fontSize: 14
                }
            }
        };
    }
}

export class ToolNode extends joint.shapes.standard.Rectangle {
    defaults() {
        return {
            ...super.defaults,
            type: 'tool',
            size: { width: 120, height: 60 },
            attrs: {
                body: {
                    fill: '#2196F3',
                    stroke: '#1976D2',
                    strokeWidth: 2
                },
                label: {
                    text: 'Tool',
                    fill: '#fff',
                    fontSize: 14
                }
            }
        };
    }
}

export class ConditionNode extends joint.shapes.standard.Rectangle {
    defaults() {
        return {
            ...super.defaults,
            type: 'condition',
            size: { width: 120, height: 60 },
            attrs: {
                body: {
                    fill: '#FF9800',
                    stroke: '#F57C00',
                    strokeWidth: 2
                },
                label: {
                    text: 'Condition',
                    fill: '#fff',
                    fontSize: 14
                }
            }
        };
    }
}

export class StartNode extends joint.shapes.standard.Circle {
    defaults() {
        return {
            ...super.defaults,
            type: 'start',
            size: { width: 60, height: 60 },
            attrs: {
                body: {
                    fill: '#8BC34A',
                    stroke: '#689F38',
                    strokeWidth: 2
                },
                label: {
                    text: 'Start',
                    fill: '#fff',
                    fontSize: 12
                }
            }
        };
    }
}

export class EndNode extends joint.shapes.standard.Circle {
    defaults() {
        return {
            ...super.defaults,
            type: 'end',
            size: { width: 60, height: 60 },
            attrs: {
                body: {
                    fill: '#F44336',
                    stroke: '#D32F2F',
                    strokeWidth: 2
                },
                label: {
                    text: 'End',
                    fill: '#fff',
                    fontSize: 12
                }
            }
        };
    }
}
