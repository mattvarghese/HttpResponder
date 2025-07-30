# HttpResponder
A configurable HTTP logger and responder which I built as an exercise in learning react, and as a handy testing tool. The app is built using TypeScript + React and Tailwind CSS on the front-end and .NET Web API on the backend. 

When published (or run from source), users can go to the App UI to create responder configurations which are keyed using guids. They can then make HTTP `GET` / `POST` / `PUT` / `PATCH` / `DELETE` requests to the `api` endpoint (including specifying deeper path-segments and query parameters). The requests must specify the config to use using either an HTTP Header or a URL Query Parameter. The app will then use their config to respond, including response status code, response headers and body, and response delay. 

The app also logs requests and responses, so that users can look these up later from the App UI.


# Running the code

## Using VS2022
This relies on the HttpLogger.sln file and standard VS2022 capabilities.

After checking out the code, right click on the solution in Solution Explorer > Configure Startup Projects > Select "Multiple startup project".
Then in the box on the right, set Action for both projects to "Start". Move the Server one up before the Client if necessary.

## Using VSCode
This relies on the root folder's .vscode/tasks.json, .vscode/launch.json.

Make sure Microsoft's "C# Dev Kit" extension for VSCode is installed.

Right click in the root folder and choose to open with VS Code. Then on the left sidebar go to the run and debug tab.
In the configurations dropdown at the top of the left sidebar, choose "Full Stack (Client + Server)" and then hit the green go button.
This opens two browser windows. One for the backend which opens ui/stats, and the other for the frontend which opens to the main app.
C# Breakpoints will hit whenever the code gets called, but TypeScript breakpoints will only hit within that Chrome window for client.

## From terminal / bash
This relies on the package.json in the root folder.

You can `cd` to the root folder in a terminal and run `npm run dev` to run the application.
This does not automatically open browser windows, and you get no debugging capabilities beyond any browser debug tools.
