import React, { useState } from "react";
import { AdjustUrl, ApiUrl } from "@/model/Utils";
import { FaChartBar, FaRegCopy } from "react-icons/fa";
import type { ConfigModel } from "@/viewmodel/ConfigModel";
import ConfigSelect from "./ConfigSelect";

interface LogViewerProps
{
    onViewLogs: (url: string) => void;
}

const LogViewer: React.FC<LogViewerProps> = ({ onViewLogs }) =>
{
    const getToday = () =>
    {
        const today = new Date();
        const year = today.getFullYear();
        const month = String(today.getMonth() + 1).padStart(2, "0");
        const day = String(today.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
    };

    const [date, setDate] = useState(getToday());
    const [count, setCount] = useState(10);
    const [selectedConfig, setSelectedConfig] = useState<ConfigModel | null>(null);

    const statsUrl = AdjustUrl("/ui/stats");

    const handleClick = () =>
    {
        const base = `/ui?date=${encodeURIComponent(date)}&count=${count}`;
        const query = selectedConfig ? `${base}&guid=${encodeURIComponent(selectedConfig.guid)}` : base;
        const fullUrl = AdjustUrl(query);
        onViewLogs(fullUrl);
    };

    return (
        <>
            {/* Top right Stats link */}
            <div className="absolute top-0 right-0 mt-2 mr-2">
                <a
                    href={statsUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1 text-sm font-medium text-sky-700 underline hover:text-sky-900"
                >
                    <FaChartBar />
                    <span>Stats</span>
                </a>
            </div>

            <div className="rounded-2xl bg-white p-8 shadow-lg">
                <h1 className="mb-6 text-center text-2xl font-semibold">
                    Vmatt's HTTP Logger
                </h1>

                <p className="mb-6 text-center text-sm text-gray-600">
                    This web service logs <code className="text-black">GET</code>,{" "}
                    <code className="text-black">POST</code>,{" "}
                    <code className="text-black">PUT</code>,{" "}
                    <code className="text-black">PATCH</code>, and{" "}
                    <code className="text-black">DELETE</code> http requests made to{" "}
                    <span className="inline-flex items-center justify-center gap-1">
                        <a
                            href={ApiUrl()}
                            className="text-sky-700 underline hover:text-sky-900"
                            target="_blank"
                            rel="noopener noreferrer"
                        >
                            {ApiUrl()}
                        </a>
                        <FaRegCopy
                            title="Copy API URL"
                            className="cursor-pointer text-gray-500 hover:text-gray-800"
                            onClick={() => navigator.clipboard.writeText(ApiUrl())}
                        />
                    </span>
                    <br />
                    and allows you to examine the logs using this UI.<br />
                    You may also configure how the service responds in the Configuration section.
                </p>

                <div className="mb-4">
                    <label className="mb-1 block font-medium">Date</label>
                    <input
                        type="date"
                        value={date}
                        onChange={(e) => setDate(e.target.value)}
                        className="w-full border border-gray-300 rounded-lg px-4 py-2"
                    />
                </div>

                <div className="mb-4">
                    <label className="mb-1 block font-medium">Count</label>
                    <input
                        type="number"
                        min={1}
                        value={count}
                        onChange={(e) => setCount(parseInt(e.target.value, 10))}
                        className="w-full border border-gray-300 rounded-lg px-4 py-2"
                    />
                </div>

                <div className="mb-6">
                    <ConfigSelect onSelect={setSelectedConfig} selectNull={true} prompt="Limit to config" />
                </div>

                <button
                    onClick={handleClick}
                    className="w-full rounded-xl bg-sky-600 py-3 font-semibold text-white transition duration-200 hover:bg-sky-700"
                >
                    View Logs
                </button>
            </div>
        </>
    );
};

export default LogViewer;
