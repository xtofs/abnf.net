# NuGet Package Publishing Guide

## Overview

This repository is configured to automatically publish the `Abnf.Net` NuGet package to nuget.org using GitHub Actions.

## Package Configuration

The package is configured in [src/abnf.net/abnf.net.csproj](../src/abnf.net/abnf.net.csproj):

- **Package ID**: `Abnf.Net`
- **Current Version**: `0.1.0-preview.1` (pre-release)
- **License**: MIT
- **Target Framework**: .NET 10.0

## Versioning

We use semantic versioning (SemVer):

- **Major.Minor.Patch** for stable releases (e.g., `1.0.0`)
- **Major.Minor.Patch-preview.N** for pre-release versions (e.g., `0.1.0-preview.1`)

### Version Suffixes

- `-alpha.N` - Early development, unstable
- `-beta.N` - Feature complete, testing phase
- `-preview.N` - Release candidate, nearly stable
- `-rc.N` - Release candidate

## Setup Requirements

### 1. NuGet API Key

Before you can publish packages, you need to configure a NuGet API key as a GitHub secret:

1. Go to [nuget.org](https://www.nuget.org/) and sign in
2. Navigate to your account settings → API Keys
3. Create a new API key with "Push" permissions
4. In your GitHub repository, go to Settings → Secrets and variables → Actions
5. Create a new secret named `NUGET_API_KEY` with your API key value

### 2. Repository Access

Ensure GitHub Actions has permission to run workflows:
- Go to Settings → Actions → General
- Allow "Read and write permissions" for GITHUB_TOKEN

## Publishing a Release

### Option 1: Tag-based Release (Recommended)

1. Update the version in `src/abnf.net/abnf.net.csproj`:
   ```xml
   <Version>0.1.0-preview.2</Version>
   ```

2. Commit and push your changes:
   ```bash
   git add src/abnf.net/abnf.net.csproj
   git commit -m "Bump version to 0.1.0-preview.2"
   git push
   ```

3. Create and push a version tag:
   ```bash
   git tag v0.1.0-preview.2
   git push origin v0.1.0-preview.2
   ```

4. The GitHub Action will automatically:
   - Build the project
   - Run tests
   - Create the NuGet package
   - Publish to nuget.org

### Option 2: Manual Trigger

1. Go to Actions → "Publish NuGet Package"
2. Click "Run workflow"
3. Select the branch and click "Run workflow"

### Option 3: Local Publishing (for testing)

Build and pack locally:
```bash
dotnet pack src/abnf.net/abnf.net.csproj --configuration Release --output ./artifacts
```

Test the package locally:
```bash
dotnet nuget push ./artifacts/*.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
```

## Continuous Integration

The CI workflow runs automatically on:
- Pushes to `main` and `feature/*` branches
- Pull requests to `main`

The CI workflow will:
- Build the solution
- Run all tests
- Create a package artifact (without publishing)

## Pre-release Versions

Pre-release versions are useful for:
- Testing in real-world scenarios before official release
- Getting early feedback from users
- Publishing experimental features

Users can install pre-release versions with:
```bash
dotnet add package Abnf.Net --prerelease
```

Or in Visual Studio: Check "Include prerelease" in the NuGet Package Manager.

## Releasing a Stable Version

When ready for a stable release:

1. Remove the pre-release suffix in the .csproj:
   ```xml
   <Version>1.0.0</Version>
   ```

2. Update CHANGELOG.md (if exists) with release notes

3. Commit, tag, and push:
   ```bash
   git add .
   git commit -m "Release v1.0.0"
   git tag v1.0.0
   git push origin main --tags
   ```

## Troubleshooting

### Build Fails

- Check that all tests pass locally: `dotnet test`
- Ensure .NET 10.0 SDK is compatible with the build environment

### Package Already Exists

If you see "409 Conflict", the version already exists on NuGet. You must:
- Bump the version number
- NuGet doesn't allow overwriting existing versions

### API Key Issues

- Verify the `NUGET_API_KEY` secret is set correctly
- Check the API key hasn't expired on nuget.org
- Ensure the key has "Push" permissions

## Package Management Best Practices

1. **Never delete published packages** - Only unlist them if needed
2. **Test pre-releases thoroughly** before stable release
3. **Document breaking changes** in release notes
4. **Follow semantic versioning** strictly
5. **Keep dependencies minimal** and well-maintained

## Resources

- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [Semantic Versioning](https://semver.org/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
