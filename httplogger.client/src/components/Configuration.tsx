import React, { useEffect, useState } from "react";
import ConfigSelect from "./ConfigSelect";
import ConfigEdit from "./ConfigEdit";
import type { ConfigModel } from "@/viewmodel/ConfigModel";
import { AdjustUrl, ApiUrl } from "@/model/Utils";
import { FaRegCopy } from "react-icons/fa";

const Configuration: React.FC = () =>
{
    const [selectedConfig, setSelectedConfig] = useState<ConfigModel | null>(null);
    const [errorMessage, setErrorMessage] = useState("");
    const [configKey, setConfigKey] = useState<string>("");

    const onDone = () =>
    {
        setSelectedConfig(null);
    };

    useEffect(() =>
    {
        const fetchKey = async () =>
        {
            try
            {
                const url = AdjustUrl("/ui/get-key");
                const res = await fetch(url);
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const key = await res.text();
                setConfigKey(key);
            } catch (err)
            {
                setErrorMessage("Failed to fetch config key from server: " + err);
            }
        };

        fetchKey();
    }, []);

    return (
        <div className="rounded-2xl bg-white p-8 shadow-lg">

            {errorMessage && <div className="mb-4 text-red-500">{errorMessage}</div>}

            <h2 className="mb-4 text-lg font-semibold text-gray-800">Configuration</h2>

            <p className="mb-6 text-sm text-gray-600">
                Create a configuration and note its GUID. Then in requests to{" "}
                <span className="inline-flex items-center gap-1">
                    <code className="text-base font-medium text-black">{ApiUrl()}</code>
                    <FaRegCopy
                        title="Copy API URL"
                        className="cursor-pointer text-gray-500 hover:text-gray-800"
                        onClick={() => navigator.clipboard.writeText(ApiUrl())}
                    />
                </span>
                , either include an HTTP header or a URL query parameter named{" "}
                <span className="inline-flex items-center gap-1">
                    <code className="text-base font-medium text-black">{configKey || "[config-key]"}</code>
                    {configKey && (
                        <FaRegCopy
                            title="Copy header name"
                            className="cursor-pointer text-gray-500 hover:text-gray-800"
                            onClick={() => navigator.clipboard.writeText(configKey)}
                        />
                    )}
                </span>{" "}
                with the GUID as the value. This will allow the service to use your
                configuration as the response to your request. Your requests need not be
                restricted to exactly the api URL - additional path-segments and query parameters
                are also handled by the app.
            </p>


            {selectedConfig === null ? (
                <ConfigSelect onSelect={setSelectedConfig} allowCreate={true} prompt="Select a config to edit" />
            ) : (
                <ConfigEdit config={selectedConfig} onDone={onDone} />
            )}
        </div>
    );
};

export default Configuration;
