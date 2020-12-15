# Building & Testing

## Building

```sh
dotnet build
```

### Release version

If you want to build release configuration of the library, you need to specify what version the output package will have. That can be done with `VERSION` environment variable or by passing `VERSION` as MSBuild parameter to the `build` command. CI also specifies `GIT_COMMIT` that is appended to `InformationalVersion` property of the assemblies to mark exact source code.

## Testing

The framework can be unit-tested by `cd`ing into `test` folder and calling

```sh
dotnet msbuild /t:RunTests
```

Moreover, there are some integration-style tests that require external services. They can be tested with `docker` and `docker-compose` tool. There is one integration-test suite currently:

 1. `test/LeanCode.IntegrationTests` - _real_ integration tests,

It has a `docker` folder that contains necessary configuration. You can run the suite using:

```sh
docker-compose run test
```

## Publishing

After successful test, packages can be packed with

```sh
dotnet pack -c Release -o $PWD/publish
```

and then published to NuGet feed with

```sh
dotnet nuget push 'publish/*.nupkg'
```

provided that API Key is correctly specified in profile/machine `NuGet.Config`.
