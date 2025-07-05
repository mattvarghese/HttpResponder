import React from "react";
import LogViewer from "@/components/LogViewer";
import ConfigButton from "@/components/ConfigButton";

const App: React.FC = () =>
{
    const handleOpenLogViewer = (url: string) =>
    {
        window.open(url, "_blank");
    };

    return (
        <div className="min-h-screen bg-sky-100 px-4 py-10">
            <div className="relative mx-auto w-full max-w-2xl space-y-10">
                <LogViewer onViewLogs={handleOpenLogViewer} />
                <ConfigButton />
            </div>
        </div>
    );
};

export default App;
