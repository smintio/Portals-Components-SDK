### Content
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

You can manage three different environments for your component development efforts ("Development", "Staging" and "Production").
For each of those environments, you can specify different connection data:

- appsettings.json contains production settings for publishing of components
- appsettings.Development.json contains settings for local development (inherits from appsettings.json, if a setting is not given)
- appsettings.Staging.json contains settings to stage developed components (inherits from appsettings.json, if a setting is not given)

Sample `appsettings.Development.json`

```json
{
  "RedirectUrl": "http://development-portal.smint.io:43450/signin-oidc",
  "SmintIo": {
    "ApiUrl": "https://portals-development.portalsapib.smint.io:43444",
    "Auth": {
      "Authority": "https://development-login.smint.io:40443/",
      "ClientId": "smintio",
      "ClientSecret": "********-****-****-****-4f105a645134"
    },
    "AuthorizationHeader": "Basic ************************************************************************YTZ6ajRh"
  }
}
```

Please request your specific configuration values from [support@smint.io](mailto:support@smint.io).

Parameters
==========

The CLI tool has parameters that can be listed by executing the binary with one of the arguments `/?`, `/help` or `--help`

Example: `%SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe /?`

- The only parameter that has to be specified is for the desired environment in which to deploy.
    - This can be done by running the tool with `-env` or `--environment` argument followed by the environment name.
    - Possible values are "Development", "Staging" or "Production".
    - The parameter can be skipped if the `ASPNETCORE_ENVIRONMENT` environment variable is set with a desired environment value.
- Optional parameter is `-dir` or `--directory`
    - This is in case a the working directory should be different from the directory where the binary is being executed.
- Optional parameter is `-all` or `--all-tenants`
    - Intended for use by the Smint.io staff only.
- Lastly `--version` shows the tool version

Publish and deploy
==================

### Frontend components 

The target component's npm "package.json" file can be extended with additional "scripts" properties.

```javascript
"scripts": {
    ...
    "smint-io-pc:development": "npm publish && npm info --json | %SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env development",
    "smint-io-pc:staging": "npm publish && npm info --json | %SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env staging",
    ...
}
```

The CLI executable requires the value of `npm info --json` as standard input so that it can determine the component name, version, and npm repository.

#### When the target component is ready for publish:

In the component folder, issue a single command to push your component to a npm repository and then deploy it to Smint.io from there.

```console
npm run smint-io-pc:development
```

Please note that the examples target a Windows environment.

For Linux, the correct environment variable syntax must be used.

### Backend components

Like front-end components, a single command can be used to publish and deploy the backend component to Smint.io.

Under a command prompt in a component folder run

```console
%SMINT_IO_SDK_HOME%\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env development
```

Behind the scenes the publish component tool will look for `*.csproj` file to compile, package and deploy the component.

Please note that all project and external package references will be included in the package.

For a development environment only, the component will be compiled using the debug profile. Others will use the release profile.

Optionally backend developers can register the tool as a global CLI tool.

#### Register the tool as a global CLI tool

1. Install as a global tool
    - dotnet tool install --global --add-source `<SMINT-IO-PRIVATE-SOURCE-FEED>` SmintIo.Portals.SDK.PublishComponent.CLI
1. Confirm the message
	- You can invoke the tool using the following command: smint-io-pc
	- Tool 'smintio.portals.sdk.publishcomponent.cli' (version '1.0.0.0') was successfully installed
	- The version number may vary
1. Go to dotnet global tools folder
	- For Windows go to '%USERPROFILE%\.dotnet\tools'
	- For Linux go to '$HOME/.dotnet/tools'
1. Go to folder '.store\smintio.portals.sdk.publishcomponent.cli\1.0.0.0\smintio.portals.sdk.publishcomponent.cli\1.0.0.0\tools\net6.0\any'
	- The version number may vary
1. Edit the applicable development, staging, and/or production app settings files similarly to [App Settings](#app-settings)
1. Useful links
	- https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-use
	- https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install

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

- Yanko Belov, Smint.io GmbH