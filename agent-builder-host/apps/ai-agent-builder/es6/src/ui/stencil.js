import * as joint from 'jointjs';
import { AgentNode, ToolNode, ConditionNode, StartNode, EndNode } from '../shapes/index.js';

export class Stencil {
    constructor(options) {
        this.container = options.container;
        this.paper = options.paper;
        this.init();
    }

    init() {
        // Create stencil shapes
        const shapes = [
            new StartNode(),
            new AgentNode(),
            new ToolNode(),
            new ConditionNode(),
            new EndNode()
        ];

        // Render stencil items
        this.container.innerHTML = `
            <div style="padding: 20px;">
                <h3>Agent Builder</h3>
                <div id="stencil-items"></div>
            </div>
        `;

        const itemsContainer = this.container.querySelector('#stencil-items');

        shapes.forEach(shape => {
            const item = document.createElement('div');
            item.className = 'stencil-item';
            item.textContent = shape.attr('label/text') || 'Node';
            item.style.cssText = 'padding: 10px; margin: 5px; background: #fff; border: 1px solid #ccc; cursor: pointer;';

            item.addEventListener('click', () => {
                const clone = shape.clone();
                clone.position(100, 100);
                this.paper.model.addCell(clone);
            });

            itemsContainer.appendChild(item);
        });
    }
}
