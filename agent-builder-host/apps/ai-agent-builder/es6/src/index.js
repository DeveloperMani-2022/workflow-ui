import { App } from './app.js';
import './config/joint-config.js';

// Initialize the application
document.addEventListener('DOMContentLoaded', () => {
    console.log('AI Agent Builder - Initializing...');

    const app = new App({
        container: document.getElementById('app'),
        apiBaseUrl: window.APP_CONFIG?.apiBaseUrl || 'http://localhost:5031/api/workflow'
    });

    app.init();

    console.log('AI Agent Builder - Ready');
});
