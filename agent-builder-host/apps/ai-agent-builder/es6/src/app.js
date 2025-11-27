import * as joint from 'jointjs';
import { Graph } from './graph/graph.js';
import { Paper } from './paper/paper.js';
import { Stencil } from './ui/stencil.js';
import { Toolbar } from './ui/toolbar.js';

export class App {
    constructor(options) {
        this.container = options.container;
        this.apiBaseUrl = options.apiBaseUrl;
        this.graph = null;
        this.paper = null;
        this.stencil = null;
        this.toolbar = null;
    }

    init() {
        // Create the graph
        this.graph = new Graph();

        // Create the paper (canvas)
        this.paper = new Paper({
            graph: this.graph.getJointGraph(),
            container: this.container.querySelector('#paper-container')
        });

        // Create the stencil (palette)
        this.stencil = new Stencil({
            container: this.container.querySelector('#stencil-container'),
            paper: this.paper.getJointPaper()
        });

        // Create the toolbar
        this.toolbar = new Toolbar({
            container: this.container.querySelector('#toolbar-container'),
            onSave: () => this.saveWorkflow(),
            onLoad: () => this.loadWorkflow(),
            onClear: () => this.clearWorkflow()
        });

        console.log('App initialized with API:', this.apiBaseUrl);
    }

    async saveWorkflow() {
        const graphData = this.graph.toJSON();
        console.log('Saving workflow:', graphData);

        try {
            const response = await fetch(this.apiBaseUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    name: 'My Workflow',
                    description: 'Created from AI Agent Builder',
                    graphJson: JSON.stringify(graphData)
                })
            });

            if (response.ok) {
                console.log('Workflow saved successfully');
                alert('Workflow saved!');
            } else {
                console.error('Failed to save workflow:', response.statusText);
                alert('Failed to save workflow');
            }
        } catch (error) {
            console.error('Error saving workflow:', error);
            alert('Error: ' + error.message);
        }
    }

    async loadWorkflow() {
        console.log('Load workflow - to be implemented');
    }

    clearWorkflow() {
        this.graph.clear();
        console.log('Workflow cleared');
    }
}
