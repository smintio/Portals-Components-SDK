Content
=======
1. [Description](#description)
1. [Download](#download)
1. [Environment Variable](#environment-variable)
1. [App settings](#app-settings)
1. [Publish and deploy](#publish-and-deploy)

Description
===========

The purpose of the Portals-SDK-PublishComponent-CLI tool is to enable developers with a short cycle to publish and deploy frontend and backend components to Smint.io.

This document contains all the steps needed to configure and run the Smint.io component publishing tool.

Please note that at any time you can build your own components based on our *Smint.io Portals SDKs*. Access to the SDKs is restricted.
Get in touch with [Smint.io](https://www.smint.io) or [support@smint.io](mailto:support@smint.io) to request access.
Access will be granted to either Smint.io Solution Partners or to all our Smint.io Portals.
Enterprise plan customers.

Current version of this document is: 1.0.0 (as of 24th of November, 2022)

Download
========

Download the ZIP archive suitable for your operating system to a convenient location.

Extract the archive.

Environment variable
====================

Create an environment variable named `SMINT_IO_SDK_HOME` pointing to the location from the previous step.
Doing so will allow the publishing tool to be invoked from different locations.
- For Windows `%SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe`
- For Linux `$SMINT_IO_SDK_HOME/SmintIo.Portals.SDK.PublishComponent.CLI`

App settings
============

You can manage three different environments for your component development efforts (`Development`, `Staging` and `Production`).
For each of those environments, you can specify different connection data:

- appsettings.json contains production settings for publishing of components
- appsettings.Development.json contains settings for local development (inherits from appsettings.json, if a setting is not given)
- appsettings.Staging.json contains settings for staging developed components (inherits from appsettings.json, if a setting is not given)

Sample `appsettings.Staging.json`

```json
{
  "RedirectUrl": "http://someone-staging.smint.io/signin-oidc",
  "SmintIo": {
    "ApiUrl": "https://someone-staging.portalsapib.smint.io",
    "Auth": {
      "Authority": "https://staging-login.smint.io/",
      "ClientId": "someone",
      "ClientSecret": "********-****-****-****-4a215b643134"
    },
    "AuthorizationHeader": "Basic ************************************************************************YTX6fdRh"
  }
}
```

Please request your specific `SmintIo` configuration `ApiUrl` and `Auth` values from [support@smint.io](mailto:support@smint.io).

The `AuthorizationHeader` value is used during publishing of a frontend component to authorize the CLI tool against the client private npm feed.

Keep in mind that the `AuthorizationHeader` value is not provided by Smint.io

Parameters
==========

The CLI tool has parameters that can be listed by executing the binary with one of the arguments `/?`, `/help` or `--help`

Example: `%SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe /?`

- The only parameter that has to be specified is for the desired environment to which to deploy.
    - This can be done by running the tool with `-env` or `--environment` argument followed by the environment name.
    - Possible values are `Development`, `Staging` or `Production`.
    - The parameter can be skipped if the `ASPNETCORE_ENVIRONMENT` environment variable is set with a desired environment value.
- Optional parameter is `-dir` or `--directory`
    - Use this in case the working directory is different from the directory where the binary is being executed.
- Optional parameter is `-all` or `--all-tenants`
    - Intended for use by Smint.io staff only.
- Lastly, `--version` shows the tool version

Publish and deploy
==================

### Frontend components 

The target component's npm "package.json" file can be extended with additional "scripts" properties.

```json
"scripts": {
    "smint-io-pc:development": "npm publish && npm info --json | %SMINT_IO_SDK_HOME%\\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env development",
    "smint-io-pc:staging": "npm publish && npm info --json | %SMINT_IO_SDK_HOME%\\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env staging",
    ...
}
```

The CLI executable requires the value of `npm info --json` as standard input so that it can determine the component name, version, and npm repository.

#### When the target component is ready for publish:

In the component folder, issue a single command to push your component to a npm repository and then deploy it to Smint.io from there.

```console
npm run smint-io-pc:development
```

*Please note that the NPM repository needs to be publicly available, so that Smint.io can retrieve your component.*

*Please note that the examples target a Windows environment. For Linux, the correct environment variable syntax must be used.*

### Backend components

Like for frontend components, a single command can be used to publish and deploy your backend component to Smint.io.

Under a command prompt in the component folder run

```console
%SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env development
```

Behind the scenes the publish component tool will look for `*.csproj` file to compile, package and deploy the component.

Please note that all project and external package references will be included in the package.

For the development environment only, the component will be compiled using the debug build profile. All other environments will use the release build profile.

Optionally, backend developers can register the tool as a global CLI tool:

#### Register the tool as a global CLI tool

1. Install as a global tool
    - dotnet tool install --global --add-source `<SMINT-IO-PRIVATE-SOURCE-FEED>` SmintIo.Portals.SDK.PublishComponent.CLI
1. Confirm the message
    - Tool 'smintio.portals.sdk.publishcomponent.cli' (version '1.0.0.0') was successfully installed
	- The version number may vary
	- You can then invoke the tool using the following command: `smint-io-pc`
1. Go to the .NET global tools folder
	- For Windows, go to `%USERPROFILE%\.dotnet\tools`
	- For Linux, go to `$HOME/.dotnet/tools`
1. Go to folder `.store\smintio.portals.sdk.publishcomponent.cli\1.0.0.0\smintio.portals.sdk.publishcomponent.cli\1.0.0.0\tools\net6.0\any`
	- The version number may vary
1. Edit the applicable development, staging, and/or production app settings files similarly to [App Settings](#app-settings)
1. Useful links
	- [Install and use a .NET global tool using the .NET CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-use)
	- [dotnet tool install](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install)

#### Uninstall the global CLI tool

1. Run
	- dotnet tool uninstall --global SmintIo.Portals.SDK.PublishComponent.CLI
1. Confirm the message
	- 'Tool 'smintio.portals.sdk.publishcomponent.cli' (version '1.0.0.0') was successfully uninstalled.'

#### Publish using the registered global CLI tool

If you registered our tool as a global CLI tool, you can use this simplified command for publishing to Smint.io:

```console
smint-io-pc -env development
```

Contributors
============

- Yosif Velev, Smint.io GmbH
- Reinhard Holzner, Smint.io GmbH