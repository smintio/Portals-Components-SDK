Content
=======
1. [Description](#description)
1. [Download](#download)
1. [App settings](#app-settings)
1. [Publish and deploy](#publish-and-deploy)
1. [Usage](#usage)

Description
===========

By default Smint.io uses CDN to distribute frontend components, but for local use, this is not the case.
For development, our Portals-DevServer tool can be used to serve a set of components.

Current version of this document is: 1.0.0 (as of 30th of November, 2022)

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