<img align="left" height="100" style="margin-right: 10px;" src="../img/icon.png">

# Identisure C# / .NET Client

[![NuGet version][nuget-image]][nuget-url]
[![MIT license][license-image]][license-url]

[nuget-image]: https://img.shields.io/nuget/v/Identisure.Client?style=flat-square
[nuget-url]: https://www.nuget.org/packages/Identisure.Client
[license-image]: https://img.shields.io/badge/license-MIT-blue?style=flat-square
[license-url]: https://opensource.org/license/mit

API client for the [Identisure](https://developers.identisure.com) REST API.

## Installation

```sh
dotnet add package Identisure.Client
```

## Usage

```csharp
using Identisure.Client;
using Identisure.Client.Models;

var client = new IdentisureClientBuilder("client_id", "client_secret")
    .Environment(IdentisureEnvironment.Staging)
    .Build();

var session = await client.BankIdSe.AuthAsync(new BankIdSeAuthRequest());
if (session is BankIdSePending pending)
{
    Console.WriteLine($"Session ID: {pending.Id}");

    var result = await client.BankIdSe.WaitForAuthAsync(pending.Id);
    switch (result)
    {
        case BankIdSeCompleted done:
            Console.WriteLine($"Authenticated: {done.Name} ({done.Ssn})");
            break;
        case BankIdSeFailed failed:
            Console.WriteLine($"Failed: {failed.Error}");
            break;
    }
}
```

### Verifying callbacks

Verify the `X-Signature-SHA256` header against the raw request body before parsing it:

```csharp
var valid = CallbackSignature.Verify(rawBody, signatureHeader, "hmac_secret");
```

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)

### Build

```sh
dotnet build Identisure.Client.slnx
```

### Test

```sh
dotnet test Identisure.Client.slnx
```
