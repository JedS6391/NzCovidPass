# NZ COVID Pass

[![nuget][nuget-image]][nuget-url]
[![build][build-image]][build-url]
[![code-coverage][code-coverage-image]][code-coverage-url]

## About

Provides the ability to ability to verify [New Zealand COVID Pass](https://nzcp.covid19.health.nz/) payloads in .NET.

## Usage

```cs
var services = new ServiceCollection();

services.AddMemoryCache();
services.AddNzCovidPassVerifier();

var provider = services.BuildServiceProvider();

var verifier = provider.GetRequiredService<PassVerifier>();

var result = await verifier.VerifyAsync("...");

if (result.HasSucceeded)
{
    // Pass successfully verified
    Console.WriteLine($"NZ COVID Pass subject details: {result.Pass.FamilyName}, {result.Pass.GivenName} - {result.Pass.DateOfBirth}");
}
else
{    
    // Invalid pass
    Console.WriteLine($"Verification failed: {string.Join(", ", result.FailureReasons.Select(fr => fr.Code))}");
}
```

Full examples of usage can be found in the [demos](./demos/) folder.

### Advanced usage

#### Logging

The `PassVerifier` logs message via the `Microsoft.extensions.Logging.ILogger<TCategoryName>` abstraction. See the [.NET documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line) for more details.

#### Customising verifier options

```cs
services.AddNzCovidPassVerifier(options =>
{
    var validIssuers = PassVerifierOptions.Defaults.ValidIssuers.ToHashSet();

    // Add test issuer
    validIssuers.Add("did:web:nzcp.covid19.health.nz");

    options.ValidIssuers = validIssuers;    
});
```

#### Custom verification keys

With the default configuration, verification keys are resolved via HTTP from the DID document associated with the  issuer and key ID contained in the pass payload.

If you wish to use a static key (e.g. to support offline usage) or for testing purposes, a custom `IVerificationKeyProvider` can be registered:

```cs
services.Replace(ServiceDescriptor.Singleton<IVerificationKeyProvider, CustomVerificationKeyProvider>());
```

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