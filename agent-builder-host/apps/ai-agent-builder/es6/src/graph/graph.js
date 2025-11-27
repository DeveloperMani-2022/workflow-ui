import * as joint from 'jointjs';

export class Graph {
    constructor() {
        this.graph = new joint.dia.Graph();
    }

    getJointGraph() {
        return this.graph;
    }

    toJSON() {
        return this.graph.toJSON();
    }

    fromJSON(json) {
        this.graph.fromJSON(json);
    }

    clear() {
        this.graph.clear();
    }
}
