import * as joint from 'jointjs';

export const AgentShape = joint.dia.Element.define('app.Agent', {
    attrs: {
        body: {
            refWidth: '100%',
            refHeight: '100%',
            strokeWidth: 2,
            stroke: '#333333',
            fill: '#FFFFFF',
            rx: 10,
            ry: 10
        },
        header: {
            refWidth: '100%',
            height: 30,
            strokeWidth: 2,
            stroke: '#333333',
            fill: '#4A90E2',
            rx: 10,
            ry: 10
        },
        headerText: {
            textVerticalAnchor: 'middle',
            textAnchor: 'middle',
            refX: '50%',
            refY: 15,
            fontSize: 14,
            fill: '#FFFFFF',
            fontWeight: 'bold',
            text: 'Agent'
        },
        label: {
            textVerticalAnchor: 'middle',
            textAnchor: 'middle',
            refX: '50%',
            refY: '60%',
            fontSize: 12,
            fill: '#333333',
            text: 'Description'
        }
    }
}, {
    markup: [{
        tagName: 'rect',
        selector: 'body'
    }, {
        tagName: 'rect',
        selector: 'header'
    }, {
        tagName: 'text',
        selector: 'headerText'
    }, {
        tagName: 'text',
        selector: 'label'
    }]
});

export const ToolShape = joint.dia.Element.define('app.Tool', {
    attrs: {
        body: {
            refWidth: '100%',
            refHeight: '100%',
            strokeWidth: 2,
            stroke: '#333333',
            fill: '#FFFFFF',
            rx: 5,
            ry: 5
        },
        icon: {
            width: 20,
            height: 20,
            x: 10,
            y: 10,
            fill: '#F5A623'
        },
        label: {
            textVerticalAnchor: 'middle',
            textAnchor: 'start',
            refX: 40,
            refY: '50%',
            fontSize: 12,
            fill: '#333333',
            text: 'Tool'
        }
    }
}, {
    markup: [{
        tagName: 'rect',
        selector: 'body'
    }, {
        tagName: 'rect',
        selector: 'icon'
    }, {
        tagName: 'text',
        selector: 'label'
    }]
});
