Content
=======

This README.md serves to clarify the general concept of Smint.io Portals frontend components, which is page templates and UI components.
Also it contains an overview of a lot of currently existing default Smint.io Portals frontend componennts that are delivered and maintained directly by us.

Finally, it will shed some light on how you can get started with developing your own custom Smint.io Portals frontend components.

Please note that at any time you can build your own  page templates or UI components based on our Smint.io Portals SDKs.
Access to the SDKs is restricted. Get in contact with Smint.io and request access.
Access will be granted to either Smint.io Solution Partners or to all our Smint.io Portals Enterprise plan customers.

You will need an account with Microsoft Visual Studio cloud offerings (Azure DevOps), as the SDKs are hosted there.

1. [UI components](#ui-components)
1. [Page templates](#page-templates)
1. [Portal templates](#portal-templates)
1. [Data adapter public API interfaces](#data-adapter-public-api-interfaces)
1. [Overview of Smint.io UI components](docs/smintio-ui-components.md)
1. [Overview of Smint.io page templates](docs/smintio-page-templates.md)
1. [Overview of Smint.io mixins](docs/smintio-mixins.md)
1. [Overview of Smint.io annotations](docs/smintio-annotations.md)
1. [How to develop your own custom component](#user-content-how-develop-your-own-frontend-component)

Current version of this document is: 1.0.0 (as of 25th of November, 2022)

UI components
=============
The most atomic part of a Smint.io Portal is the *Smint.io Portals UI component*. A UI component could, for example, be:

- A header bar
- A text block
- A search result display
- And so on

For sure you know this concept also from popular page builders like Wix.

*Focus on simplicity*

To make the setup of a portal very easy and fast for a Smint.io Portals user, please avoid building very granular or
highly flexible UI components (like a single button, with a lot of options to customize).

It is desirable to build higher level UI components (like a complete metadata viewer) with less options, because any
excess granularity or flexibility will cause confusion and source of error. On top of that it will confuse users that
are not as tech savvy as we developers or designers are.

*Start simple, become more sophisticated over time*

So please build higher level UI components, and avoid a lot of options at the start. If needed, you can always introduce
additional options to your UI components and increase the flexibility of your UI component at any time later on.

*No nesting*

Please note, that components are high level elements. So, it is not possible to nest UI components. An UI component
may *NOT* contain another UI component.

*Note*: this applies to *Smint.io Portals UI components*. Of course you can nest non-Smint.io Portals UI components (aka
Vue.js components, see below) within your Smint.io Portals UI component as you like!

*Note*: there is a concept called *sections*. A section consists of the *section start* UI component (typically written
by you), and by the *section end* UI component (which is a generic component coming from us). Between the *section start*
and the *section end* UI component, users can add any number of other UI components. During rendering, the *section start*
UI component will get a reference to all the UI components within the section, and can then render the section accordingly.

The Smint.io Portals *smintio-ui-generic-section-start-table-1* UI component, for example, uses the section concept to implement a
very sophisticated and responsive table construct.

*Standalone*

UI components must *NOT* be dependent on the presence of any other UI component on the page, or on functionality
provided by the page itself. UI components should always be able to work standalone. UI components *MAY* communicate
with other UI components or with the page using props or events, but please keep this rule in mind.

*There is one exception*: the UI component can enforce through its settings to only be allowed to be added to a certain
page type (see below). E.g. a search page or an asset details page usually coordinates it's UI components and provides
certain functionality. In this case, the UI component may rely on the page to behave as it is being specified by the
page type interface specification.

*Layout*

The most important layout rule for UI components is, that the UI component content always *MUST* fill it's layout box.
The UI component is *NOT* responsible for padding and margin management outside of it's own layout box.

This means that the UI component *MUST NOT HAVE* any white space around it's content. This also means, that the
content *MUST NOT* exceed the UI component's layout box. This is to make sure that paddings and margins on the overall
page stay regular and look good (I am sure you know of situations where padding and margin management quickly became a
nightmare because of not following this rule).

*UI component ID and UI component type*

Each Smint.io Portals UI component has an ID that is being defined by the developer of the UI component. Please prefix
your UI component IDs by your domain (e.g. *smintio-* or *smint-io-*) to avoid ID clashes. On top of that it is assigned
to an UI component type. There is a predefined list of UI component types (see below) which is maintained by Smint.io.
If you need an additional UI component type, please get in touch.

*Instance creation*

UI component instances are being created either a) when the user creates a new portal from a portal template (see below)
or b) when the user creates a new page from a page template (see below) or c) when the user adds a new UI component to a
page.

*SDK*

Smint.io Portals UI components can be developed using the *SmintIo-UIComponents-SDK*. The SDK is based on *TypeScript*, 
*Vue.js* and *Vuetify*. Vue.js *props* can be made available for editing UI component properties by the user through *annotations*. 
The UI components can then be published to any NPM repository from where our system can consume the
components. You can locally develop Smint.io Portals UI components while having them run in our production system by
running our very simple *Smint.io Portals DevServer*.

Page templates
==============

The second level of structure on top of UI components is introduced by *Smint.io Page Templates*. A page template
defines the structure of a Smint.io Portals page by defining and rendering so-called *slots* (do NOT confuse with Vue.js
slots, which is a completely different concept to Smint.io Portals slots). The typical slots of a page could for example
be:

- Header slot
- Left slot
- Right slot
- Content slot
- Footer slot

Each slot can optionally restrict or enforce the *UI component types* that fit into that slot (e.g. the header slot is
only allowed to contain UI components of type "Header"). Also the *minimum or maximum number of UI components* that fit
into the slot can be optionally defined.

The page template will have the instantiated UI components for the slots injected by the Smint.io Portals runtime
environment upon render time and is responsible to render the UI components to the DOM.

*Layout*

Page templates are responsible for padding and margin management. This means that page templates *MUST* take care about
padding and margins between all of their UI components.

*Page template ID and page type*

Each Smint.io Portals page template has an ID that is being defined by the developer of the page template. Please prefix
your page template IDs by your domain (e.g. *smintio-* or *smint-io-*) to avoid ID clashes. On top of that it is
assigned to a page type. There is a predefined list of page types (see below) which is maintained by Smint.io. If you
need an additional page type, please get in touch.

*Instance creation*

Pages are being created from a page template either a) when the user creates a new portal from a portal template (see
below) or b) when the user creates a new page from a page template.

