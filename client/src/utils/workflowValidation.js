/**
 * Validates a workflow graph before saving or executing
 * @param {Object} graph - RappidJS graph object
 * @returns {Object} - Validation result with isValid flag and errors/warnings arrays
 */
export function validateWorkflow(graph) {
    const result = {
        isValid: true,
        errors: [],
        warnings: []
    };

    const cells = graph.getCells();
    const nodes = cells.filter(cell => cell.isElement());
    const links = cells.filter(cell => cell.isLink());

    // Check for start node
    const startNodes = nodes.filter(node =>
        node.attributes.type === 'StartNode' || node.attributes.type.includes('Start')
    );

    if (startNodes.length === 0) {
        result.errors.push({
            code: 'NO_START_NODE',
            message: 'Workflow must have at least one start node'
        });
        result.isValid = false;
    } else if (startNodes.length > 1) {
        result.warnings.push({
            code: 'MULTIPLE_START_NODES',
            message: 'Workflow has multiple start nodes. Only the first one will be used.'
        });
    }

    // Check for end node
    const endNodes = nodes.filter(node =>
        node.attributes.type === 'EndNode' || node.attributes.type.includes('End')
    );

    if (endNodes.length === 0) {
        result.errors.push({
            code: 'NO_END_NODE',
            message: 'Workflow must have at least one end node'
        });
        result.isValid = false;
    }

    // Check for orphan nodes (nodes with no connections)
    const connectedNodeIds = new Set();
    links.forEach(link => {
        const source = link.get('source');
        const target = link.get('target');
        if (source && source.id) connectedNodeIds.add(source.id);
        if (target && target.id) connectedNodeIds.add(target.id);
    });

    nodes.forEach(node => {
        const nodeType = node.attributes.type;
        // Skip start and end nodes for orphan check
        if (nodeType !== 'StartNode' && nodeType !== 'EndNode' &&
            !nodeType.includes('Start') && !nodeType.includes('End')) {
            if (!connectedNodeIds.has(node.id)) {
                result.warnings.push({
                    code: 'ORPHAN_NODE',
                    message: `Node '${node.attr('label/text') || node.id}' is not connected to any other nodes`,
                    nodeId: node.id
                });
            }
        }
    });

    // Check for nodes with no outgoing connections (except end nodes)
    const nodesWithOutgoing = new Set();
    links.forEach(link => {
        const source = link.get('source');
        if (source && source.id) {
            nodesWithOutgoing.add(source.id);
        }
    });

    nodes.forEach(node => {
        const nodeType = node.attributes.type;
        if (nodeType !== 'EndNode' && !nodeType.includes('End')) {
            if (!nodesWithOutgoing.has(node.id)) {
                result.warnings.push({
                    code: 'NO_OUTGOING_CONNECTION',
                    message: `Node '${node.attr('label/text') || node.id}' has no outgoing connections`,
                    nodeId: node.id
                });
            }
        }
    });

    // Validate node-specific configurations
    nodes.forEach(node => {
        const nodeValidation = validateNodeConfiguration(node);
        if (!nodeValidation.isValid) {
            result.errors.push(...nodeValidation.errors);
            result.isValid = false;
        }
        result.warnings.push(...nodeValidation.warnings);
    });

    return result;
}

/**
 * Validates individual node configuration
 * @param {Object} node - RappidJS node element
 * @returns {Object} - Validation result
 */
function validateNodeConfiguration(node) {
    const result = {
        isValid: true,
        errors: [],
        warnings: []
    };

    const nodeType = node.attributes.type;
    const data = node.get('data') || {};

    switch (nodeType) {
        case 'APICallNode':
            if (!data.apiUrl || data.apiUrl.trim() === '') {
                result.errors.push({
                    code: 'MISSING_API_URL',
                    message: `API Call node '${node.attr('label/text') || node.id}' is missing API URL`,
                    nodeId: node.id
                });
                result.isValid = false;
            }
            break;

        case 'QuestionNode':
            if (!data.promptText || data.promptText.trim() === '') {
                result.warnings.push({
                    code: 'MISSING_PROMPT',
                    message: `Question node '${node.attr('label/text') || node.id}' is missing prompt text`,
                    nodeId: node.id
                });
            }
            break;

        case 'ConditionNode':
            if (!data.branches || data.branches.length === 0) {
                result.errors.push({
                    code: 'MISSING_CONDITIONS',
                    message: `Condition node '${node.attr('label/text') || node.id}' has no condition branches defined`,
                    nodeId: node.id
                });
                result.isValid = false;
            }
            break;
    }

    return result;
}
