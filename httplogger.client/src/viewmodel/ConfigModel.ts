export interface ConfigModel
{
    guid: string;
    name?: string; // optional string
    statusCode: number; // required number
    responseHeaders?: Record<string, string>; // optional dictionary
    body?: string; // optional string
    responseDelay: number; // required number
}
