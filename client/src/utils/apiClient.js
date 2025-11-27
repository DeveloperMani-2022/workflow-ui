import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Workflow API calls
export const workflowAPI = {
    // Create a new workflow
    createWorkflow: async (workflow) => {
        const response = await apiClient.post('/workflow', workflow);
        return response.data;
    },

    // Get workflow by ID
    getWorkflow: async (id) => {
        const response = await apiClient.get(`/workflow/${id}`);
        return response.data;
    },

    // Get all workflows
    listWorkflows: async () => {
        const response = await apiClient.get('/workflow/list');
        return response.data;
    },

    // Update workflow
    updateWorkflow: async (id, workflow) => {
        const response = await apiClient.put(`/workflow/${id}`, workflow);
        return response.data;
    },

    // Delete workflow
    deleteWorkflow: async (id) => {
        await apiClient.delete(`/workflow/${id}`);
    },

    // Validate workflow
    validateWorkflow: async (id) => {
        const response = await apiClient.post(`/workflow/${id}/validate`);
        return response.data;
    },

    // Publish workflow
    publishWorkflow: async (id, publishData) => {
        const response = await apiClient.post(`/workflow/${id}/publish`, publishData);
        return response.data;
    },

    // Execute workflow
    executeWorkflow: async (executionRequest) => {
        const response = await apiClient.post('/workflow/execute', executionRequest);
        return response.data;
    },
};

export default apiClient;
