Content
=======
1. [Description](#description)
1. [Download](#download)
1. [App settings](#app-settings)
1. [Publish and deploy](#publish-and-deploy)
1. [Usage](#usage)

Current version of this document is: 1.0.0 (as of 30th of November, 2022)

Description
===========

By default, Smint.io distributes frontend components through a CDN. This means, that you cannot change a frontend component easily
and quickly update them in your browser without building, packaging and publishing the frontend component to the Smint.io servers
using the [Portals-SDK-PublishComponent-CLI tool](../../Portals-SDK-PublishComponent-CLI/Release/).

The Portals-DevServer fixes that problem. Once you point the dev server to your local component build directory, it is able to
serve the JavaScript of the frontend component to your browser directly, without going through the CDN.

This means that you can use e.g. `npm run watch` to quickly build your frontend component whenever you change it. Then you just need
to refresh the browser (*do not forget to turn off the browser cache!*) for the changes to take effect.

For this to work, *the portal you are using to test your frontend components needs to be configured* to look up frontend components
from localhost before loading them from our CDN. We can do that for you. Please get in touch with [support@smint.io](mailto:support@smint.io)
to start the process.

*Please note, that this only works for frontend component JavaScript changes!* If you want to change the *configuration options* of
your frontend component, you need to build, package and publish the frontend component to the Smint.io servers for the change to take
effect!

Download
========

Download the ZIP archive suitable for your operating system to a convenient location.

Extract the archive.

App settings
============

A few things needs to be configured for Portals-DevServer to work properly.

Inside the `appsettings.json` file, please configure the following

- AllowedHosts - By default the value is `*`
- RootDirectory - This is the base path of the components location. This is optional
- ComponentMappings - A key pair of the component name and its location relative to the `RootDirectory`

Please note that the `RootDirectory` is optional, but then `ComponentMappings` must contain the full path.

By default all Smint.io frontend components are added to the `appsettings.json` file.

When a change is made to the `appsettings.json` file and the server is running, Portals-DevServer must be restarted.

Usage
=====

The DevServer tool can be started by executing the binary for the target operating system.

During initialization the server with list all known configuration mappings.

If the component is not listed in the mappings, the server will display a message indicating that the component is not recognized.

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) for any questions regarding Portals-DevServer.

Contributors
============

- Yosif Velev, Smint.io GmbH
- Reinhard Holzner, Smint.io GmbH