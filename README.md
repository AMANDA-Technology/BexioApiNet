# BexioApiNet

Unofficial API client implementation for the Bexio v3 API. See [API Doc](https://docs.bexio.com/). (used version 3.0.0)

NOTE: Since Bexio does not provide free test accounts, this project will be temporarily discontinued. If you have landed here and are looking for a more comprehensive Bexio API Client Library for .Net, please contact us and we will look for a solution together! Or if you are someone from Bexio and you would like to offer a .Net library to your customers, you are welcome to contact us for a cooperation. 🙌

See [Bexio website](https://www.bexio.com/).

### Packages

- BexioApiNet: Client service to interact with the Bexio API
- BexioApiNet.Abstractions: Models, Views and Enums used for the API
- BexioApiNet.AspNetCore: Dependency injection in ASP.NET Core

[![BuildNuGetAndPublish](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/main.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/main.yml)

[![PR CI](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/pr.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/pr.yml)

[![CodeQL](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/codeql-analysis.yml)

[![SonarCloud](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/sonar-analysis.yml/badge.svg)](https://github.com/AMANDA-Technology/BexioApiNet/actions/workflows/sonar-analysis.yml)

### Branch protection

The `PR CI` check (build + offline unit tests) is intended to be **required** on `main`. Configure under *Settings → Branches → Branch protection rules → main*: require status check `Build and test (offline)`. E2E tests are never invoked in CI — they need Bexio credentials and run only locally.
