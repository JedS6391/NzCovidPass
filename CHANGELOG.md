# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2021-11-20

### Changed

- Use `System.Formats.Cbor` for reading/writing CBOR encoded data
- The `Tokens` namespace is now `Cwt`. All types in `Tokens` namespace have been updated

## [0.4.1] - 2021-11-19

- Enable [Source Link](https://github.com/dotnet/sourcelink)

## [0.4.0] - 2021-11-18

### Added

- Validate credential details as part of verification per the specification

### Changed

- Rename `CborWebToken` to `CwtSecurityToken` (as well as components that relate to it)
- Simplify the default configuration of `PassVerifierOptions`. Default values are set when the object is initialised rather than needing explicit configuration
- All models now enforce their required fields at construction

### Fixed

- Improve validation around reading CWT claim values to prevent exceptions
- Validate COSE structure before during token read process. This prevents us trying to construct a `CwtSecurityToken` with an invalid structure

## [0.3.0] - 2021-11-15

### Added

- `VerifiableCredential<TCredential>` added to represent the generic verifiable credential data model

### Changed

- `ICborWebTokenReader.ReadToken` now takes a `CborWebTokenReaderContext` object to allow the caller to inspect token read failures
- `PublicCovidPass` now only contains the properties associated with the type, rather than the broader verifiable credential properties
- `PublicCovidPass.DateOfBirth` is now of type `DateTimeOffset` rather than `string`

### Fixed

- Improve handling of invalid base-32 payloads

## [0.2.0] - 2021-11-14

### Added

- Caching of security keys obtained during validation. Note this will require an `IMemoryCache` implementation to be provided to `DecentralizedIdentifierDocumentVerificationKeyProvider`

## [0.1.1] - 2021-11-14

### Added

- Missing license and README in Nuget package

## [0.1.0] - 2021-11-14

### Added

- Initial library release providing the ability to verify New Zealand COVID Pass payloads

[0.5.0]: https://github.com/JedS6391/NzCovidPass/compare/0.4.1...0.5.0
[0.4.1]: https://github.com/JedS6391/NzCovidPass/compare/0.4.0...0.4.1
[0.4.0]: https://github.com/JedS6391/NzCovidPass/compare/0.3.0...0.4.0
[0.3.0]: https://github.com/JedS6391/NzCovidPass/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/JedS6391/NzCovidPass/compare/0.1.1...0.2.0
[0.1.1]: https://github.com/JedS6391/NzCovidPass/compare/0.1.0...0.1.1
[0.1.0]: https://github.com/JedS6391/NzCovidPass/releases/tag/0.1.0