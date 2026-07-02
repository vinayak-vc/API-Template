# ViitorCloud REST API Mechanism

A lightweight Unity REST API wrapper for typed JSON requests, multipart file uploads, environment switching, and per-request logging control.

## Features

- Typed JSON request and response helpers
- GET, POST, PUT, DELETE support
- Single-file and multi-file multipart uploads
- Optional bearer token support
- Environment selection for Development, Live, or config-driven base URL
- Per-request logging modes: `Default`, `Quiet`, and `Verbose`

## Requirements

- Unity `2022.3` or later
- `UnityWebRequest`
- A scene object with `ServerCommunication` available, or a bootstrap that creates it

## Installation

Install this package as a Unity package or copy the folder into your project and let Unity import it.

## Quick Start

1. Add a `ServerCommunication` component to a GameObject in your startup scene.
2. Set `ServerCommunication.server` to the environment you want.
3. Use `ServerCommunicationTemplate.Instance` or `ServerCommunication.Instance` to send requests.

## Login Example

```csharp
using ViitorCloud.API;
using ViitorCloud.API.StandardTemplates;

public class LoginExample : UnityEngine.MonoBehaviour
{
    public void Login()
    {
        var request = new Login
        {
            emailId = "user@example.com",
            password = "secret"
        };

        ServerCommunicationTemplate.Instance.RequestLogin(
            request,
            response =>
            {
                UnityEngine.Debug.Log("Login success: " + response.data?.user?.emailId);
            },
            error =>
            {
                UnityEngine.Debug.LogError("Login failed: " + error);
            },
            ServerCommunication.RequestLogMode.Quiet);
    }
}
```

## Generic JSON Example

```csharp
ServerCommunicationTemplate.Instance.RequestJson<MyRequest, MyResponse>(
    ServerCommunication.HttpMethod.Post,
    "https://api.example.com/profile/update",
    new MyRequest { name = "Alex" },
    onSuccess,
    onFail,
    ServerCommunication.RequestLogMode.Verbose);
```

## Raw Request Example

```csharp
ServerCommunicationTemplate.Instance.RequestRaw(
    ServerCommunication.HttpMethod.Post,
    "https://api.example.com/raw-endpoint",
    "{\"hello\":\"world\"}",
    responseText => UnityEngine.Debug.Log(responseText),
    error => UnityEngine.Debug.LogError(error));
```

## GET and DELETE Examples

```csharp
ServerCommunicationTemplate.Instance.RequestGet<MyResponse>(
    "https://api.example.com/items/1",
    onSuccess,
    onFail);

ServerCommunicationTemplate.Instance.RequestDelete(
    "https://api.example.com/items/1",
    () => UnityEngine.Debug.Log("Deleted"),
    error => UnityEngine.Debug.LogError(error));
```

## File Upload Examples

```csharp
ServerCommunicationTemplate.Instance.RequestUpload<MyResponse>(
    "file",
    @"C:\temp\avatar.png",
    "https://api.example.com/upload",
    onSuccess,
    onFail);
```

```csharp
var files = new System.Collections.Generic.List<ServerCommunication.FileUpload>
{
    new ServerCommunication.FileUpload { fieldName = "front", filePath = @"C:\temp\front.png" },
    new ServerCommunication.FileUpload { fieldName = "back", filePath = @"C:\temp\back.png" }
};

ServerCommunicationTemplate.Instance.RequestUpload<MyResponse>(
    "https://api.example.com/upload/multi",
    files,
    onSuccess,
    onFail,
    ServerCommunication.RequestLogMode.Verbose);
```

## Logging Modes

- `Default`: follows the global `ServerCommunication.debug` flag
- `Quiet`: suppresses request lifecycle and body logs for that call
- `Verbose`: forces detailed logs for that call

## Environment Setup

`ServerCommunication` exposes a `server` field with these modes:

- `Development`
- `Live`
- `FromConfig`

If you use `FromConfig`, the package expects the config flow defined in `API/Constants.cs`.

## Notes

- `ServerCommunicationTemplate` is a convenience wrapper and example surface.
- You can call `ServerCommunication.Instance` directly if you prefer a lower-level API.
- Error callbacks receive the server message when available, otherwise a fallback message.

## Package Metadata

- Package name: `com.viitorcloud.apimechanism`
- Display name: `ViitorCloud REST API Mechanism`
- Supported Unity version: `2022.3`

