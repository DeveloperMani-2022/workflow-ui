import * as joint from 'jointjs';

export class Paper {
    constructor(options) {
        this.paper = new joint.dia.Paper({
            el: options.container,
            model: options.graph,
            width: '100%',
            height: '100%',
            gridSize: 10,
            drawGrid: true,
            background: {
                color: '#f5f5f5'
            },
            interactive: true
        });
    }

    getJointPaper() {
        return this.paper;
    }
}
