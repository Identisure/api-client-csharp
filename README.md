<img align="left" height="100" style="margin-right: 10px;" src="../img/icon.png">

# IDkollen C# / .NET Client

[![NuGet version][nuget-image]][nuget-url]
[![MIT license][license-image]][license-url]

[nuget-image]: https://img.shields.io/nuget/v/Idkollen.Client?style=flat-square
[nuget-url]: https://www.nuget.org/packages/Idkollen.Client
[license-image]: https://img.shields.io/badge/license-MIT-blue?style=flat-square
[license-url]: https://opensource.org/license/mit

API client for the [IDkollen](https://developers.idkollen.se) REST API.

## Installation

```sh
dotnet add package Idkollen.Client
```

## Usage

```csharp
using Idkollen.Client;
using Idkollen.Client.Models;

var client = new IdkollenClientBuilder("client_id", "client_secret")
    .Environment(Environment.Staging)
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

## Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)

### Build

```sh
dotnet build Idkollen.Client.sln
```

### Test

```sh
dotnet test Idkollen.Client.sln
```
