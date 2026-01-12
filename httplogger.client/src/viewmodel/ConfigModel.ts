export interface PathResponseModel
{
    statusCode: number;
    responseHeaders?: Record<string, string>;
    body?: string;
}

export interface PathSpecificRule
{
    pattern: string;
    isRegularExpression?: boolean; // defaults to false on server
    ignoreCase?: boolean;          // defaults to true on server
    response: PathResponseModel;
}

export interface ConfigModel extends PathResponseModel
{
    guid: string;
    name?: string;
    responseDelay: number;
    pathSpecificResponse?: PathSpecificRule[];
}
