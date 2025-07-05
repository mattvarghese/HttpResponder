# HttpResponder
A configurable HTTP logger and responder which I built as an exercise in learning react, and as a handy testing tool. The app is built using TypeScript + React and Tailwind CSS on the front-end and .NET Web API on the backend. 

When published (or run from source), users can go to the App UI to create responder configurations which are keyed using guids. They can then make HTTP GET / POST / PUT / PATCH / DELETE requests to the `api` endpoint (including specifying deeper path-segments and query parameters). The requests must specify the config to use using either an HTTP Header or a URL Query Parameter. The app will then use their config to respond, including response status code, response headers and body, and response delay. 

The app also logs requests and responses, so that users can look these up later from the App UI.

