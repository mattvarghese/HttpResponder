import React, { useState } from "react";
import OverlayFrame from "./OverlayFrame";
import Configuration from "./Configuration";

const ConfigButton: React.FC = () =>
{
    const [isOpen, setIsOpen] = useState(false);

    const handleConfigure = () =>
    {
        setIsOpen(true);
    };

    const handleClose = () =>
    {
        setIsOpen(false);
    };

    return (
        <>
            <div className="rounded-2xl bg-white p-8 shadow-lg">
                <h2 className="mb-2 text-lg font-semibold text-gray-800">Configuration</h2>
                <p className="mb-4 text-sm text-gray-600">
                    Click here to configure the response from the API endpoint.
                </p>
                <button
                    onClick={handleConfigure}
                    className="w-full rounded-xl bg-gray-800 py-3 font-semibold text-white transition duration-200 hover:bg-gray-900"
                >
                    Configure
                </button>
            </div>

            {isOpen && (
                <OverlayFrame title="Edit Configuration" onClose={handleClose}>
                    <Configuration />
                </OverlayFrame>
            )}
        </>
    );
};

export default ConfigButton;