*SDK*

Smint.io page templates can be developed using the *SmintIo-PageTemplate-SDK*. The SDK is based on *TypeScript*, *
Vue.js* and *Vuetify*. Vue.js *props* can be made available for editing page template properties by the user through *
annotations*. The page templates can then be published to any NPM repository from where our system can consume the page
templates. You can locally develop Smint.io Portals page templates while having them run in our production system by
running our very simple *Smint.io Portals DevServer*.

Portal templates
================

To wrap the story up, the topmost level of the structure of a Smint.io Portals portal is introduced by *Smint.io portal
templates*. A portal template combines and configures page templates for a complete portal experience. The portal
template itself does *NOT* render any output to the frontend itself, it is just being used when instantiating a new
Smint.io Portal from the portal template.

*Portal template ID and portal type*

Each Smint.io Portals portal template has an ID that is being defined by the developer of the portal template. Please
prefix your portal template IDs by your domain (e.g. *smintio-* or *smint-io-*) to avoid ID clashes. On top of that it
is assigned to a portal type. There is a predefined list of portal types (see below) which is maintained by Smint.io. If
you need an additional portal type, please get in touch.

*SDK*

Smint.io portal templates can be developed using the *SmintIo-PortalTemplate-SDK*. The SDK is based on *C#* (.NET Core).
Template properties can be made available for editing by the user through *annotations*. Portal templates need to be
submitted to Smint.io for approval and inclusion to our portal template library.

Data adapter public API interfaces
==================================

Each Smint.io Portals UI component can tie itself to public API interfaces published by Smint.io Portals data adapters.

You can do this by requesting a data adapter public API interface through the configuration of the Smint.io Portals
UI component:

