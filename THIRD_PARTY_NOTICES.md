# THIRD_PARTY_NOTICES

## Dependency audit

- `TelegramLauncher.csproj` does not include external NuGet dependencies (`PackageReference` is absent).
- The app uses .NET 8 platform libraries (`System.*`) distributed under the .NET Foundation licensing model.

## External data sources

The app downloads public proxy lists from third-party repositories and APIs at runtime. Those lists are external data, not bundled code. Source URLs are declared in:

- `TelegramLauncher/Services/ProxySourceService.cs`

Before commercial or redistributed use of the fetched data, review terms/policies of each upstream source.

## Design references

UI ideas were adapted from publicly shared design references. No third-party proprietary code is embedded into the project.
