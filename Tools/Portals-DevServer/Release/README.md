The Portals-DevServer
=====================

1. [Description](#description)
1. [Download](#download)
1. [App settings](#app-settings)
1. [Publish and deploy](#publish-and-deploy)
1. [Usage](#usage)

Current version of this document is: 1.0.0 (as of 30th of November, 2022)

## Description

By default, Smint.io distributes frontend components through a CDN. This means, that you cannot change a frontend component easily
and quickly update them in your browser without building, packaging and publishing the frontend component to the Smint.io servers
using the [Portals-SDK-PublishComponent-CLI tool](../../Portals-SDK-PublishComponent-CLI/Release/).

The Portals-DevServer fixes that problem. Once you point the dev server to your local frontend component build directory, it is able to
serve the JavaScript of the frontend component to your browser directly, without going through the CDN.

This means that you can use e.g. `npm run watch` to quickly build your frontend component whenever you change it. Then you just need
to refresh the browser (*do not forget to turn off the browser cache!*) for the changes to take effect.

For this to work, *the portal you are using to test your frontend components needs to be configured* to look up frontend components
from the dev server before loading them from our CDN. We can do that for you. Please get in touch with [support@smint.io](mailto:support@smint.io)
to start the process.

*Please note, that this only works for frontend component JavaScript changes!* If you want to change the *configuration options* of
your frontend component, you need to build, package and publish the frontend component to the Smint.io servers for the change to take
effect!

Please note, that you need to build, package and publish the frontend component *at least once* before the dev server can work!

## Download

Download the ZIP archive suitable for your operating system to a convenient location.

Extract the archive.

## App settings

A few things needs to be configured for Portals-DevServer to work properly.

Inside the `appsettings.json` file, please configure the following

- AllowedHosts - By default the value is `*`
- RootDirectory - This is the base path of the build location of your frontend components
- ComponentMappings - A key pair mapping the frontend component to its location relative to the `RootDirectory`

Each ComponentMapping key should be considered like a `*[KEY]*` filter on the name of your frontend component's npm package.
This means that if your frontend component's npm package name is `ui-generic-account-accept-terms-form-1-6.3.0.tgz`, it is sufficient
to e.g. type `ui-generic-account-accept-terms-form` to map this frontend component.

Please note that the `RootDirectory` is optional, but then `ComponentMappings` must contain the full path.

When a change is made to the `appsettings.json` file and the dev server is running, the dev must be restarted for the changes to take effect.

## Usage

The DevServer tool can be started by executing the binary for the target operating system.

During initialization the server with list all known frontend component configuration mappings.

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) for any questions regarding the dev server.

Contributors
============

- Yosif Velev, Smint.io GmbH
- Reinhard Holzner, Smint.io GmbH