```javascript
import type {
    IAssetsSearch,
} from "@smintio/portals-component-sdk";

...

@DisplayName("en", "Data source for auto completion", true)
@DisplayName("de", "Daten-Quelle für die Auto-Vervollständigung")
@Description("en", "The data source to query the search bar auto completion suggestions from.", true)
@Description("de", "Die Daten-Quelle, aus der die Vorschläge für die Auto-Vervollständigung für die Such-Eingabeleiste geladen werden.")
@Implements("IAssetsSearch")
@ComponentProperty({ name: "searchBarAutoCompletion" })
@FormGroup("s-search-bar")
public readonly searchBarAutoCompletion!: IAssetsSearch;
```

Once the Smint.io Portals UI component is instanciated, you can easily call methods of that public API interface.

```javascript
this.searchBarAutoCompletion.getFullTextSearchProposalsAsync({ queryString: this.searchQuery })
	.catch((e) => {
		...
	})
	.then((searchProposals?: IGetFullTextSearchProposalsResult) => {
		...
	})
	.finally(() => {
		...
	});
```

In the above example, the `IAssetsSearch` is a standard public API interface provided by Smint.io Portals. 

*The great thing is*: if you develop your own Smint.io Portals data adapter, you can easily publish your own custom 
public API interfaces as well. This enables you to easily develop any custom functionality required using the Smint.io 
Portals component framework and runtime.

However, for use of your custom public API interfaces in a Smint.io Portals UI component you'll need its Typescript 
public API interface definition.

Use the [Smint.io Portals Data Adapter Exporter CLI tool](https://github.com/smintio/Portals-Components-SDK/tree/main/Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release) 
to generate the Typescript public API interface definition directly from your Smint.io Portals data adapter assembly. 

You can then simple use that Typescript public API interface definition file
in your Smint.io Portals UI component.

*All the wiring from frontend to backend is done for you, without any further work involved.*

Learn more about how to do that [here](https://github.com/smintio/Portals-Components-SDK/tree/main/Examples/Backend#custom-public-api-interfaces).

How develop your own frontend component
=======================================

A good starting point is looking at our [Hello world](ui-example-hello-world-1/) example. It is an example of a Smint.io Portals UI component.
An example for crafting your own page template will follow soon.

The [Vue.js component itself](ui-example-hello-world-1//src//PortalsUiComponent.vue) is pretty basic. 
It can display colored static text once added to a page.

The source follows established Vue.js structure practices by containing a template, typescript, and css sections.

`PortalsUiComponent` is annotated with custom attributes which contribute to the component description in way meaningful to Smint.io.

```javascript
@PortalsUiComponent({
    type: "ui-type-text",
    key: "smintio-ui-example-hello-world-1",
    displayName: {
        [DefaultCulture]: "Hello world",
        de: "Hallo Welt",
    },
    description: {
        [DefaultCulture]: "This component displays a message with an optional color.",
        de: "Diese Komponente dient zur Darstellung eines Banners mit optionalem Titel und einer Suchleiste.",
    },
})
```

The full list of supported Smint.io Portals frontend component types can be seen [here](docs/smintio-frontend-component-types.md).

The `PortalsUiComponent` implementation exports fully localized properties that are interpreted by the Smint.io pages as `FormGroup`.
Additional attributes such as `DynamicAllowedValuesProvider` or `IsColor` control how the component configuration can look.
In this case, an example would be a text input field or a color picker.

The full list of supported Smint.io Portals annotations can be found [here](docs/smintio-annotations.md).

By default, [package.json](ui-example-hello-world-1/package.json) is used by the npm CLI (and others) to identify the component and how to handle its relevant dependencies.

In addition to the minimal and default settings, custom script sections are added to make it easier to deploy components to different Smint.io environments using our publish component CLI tool. 

More information about building, packaging and publishing Smint.io Portals components using the *Portals-SDK-PublishComponent-CLI tool* can be found [here](../../Tools/Portals-SDK-PublishComponent-CLI/Release/).

An example would be
```json
"scripts": {
    "smint-io-pc": "npm publish && npm info --json | %SMINT_IO_SDK_HOME%\\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env staging"
    ...
}
```

Running the following command will package `ui-example-hello-world-1` in the defined npm registry and then trigger a REST API request to register the package for the specified environment.

```console
npm run smint-io-pc
```

Please note that calling the script repeatedly will result in an error.

With each code change, the version number must be increased.

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) to request publishing.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yanko Belov, Smint.io GmbH
- Yosif Velev, Smint.io GmbH