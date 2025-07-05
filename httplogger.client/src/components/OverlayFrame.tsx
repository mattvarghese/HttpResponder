import React, { useEffect, type ReactNode } from "react";
import { FaTimes } from "react-icons/fa";

interface ExitButtonProps
{
    onClick: () => void;
    ariaLabel?: string;
    title: string;
}

const ExitButton: React.FC<ExitButtonProps> = ({ onClick, ariaLabel = "Close", title }) =>
{
    return (
        <button
            onClick={onClick}
            className="absolute top-2 right-2 p-1 text-gray-600 hover:text-gray-900"
            aria-label={ariaLabel} title={title }
        >
            <FaTimes size={20} />
        </button>
    );
};

interface OverlayFrameProps
{
    children: ReactNode;
    onClose: () => void;
    title: string;
    nestingLevel?: number;
    titleBarColor?: string;
}

const OverlayFrame: React.FC<OverlayFrameProps> = ({
    children,
    onClose,
    title,
    nestingLevel = 1,
    titleBarColor
}) =>
{
    const h = 95 - nestingLevel * 5;
    const w = 95 - nestingLevel * 5;
    const height = `${h}vh`;
    const width = `${w}vw`;

    // Keyboard accelerator: ALT+X
    useEffect(() =>
    {
        const handleKeyDown = (e: KeyboardEvent) =>
        {
            if (e.altKey && (e.key === "x" || e.key === "X"))
            {
                e.preventDefault();
                onClose();
            }
        };

        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [onClose]);

    return (
        <div className="fixed inset-0 flex items-center justify-center bg-black/50">
            <div
                className="relative flex flex-col rounded-lg bg-white shadow-lg"
                style={{ height, width }}
            >
                {/* Title Bar */}
                <div
                    className={`flex items-center justify-between rounded-t-lg px-4 py-2 ${titleBarColor ?? "bg-sky-200 text-sky-800"}`}
                >
                    <span className="text-lg font-semibold">{title}</span>
                    <ExitButton onClick={onClose} title="Close (Alt+X)"  />
                </div>

                {/* Content Area */}
                <div className="flex-1 overflow-auto p-6">{children}</div>
            </div>
        </div>
    );
};

export default OverlayFrame;
