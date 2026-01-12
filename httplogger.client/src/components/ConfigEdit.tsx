import React, { useMemo, useState } from "react";
import { FaTrash, FaPlus, FaSave, FaUndo, FaRegCopy, FaTimes } from "react-icons/fa";
import MonacoEditor from "@monaco-editor/react";
import type { ConfigModel, PathSpecificRule } from "@/viewmodel/ConfigModel";
import { AdjustUrl } from "@/model/Utils";

interface Props
{
    config: ConfigModel;
    onDone: () => void;
}

const ConfigEdit: React.FC<Props> = ({ config, onDone }) =>
{
    const [localConfig, setLocalConfig] = useState(config);
    const [errorMessage, setErrorMessage] = useState("");

    const [pathOverridesOpen, setPathOverridesOpen] = useState(false);

    const [pathOverridesJson, setPathOverridesJson] = useState<string>(() =>
        JSON.stringify(localConfig.pathSpecificResponse ?? [], null, 2)
    );

    const pathOverridesCountText = useMemo(() =>
    {
        try
        {
            const parsed = JSON.parse(pathOverridesJson) as unknown;
            if (Array.isArray(parsed))
            {
                return String(parsed.length);
            }
            return "invalid";
        }
        catch
        {
            return "invalid";
        }
    }, [pathOverridesJson]);

    const statusCodeOptions = [
        { code: 100, label: "100 Continue" },
        { code: 101, label: "101 Switching Protocols" },
        { code: 102, label: "102 Processing" },
        { code: 200, label: "200 OK" },
        { code: 201, label: "201 Created" },
        { code: 202, label: "202 Accepted" },
        { code: 204, label: "204 No Content" },
        { code: 301, label: "301 Moved Permanently" },
        { code: 302, label: "302 Found" },
        { code: 304, label: "304 Not Modified" },
        { code: 400, label: "400 Bad Request" },
        { code: 401, label: "401 Unauthorized" },
        { code: 403, label: "403 Forbidden" },
        { code: 404, label: "404 Not Found" },
        { code: 405, label: "405 Method Not Allowed" },
        { code: 409, label: "409 Conflict" },
        { code: 418, label: "418 I'm a teapot" },
        { code: 429, label: "429 Too Many Requests" },
        { code: 500, label: "500 Internal Server Error" },
        { code: 501, label: "501 Not Implemented" },
        { code: 502, label: "502 Bad Gateway" },
        { code: 503, label: "503 Service Unavailable" },
        { code: 504, label: "504 Gateway Timeout" }
    ];

    const handleHeaderChange = (index: number, field: "key" | "value", value: string) =>
    {
        const headers = Object.entries(localConfig.responseHeaders ?? {});
        const [k, v] = headers[index] || ["", ""];
        const newKey = field === "key" ? value : k;
        const newValue = field === "value" ? value : v;

        const updated: Record<string, string> = {};
        headers.forEach((entry, i) =>
        {
            if (i === index)
            {
                if (newKey.trim()) updated[newKey] = newValue;
            } else
            {
                updated[entry[0]] = entry[1];
            }
        });
        setLocalConfig({ ...localConfig, responseHeaders: updated });
    };

    const handleDeleteRow = (index: number) =>
    {
        const headers = Object.entries(localConfig.responseHeaders ?? {});
        headers.splice(index, 1);
        const updated = Object.fromEntries(headers);
        setLocalConfig({ ...localConfig, responseHeaders: updated });
    };

    const handleAddRow = () =>
    {
        setLocalConfig({
            ...localConfig,
            responseHeaders: { ...(localConfig.responseHeaders ?? {}), "": "" }
        });
    };

    const applyPathOverridesJsonToModel = (): boolean =>
    {
        try
        {
            const parsed = JSON.parse(pathOverridesJson) as unknown;

            if (!Array.isArray(parsed))
            {
                setErrorMessage("Path Specific Response Overrides must be a JSON array.");
                return false;
            }

            setLocalConfig({ ...localConfig, pathSpecificResponse: parsed as PathSpecificRule[] });
            return true;
        }
        catch (ex)
        {
            setErrorMessage("Path Specific Response Overrides JSON is invalid: " + ex);
            return false;
        }
    };

    const handleSaveConfig = async () =>
    {
        try
        {
            setErrorMessage("");

            if (!applyPathOverridesJsonToModel())
            {
                return;
            }

            const updatedConfig: ConfigModel = {
                ...localConfig,
                pathSpecificResponse: (JSON.parse(pathOverridesJson) as PathSpecificRule[])
            };

            const url = AdjustUrl(`/ui/set-config?config-guid=${updatedConfig.guid}`);
            const res = await fetch(url, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedConfig)
            });
            if (res.ok)
            {
                setErrorMessage("");
                onDone();
            }
            else
            {
                throw new Error(`HTTP ${res.status}`);
            }
        }
        catch (ex)
        {
            setErrorMessage("Failed saving config: " + ex);
        }
    };

    const handleDeleteConfig = async () =>
    {
        const confirmed = confirm("Are you sure you want to delete this config?");
        if (!confirmed) return;

        try
        {
            const url = AdjustUrl(`/ui/delete-config?config-guid=${localConfig.guid}`);
            const res = await fetch(url, { method: "DELETE" });
            if (res.ok)
            {
                onDone();
            } else
            {
                throw new Error(`HTTP ${res.status}`);
            }
        } catch (ex)
        {
            setErrorMessage("Failed deleting config: " + ex);
        }
    };

    return (
        <div className="space-y-6">

            {errorMessage && <div className="mb-4 text-red-500">{errorMessage}</div>}

            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                {/* Friendly Name */}
                <div>
                    <label className="block font-medium">Friendly Name (just for you to remember by)</label>
                    <input
                        type="text"
                        value={localConfig.name}
                        onChange={(e) => setLocalConfig({ ...localConfig, name: e.target.value })}
                        className="w-full border border-gray-300 rounded px-4 py-2"
                    />
                </div>

                {/* GUID */}
                <div>
                    <label className="mb-1 block font-medium">GUID</label>
                    <div className="flex items-center gap-2">
                        <input
                            type="text"
                            readOnly
                            value={localConfig.guid}
                            className="w-full rounded border border-gray-300 bg-gray-100 px-4 py-2"
                        />
                        <FaRegCopy
                            title="Copy GUID"
                            className="shrink-0 cursor-pointer text-gray-500 hover:text-gray-800"
                            onClick={() => navigator.clipboard.writeText(localConfig.guid)}
                        />
                    </div>
                </div>

                {/* Status Code */}
                <div>
                    <label className="block font-medium">HTTP Status Code</label>
                    <select
                        value={localConfig.statusCode}
                        onChange={(e) =>
                            setLocalConfig({ ...localConfig, statusCode: parseInt(e.target.value, 10) })
                        }
                        className="w-full border border-gray-300 rounded px-4 py-2"
                    >
                        {statusCodeOptions.map((opt) => (
                            <option key={opt.code} value={opt.code}>
                                {opt.label}
                            </option>
                        ))}
                    </select>
                </div>

                {/* Response Delay */}
                <div>
                    <label className="block font-medium">Response Delay (ms)</label>
                    <input
                        type="number"
                        min={0}
                        value={localConfig.responseDelay}
                        onChange={(e) =>
                            setLocalConfig({
                                ...localConfig,
                                responseDelay: parseInt(e.target.value) || 0
                            })
                        }
                        className="w-full border border-gray-300 rounded px-4 py-2"
                    />
                </div>
            </div>

            <div>
                <label className="mb-2 block font-medium">Response Headers</label>
                <div className="space-y-2">
                    {Object.entries(localConfig.responseHeaders ?? {}).map(([key, value], idx) => (
                        <div key={idx} className="flex items-center gap-2">
                            <input
                                value={key}
                                onChange={(e) => handleHeaderChange(idx, "key", e.target.value)}
                                className="flex-1 border border-gray-300 rounded px-2 py-1"
                                placeholder="Header name"
                            />
                            <input
                                value={value}
                                onChange={(e) => handleHeaderChange(idx, "value", e.target.value)}
                                className="flex-1 border border-gray-300 rounded px-2 py-1"
                                placeholder="Header value"
                            />
                            <button onClick={() => handleDeleteRow(idx)} className="text-red-500">
                                <FaTimes />
                            </button>
                        </div>
                    ))}
                    <button onClick={handleAddRow} className="mt-2 flex items-center gap-2 text-sm text-sky-700">
                        <FaPlus /> Add Header
                    </button>
                </div>
            </div>

            <div>
                <label className="mb-1 block font-medium">Response Body</label>
                <div className="h-60 overflow-hidden rounded border border-gray-300">
                    <MonacoEditor
                        language="json"
                        value={localConfig.body}
                        onChange={(value) => setLocalConfig({ ...localConfig, body: value || "" })}
                        height="100%"
                        options={{ minimap: { enabled: false } }}
                    />
                </div>
            </div>

            {/* Path Specific Response Overrides (collapsible) */}
            <div className="rounded border border-gray-300">
                <button
                    type="button"
                    className="flex w-full items-center justify-between px-4 py-2 text-left font-medium"
                    onClick={() => setPathOverridesOpen(!pathOverridesOpen)}
                >
                    <span>
                        {pathOverridesOpen
                            ? "Path Specific Response Overrides"
                            : `Path Specific Response Overrides (${pathOverridesCountText})`}
                    </span>
                    <span className="text-gray-500">{pathOverridesOpen ? "Hide" : "Show"}</span>
                </button>

                {pathOverridesOpen && (
                    <div className="space-y-3 px-4 pb-4">
                        <div className="text-sm text-gray-700">
                            Path specific overrides are advanced functionality. They allow overrides by literal paths or
                            regular expressions. See source-code at{" "}
                            <a
                                href="https://github.com/mattvarghese/HttpResponder/blob/main/HttpLogger.Server/Controllers/HttpController.cs"
                                target="_blank"
                                rel="noreferrer"
                                className="text-sky-700 underline"
                            >
                                https://github.com/mattvarghese/HttpResponder/blob/main/HttpLogger.Server/Controllers/HttpController.cs
                            </a>{" "}
                            for how to use.
                        </div>

                        <div className="h-60 overflow-hidden rounded border border-gray-300">
                            <MonacoEditor
                                language="json"
                                value={pathOverridesJson}
                                onChange={(value) => setPathOverridesJson(value || "[]")}
                                height="100%"
                                options={{ minimap: { enabled: false } }}
                            />
                        </div>
                    </div>
                )}
            </div>

            <div className="flex flex-wrap items-center justify-between gap-4">
                <div className="flex gap-4">
                    <button
                        onClick={handleSaveConfig}
                        className="flex items-center gap-2 rounded bg-green-600 px-4 py-2 text-white hover:bg-green-700"
                    >
                        <FaSave /> Save
                    </button>

                    <button
                        onClick={() =>
                        {
                            setLocalConfig({ ...config });
                            setPathOverridesJson(JSON.stringify(config.pathSpecificResponse ?? [], null, 2));
                            setErrorMessage("");
                        }}
                        className="flex items-center gap-2 bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
                    >
                        <FaUndo /> Reset
                    </button>
                </div>

                <div className="flex gap-4">
                    <button
                        onClick={handleDeleteConfig}
                        className="flex items-center gap-2 rounded bg-red-600 px-4 py-2 text-white hover:bg-red-700"
                    >
                        <FaTrash /> Delete
                    </button>

                    <button
                        onClick={onDone}
                        className="flex items-center gap-2 rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
                    >
                        <FaTimes /> Close
                    </button>
                </div>
            </div>

        </div>
    );
};

export default ConfigEdit;
