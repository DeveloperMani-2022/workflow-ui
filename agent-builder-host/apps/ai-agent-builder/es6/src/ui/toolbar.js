export class Toolbar {
    constructor(options) {
        this.container = options.container;
        this.onSave = options.onSave;
        this.onLoad = options.onLoad;
        this.onClear = options.onClear;
        this.init();
    }

    init() {
        this.container.innerHTML = `
            <div style="padding: 10px; background: #333; color: #fff; display: flex; gap: 10px;">
                <button id="save-btn" style="padding: 8px 16px; cursor: pointer;">Save</button>
                <button id="load-btn" style="padding: 8px 16px; cursor: pointer;">Load</button>
                <button id="clear-btn" style="padding: 8px 16px; cursor: pointer;">Clear</button>
            </div>
        `;

        this.container.querySelector('#save-btn').addEventListener('click', this.onSave);
        this.container.querySelector('#load-btn').addEventListener('click', this.onLoad);
        this.container.querySelector('#clear-btn').addEventListener('click', this.onClear);
    }
}
