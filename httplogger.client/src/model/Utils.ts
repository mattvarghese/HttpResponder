
export function AdjustUrl(url: string): string
{
        const appPath = window.location.pathname.replace(/\/$/, "");
        return window.location.origin + appPath + (url.startsWith("/") ? url : "/" + url);
}

// Static URL for direct access to the hosted API endpoint
export const ApiUrl = () => { return AdjustUrl("api"); };
