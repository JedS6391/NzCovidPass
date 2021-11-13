# NZ COVID Pass

[![build][build-image]][build-url]

## About

Provides the ability to ability to verify New Zealand COVID Pass payloads in .NET.

## Usage

```cs
var services = new ServiceCollection();

services.AddNzCovidPassVerifier();

var provider = services.BuildServiceProvider();

var verifier = provider.GetRequiredService<PassVerifier>();

var result = await verifier.VerifyAsync("...");

if (result.HasSucceeded)
{
    // Pass is valid.
    var details = result.Credentials.Details;

    Console.WriteLine($"{details.FamilyName}, {details.GivenName} - {details.DateOfBirth}");
}
else
{
    // Pass is not valid.
}
```

Further examples of usage can be found in the [demos](./demos/) folder.

## Development

Following the instructions below to get started with the project in a local development environment.

### Prerequisites

- [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)

### Building

After cloning the source code to a destination of your choice, run the following command to build the solution:

```console
dotnet build
```

### Tests

The test suite can be run using the following command:

```console
dotnet test
```

[build-image]: https://img.shields.io/github/workflow/status/JedS6391/NzCovidPass/CI?style=flat-square
[build-url]: https://github.com/JedS6391/NzCovidPass/actions/workflows/ci.yml