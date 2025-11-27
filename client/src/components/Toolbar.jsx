import React, { useRef } from 'react';
import './Toolbar.css';

const Toolbar = ({ onSave, onValidate, onExecute, onExport, onImport }) => {
    const fileInputRef = useRef(null);

    const handleImportClick = () => {
        fileInputRef.current?.click();
    };

    return (
        <div className="toolbar">
            <div className="toolbar-group">
                <button onClick={onSave} className="toolbar-btn primary" title="Save workflow">
                    💾 Save
                </button>
                <button onClick={onValidate} className="toolbar-btn" title="Validate workflow">
                    ✓ Validate
                </button>
                <button onClick={onExecute} className="toolbar-btn success" title="Execute workflow">
                    ▶ Execute
                </button>
            </div>

            <div className="toolbar-group">
                <button onClick={onExport} className="toolbar-btn" title="Export to JSON">
                    📤 Export
                </button>
                <button onClick={handleImportClick} className="toolbar-btn" title="Import from JSON">
                    📥 Import
                </button>
                <input
                    ref={fileInputRef}
                    type="file"
                    accept=".json"
                    onChange={onImport}
                    style={{ display: 'none' }}
                />
            </div>

            <div className="toolbar-info">
                <span>Workflow Builder v1.0</span>
            </div>
        </div>
    );
};

export default Toolbar;
