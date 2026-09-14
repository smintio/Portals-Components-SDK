The Portals-DevServer
=====================

1. [Description](#user-content-description)
1. [Download](#user-content-download)
1. [App settings](#user-content-app-settings)
1. [Usage](#user-content-usage)
1. [When nothing is served](#user-content-when-nothing-is-served)

Current version of this document is: 1.2.0 (as of 14th of September, 2026)

## Description

By default, Smint.io distributes frontend components through a CDN. This means, that you cannot change a frontend component easily
and quickly update them in your browser without building, packaging and publishing the frontend component to the Smint.io servers
using the [Portals-SDK-PublishComponent-CLI tool](../../../Portals-SDK-PublishComponent-CLI/Release/).

The Portals-DevServer fixes that problem. Once you point the dev server to your local frontend component build directory, it is able to
serve the JavaScript of the frontend component to your browser directly, without going through the CDN.

This means that you can use e.g. `npm run watch` to quickly build your frontend component whenever you change it. Then you just need
to refresh the browser (*do not forget to turn off the browser cache!*) for the changes to take effect.

For this to work, *the portal you are using to test your frontend components needs to be configured* to look up frontend components
from the dev server before loading them from our CDN. We can do that for you. Please get in touch with [support@smint.io](mailto:support@smint.io)
to start the process.

**Two conditions have to be met on the portal side before any request reaches your dev server**, and neither of them is
visible from the dev server itself:

1. the portal must have *development mode* switched on in its basic settings, and
2. the user looking at the portal must be signed in as a user that Smint.io has cleared for component development.

The second one is easy to overlook: the clearance sits on the signed-in portal user, so an anonymous visitor never
qualifies, however correct everything else is. With either condition unmet, the portal renders normally, loads the
published component from the CDN, and your dev server sits there with an empty log.

*Please note, that this only works for frontend component JavaScript changes!* If you want to change the *configuration options* of
your frontend component, you need to build, package and publish the frontend component to the Smint.io servers for the change to take
effect!

Please note, that you need to build, package and publish the frontend component *at least once* before the dev server can work!

## Download

Download the ZIP archive suitable for your operating system to a convenient location — builds are
provided for Windows (`win-x64`), Linux (`linux-x64`), macOS on Intel (`osx-x64`) and macOS on
Apple Silicon (`osx-arm64`). Each one is self-contained, so there is no .NET runtime to install.

Extract the archive.

Trust the rootCA.pem self signed root authority, to allow HTTPS connection to your local dev server. 
The root authority certificate has been issued by Smint.io.

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

The DevServer tool can be started by executing the binary for the target operating system —
`SmintIo.Portals.DevServer.exe` on Windows, `./SmintIo.Portals.DevServer` on Linux and macOS.

During initialization the server will list all known frontend component configuration mappings.

It listens on `development-host.smint.io`, on **HTTPS port 8443** and HTTP port 8000. The portal
always asks for the HTTPS one, so 8443 is the port that matters. Please note that the startup
banner prints an `https://…:8000` URL, which is wrong — ignore it.

## When nothing is served

If the portal shows the published version of your component and the dev server log stays empty, work through this order —
it is almost always one of the first two:

| Check | How it looks |
|---|---|
| Is *development mode* on for this portal, and are you signed in as a developer-cleared user? | The dev server log is completely empty — not a single request arrives |
| Did you restart the dev server after editing `appsettings.json`? | The mapping you just added is missing from the list printed at startup |
| Is the component on this page at all? | The browser's network tab shows a different component bundle being fetched — the page is not using yours |
| Is the browser cache off? | The same old bundle is served repeatedly although the file on disk has changed |
| Did you build at least once? | The mapped directory holds no build output to serve |

The component bundle's URL carries the *registered* version number, so the browser's network tab is also the quickest way
to confirm which version a portal has registered — worth checking after a publish.

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) for any questions regarding the dev server.

Contributors
============

- Yosif Velev, Smint.io GmbH
- Reinhard Holzner, Smint.io GmbH