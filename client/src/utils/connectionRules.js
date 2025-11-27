/**
 * Validates if a connection between two nodes is allowed
 * @param {Object} source - Source node
 * @param {Object} target - Target node
 * @param {string} sourcePort - Source port name
 * @param {string} targetPort - Target port name
 * @returns {boolean} - True if connection is valid
 */
export function validateConnection(source, target, sourcePort, targetPort) {
    // Prevent self-connections
    if (source.id === target.id) {
        return false;
    }

    // Start nodes can only have outgoing connections
    if (target.attributes.type === 'StartNode') {
        return false;
    }

    // End nodes can only have incoming connections
    if (source.attributes.type === 'EndNode') {
        return false;
    }

    // Prevent duplicate connections
    const existingLinks = source.graph.getConnectedLinks(source);
    const duplicateExists = existingLinks.some(link => {
        return link.get('source').id === source.id &&
            link.get('target').id === target.id &&
            link.get('source').port === sourcePort &&
            link.get('target').port === targetPort;
    });

    if (duplicateExists) {
        return false;
    }

    return true;
}

/**
 * Gets allowed target node types for a given source node type
 * @param {string} sourceType - Source node type
 * @returns {Array<string>} - Array of allowed target node types
 */
export function getAllowedTargets(sourceType) {
    const rules = {
        'StartNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode'],
        'MessageNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode', 'EndNode'],
        'QuestionNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode', 'EndNode'],
        'APICallNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode', 'EndNode'],
        'ConditionNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode', 'EndNode'],
        'FunctionNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode', 'EndNode'],
        'StateUpdateNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode', 'EndNode'],
        'DecisionSplitNode': ['MessageNode', 'QuestionNode', 'APICallNode', 'FunctionNode', 'StateUpdateNode', 'ConditionNode', 'EndNode'],
        'EndNode': []
    };

    return rules[sourceType] || [];
}
