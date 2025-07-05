import React, { useCallback, useEffect, useState } from "react";
import type { ConfigModel } from "@/viewmodel/ConfigModel";
import { AdjustUrl } from "@/model/Utils";
import { FaSyncAlt } from "react-icons/fa";

interface ConfigSummary
{
    guid: string;
    name: string;
    lastUpdate: string;
}

interface Props
{
    onSelect: (config: ConfigModel | null) => void;
    allowCreate?: boolean;
    selectNull?: boolean;
    prompt: string;
}

const ConfigSelect: React.FC<Props> = ({
    onSelect,
    allowCreate = false,
    selectNull = false,
    prompt
}) =>
{
    const [configs, setConfigs] = useState<ConfigSummary[]>([]);
    const [errorMessage, setErrorMessage] = useState("");
    const [selectedGuid, setSelectedGuid] = useState("unselected");

    const handleSelectionChange = async (guid: string) =>
    {
        setSelectedGuid(guid);

        if (guid === "unselected")
        {
            if (selectNull)
            {
                onSelect(null);
            }
            return;
        }

        if (guid === "create-new")
        {
            onSelect({
                name: "",
                statusCode: 200,
                responseHeaders: {},
                body: "",
                guid: crypto.randomUUID(),
                responseDelay: 500
            });
        } else
        {
            try
            {
                const url = AdjustUrl(`/ui/get-config?config-guid=${guid}`);
                const res = await fetch(url);
                if (res.ok)
                {
                    setErrorMessage("");
                    const config = await res.json();
                    onSelect(config);
                } else
                {
                    throw new Error(`HTTP ${res.status}`);
                }
            } catch (err)
            {
                setErrorMessage("Failed fetching the config: " + err);
            }
        }
    };

    const loadConfigs = useCallback(async () =>
    {
        try
        {
            const url = AdjustUrl("/ui/configs");
            const res = await fetch(url);
            if (res.ok)
            {
                setErrorMessage("");
                const data = await res.json();
                setConfigs(data);

                // If current selection is missing in refreshed data, reset
                const selectedExists = data.some((c: ConfigSummary) => c.guid === selectedGuid);
                if (!selectedExists && selectedGuid !== "unselected")
                {
                    setSelectedGuid("unselected");
                    if (selectNull)
                    {
                        onSelect(null);
                    }
                }
            } else
            {
                throw new Error(`HTTP ${res.status}`);
            }
        } catch (err)
        {
            setErrorMessage("Failed fetching configs list: " + err);
            setConfigs([]);
            setSelectedGuid("unselected");
            if (selectNull)
            {
                onSelect(null);
            }
        }
    },[onSelect, selectNull, selectedGuid]);

    useEffect(() =>
    {
        loadConfigs();
    }, [loadConfigs]);

    return (
        <div className="space-y-4">
            {errorMessage && <div className="mb-4 text-red-500">{errorMessage}</div>}

            <label className="block font-medium">{prompt}</label>
            <div className="flex w-full items-center gap-2 overflow-hidden">
                <select
                    value={selectedGuid}
                    onChange={(e) => handleSelectionChange(e.target.value)}
                    className="w-full border border-gray-300 rounded-lg px-4 py-2 truncate"
                >
                    <option value="unselected">-- Make a selection --</option>
                    {allowCreate && <option value="create-new">+ Create new configuration</option>}
                    {configs.map((c: ConfigSummary) => (
                        <option key={c.guid} value={c.guid}>
                            {(c.name || "(unnamed)") + " | " + c.guid + " | " + c.lastUpdate}
                        </option>
                    ))}
                </select>
                <button
                    onClick={loadConfigs}
                    title="Refresh list"
                    className="shrink-0 text-gray-600 transition hover:text-sky-700"
                >
                    <FaSyncAlt />
                </button>
            </div>


        </div>
    );
};

export default ConfigSelect;
