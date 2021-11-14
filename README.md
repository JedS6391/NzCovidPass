# NZ COVID Pass

[![nuget][nuget-image]][nuget-url]
[![build][build-image]][build-url]
[![code-coverage][code-coverage-image]][code-coverage-url]

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

[nuget-image]: https://img.shields.io/nuget/v/NzCovidPass.Core?style=flat-square
[nuget-url]: https://www.nuget.org/packages/NzCovidPass.Core
[build-image]: https://img.shields.io/github/workflow/status/JedS6391/NzCovidPass/CI?style=flat-square
[build-url]: https://github.com/JedS6391/NzCovidPass/actions/workflows/ci.yml
[code-coverage-image]: https://img.shields.io/codecov/c/github/JedS6391/NzCovidPass?style=flat-square
[code-coverage-url]: https://app.codecov.io/gh/JedS6391/NzCovidPass