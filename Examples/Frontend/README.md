Developing Smint.io Portals frontend components
===============================================

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
1. [Overview of Smint.io frontend component types](docs/smintio-frontend-component-types.md)
1. [Frontend reference: services, filters, property mixins, providers, CSS](docs/smintio-frontend-reference.md)
1. [Data adapter reference: every public API interface and model](docs/smintio-data-adapter-reference.md)
1. [Page type contracts: what a page hands to the components it hosts](docs/smintio-page-type-contracts.md)
1. [How to develop your own custom component](#user-content-how-develop-your-own-frontend-component)
1. [Before you start: the questions to answer](#user-content-before-you-start-the-questions-to-answer)
1. [Crafting a page template instead](#user-content-crafting-a-page-template-instead)
1. [Building a section component](#user-content-building-a-section-component)
1. [Extras for Smint.io Certified partners](#user-content-extras-for-smintio-certified-partners)
1. [Problems](#user-content-problems)

Current version of this document is: 1.2.0 (as of 11th of September, 2026)

## UI components

The most atomic part of a Smint.io Portal is the *UI component*. A UI component could, for example, be:

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

*Note*: this applies to *UI components*. Of course you can nest non-Smint.io Portals UI components (aka
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

*Settings*

Your Smint.io Portals UI component should only have settings that actually work. Please do *NEVER* add settings to 
your Smint.io Portals UI component that have no effect!

*UI component ID and UI component type*

Each Smint.io Portals UI component has an ID that is being defined by the developer of the UI component. Please prefix
your UI component IDs by your partner ID (e.g. `smintio-`) to avoid ID clashes. On top of that it is assigned
to an UI component type. There is a predefined list of UI component types (see below) which is maintained by Smint.io.
If you need an additional UI component type, please get in touch.

*Instance creation*

UI component instances are being created either a) when the user creates a new portal from a portal template (see below)
or b) when the user creates a new page from a page template (see below) or c) when the user adds a new UI component to a
page.

*SDK*

Smint.io Portals UI components are developed against the *Smint.io Portals UI component SDK*, which you consume as the
npm package `@smintio/portals-component-sdk`, usually together with the shared component library
`@smintio/portals-components`. The SDK is based on *TypeScript*, *Vue.js 2* and *Vuetify 2*. Vue.js *props* can be made
available for editing UI component properties by the user through *annotations*.

The UI components can then be published to any NPM repository from where our system can consume the
components. You can locally develop Smint.io Portals UI components while having them run in our production system by
running our very simple *Smint.io Portals DevServer*.

## Page templates

The second level of structure on top of UI components is the *page template*. A page template
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

*Settings*

Your Smint.io Portals page template should only have settings that actually work. Please do *NEVER* add settings to 
your Smint.io Portals page template that have no effect!

*Page template ID and page type*

Each Smint.io Portals page template has an ID that is being defined by the developer of the page template. Please prefix
your page template IDs by your partner ID (e.g. `smintio-`) to avoid ID clashes. On top of that it is
assigned to a page type. There is a predefined list of page types (see below) which is maintained by Smint.io. If you
need an additional page type, please get in touch.

*Instance creation*

Pages are being created from a page template either a) when the user creates a new portal from a portal template (see
below) or b) when the user creates a new page from a page template.

*SDK*

Smint.io page templates are developed against the very same npm SDK as UI components — `@smintio/portals-component-sdk`,
usually together with `@smintio/portals-components`. There is no separate npm SDK for page templates. The SDK is based
on *TypeScript*, *Vue.js 2* and *Vuetify 2*. Vue.js *props* can be made available for editing page template properties
by the user through *annotations*. The page templates can then be published to any NPM repository from where our system
can consume the page templates. You can locally develop Smint.io Portals page templates while having them run in our
production system by running our very simple *Smint.io Portals dev server*.

The differences of a page template to a UI component are small and are described under
[How develop your own frontend component](#user-content-how-develop-your-own-frontend-component) below.

## Portal templates

To wrap the story up, the topmost level of the structure of a Smint.io Portals portal is introduced by the *portal
template*. A portal template combines and configures page templates for a complete portal experience. The portal
template itself does *NOT* render any output to the frontend itself, it is just being used when instantiating a new
Smint.io Portal from the portal template.

*Portal template ID and portal type*

Each Smint.io Portals portal template has an ID that is being defined by the developer of the portal template. Please
prefix your portal template IDs by your partner ID (e.g. `smintio-`) to avoid ID clashes. On top of that it
is assigned to a portal type. There is a predefined list of portal types (see below) which is maintained by Smint.io. If
you need an additional portal type, please get in touch.

*SDK*

Smint.io portal templates can be developed using the *Smint.io Portals portal template SDK*
(`SmintIo.Portals.PortalTemplateSDK`). Unlike UI components and page templates, this SDK is based on *C#* (.NET).
Template properties can be made available for editing by the user through *annotations*. Portal templates need to be
submitted to Smint.io for approval and inclusion to our portal template library.

## Data adapter public API interfaces

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
this.searchBarAutoCompletion.getFullTextSearchProposalsAsync({ searchQueryString: this.searchQuery })
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
Every standard interface, every parameter and result model, and every enumeration is listed in the
[data adapter reference](docs/smintio-data-adapter-reference.md) — including `IAssetDataObject`, the shape of an
asset, which is what most components end up rendering.

Two things to keep in mind when calling a data adapter public API interface:

- The property can be `undefined`, because the portal editor may not have configured a data source for it. Guard for it.
- Each data adapter will implement all the methods of the interfaces it supports. If functionality is not supported,
  the data adapter does not declare the interface, and it can then not be selected for the purpose requested by you.

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

## How develop your own frontend component

### Before you start: the questions to answer

A custom frontend component is quick to write and expensive to change once it is out. The
component `key`, its `type`, the persisted names of its configuration properties and the tenant
it is published for are all effectively permanent as soon as a portal has been configured
against it.

So please settle the following before you create the first file. If you are building the
component for a customer, for a colleague or for a project team, ask them all of it in one go —
it is a short conversation, and it saves a rewrite.

*Identity and placement*

| Question | What it decides | If it is wrong |
|---|---|---|
| Who is the component for, and what does it do — in one sentence? | the directory name, and with it the package name and the component `key` | the `key` is what portals store in their page configuration. It cannot be changed after release |
| Is this a new component, or a variation of one you already have? | whether you start a fresh `-1`, or add a `-2` package next to the existing one | never renumber a released component. Portals in the field are configured against the old name |
| Which frontend component type does it have? | which page template slots will accept it, and which page contract it can rely on — see [the frontend component types](docs/smintio-frontend-component-types.md) | a type that does not exist cannot be invented on your side. If none of them fits, get in touch at [support@smint.io](mailto:support@smint.io) |

*Where it sits, and where its content comes from*

| Question | What it decides |
|---|---|
| Which portal types is it for, and which page does it go on? | whether you restrict it with `allowedPortalTypes`, and which contract applies — [what a page hands to the components it hosts](docs/smintio-page-type-contracts.md) |
| Where does the content come from: the component's own configuration, a Smint.io data adapter, or a system of your own? | whether you work against the [data adapter public API interfaces](docs/smintio-data-adapter-reference.md), or declare your own interface |
| Which existing component is the closest starting point? | how much you have to write. `ui-example-hello-world-1` is the minimal skeleton; the [overview of Smint.io UI components](docs/smintio-ui-components.md) shows what already exists |
| Which settings must the portal user be able to change, and which of them are advanced? | your configuration properties. Start with few — you can add options later, you cannot take them away. Text, link, colour and layout settings already exist as [mixins](docs/smintio-mixins.md), so only ask for what those do not cover |

*Content and language*

| Question | What it decides |
|---|---|
| Which languages does the component have to offer? | the `@DisplayName` and `@Description` annotations, and whether user-editable text is a localized string rather than a plain one |
| Is anything pre-filled or shipped with the component — default values, string resources, images, an icon for the page editor? | the resource definition and the contents of your `resources` folder. A component that arrives blank in the page editor looks broken |
| What should it look like: a design, a screenshot, an existing page to match, or your own judgement? | there is no other source for this, and it is the question most often left unasked |

*Delivery*

| Question | What it decides |
|---|---|
| Which tenant is the component published for? | who can actually use it. Nothing in the component's source decides this — see [Which tenant your component is published for](#user-content-which-tenant-your-component-is-published-for). Agree it up front, because it is the one thing that cannot be checked by looking at the code |
| Which environment: development, staging or production? | which `appsettings` file the publish uses, and therefore which `smint-io-pc` script you run. Each environment has its own tenant URL, so confirm the tenant per environment |

If the component should be available more widely than a single tenant, that is something we
arrange on our side — please get in touch at [support@smint.io](mailto:support@smint.io) rather
than trying to configure it in the component.

### Getting started

Please note that access to the SDKs is restricted. Get in contact with [Smint.io](https://www.smint.io)
and request access. Access will be granted to either Smint.io Solution Partners or to all our Smint.io Portals
Enterprise plan customers.

You will need an account with Microsoft Visual Studio cloud offerings (Azure DevOps), as the SDKs are hosted there.

Steps to follow:

1. Navigate to a folder where the components should be physically present. We will call it the root folder
2. Copy the `ui-example-hello-world-1` directory from this repository to the root folder as a starting point
3. Rename the `ui-example-hello-world-1` directory to your desired frontend component name

	- Start the directory name with `ui-` for UI components, and `page-` for page templates
	- End the directory name with `-1` so that later on, if desired, you can potentially create different variations of the frontend component

4. Edit the `package.json` file in the new directory, and change `name`, `description`, `version` and `author` of your new frontend component

	- Prefix the package `name` with your partner ID e.g. `@smintio/`
	- Then simply add the directory name from the previous step as the package name

5. Make sure you use node version 12.22.10 (use [NVM](https://github.com/nvm-sh/nvm) if you use different node versions)
6. Make sure that your component .npmrc file contains the proper reference to our NPM SDK repo:

```
@smintio:registry=https://smintio.pkgs.visualstudio.com/_packaging/Portals-Components-Public/npm/registry/
always-auth=true
```

7. Authorize your NPM for use of our `Portals-Components-Public` NPM SDK repo. You will find more info by accessing the [Azure DevOps location of our NPM SDK  repo](https://smintio.visualstudio.com/SmintIo-UIComponents/_artifacts/feed/Portals-Components-Public) and clicking `Connect to feed` -> `npm` -> select `Windows` or `Other` for instructions)
8. If applicable, make sure that your component .npmrc file contains the proper reference to your partner NPM repo (replace `partner-id` by your partner ID):

```
@[partner-id]:registry=https://smintio.pkgs.visualstudio.com/_packaging/Portals-Components-Partners-[partner-id]/npm/registry/ 
always-auth=true
```

9. If applicable, authorize your NPM for use of your partner NPM repo. You will find more info by accessing the Azure DevOps location of your partner repo (https://smintio.visualstudio.com/SmintIo-UIComponents/_artifacts/feed/Portals-Components-Partners-[partner-id]) and by then clicking `Connect to feed` -> `npm` -> select `Windows` or `Other` for instructions)

10. Run `npm i` at the first time, or when you update dependencies

	- If there is any authorization issues you are running into, you will have done something wrong in step 6-9. Please revisit your settings
	- If you absolutely cannot manage to get going, please get in touch at [support@smint.io](mailto:support@smint.io)
	
11. Please adjust `src/PortalsUiComponent.vue` accordingly

### Things to do for Mac or Linux users

The `package.json` scripts you find in the example component are tuned at Windows users.

For Mac or Linux users, you have to change your `package.json`. Please change the `smint-io-pc` script parts as follows (remove .exe extension of the executable file, fix the environment variable reference from `%SMINT_IO_SDK_HOME%` to `$SMINT_IO_SDK_HOME`, and fix the path separator):

```
"smint-io-pc:development": "npm publish && npm info --json | $SMINT_IO_SDK_HOME/SmintIo.Portals.SDK.PublishComponent.CLI -env development",
"smint-io-pc": "npm publish && npm info --json | $SMINT_IO_SDK_HOME/SmintIo.Portals.SDK.PublishComponent.CLI -env staging",
"smint-io-pc:production": "npm publish && npm info --json | $SMINT_IO_SDK_HOME/SmintIo.Portals.SDK.PublishComponent.CLI -env production"
```

### The example frontend component

The [example Vue.js component itself](ui-example-hello-world-1//src//PortalsUiComponent.vue) is pretty basic. 
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

```javascript
    @DisplayName("en", "Component text", true)
    @DisplayName("de", "Komponententext")
    @Description("en", "The component text.", true)
    @Description("de", "Der Komponententext.")
    @Implements("ILocalizedStringsModel")
    @ComponentProperty({ name: "componentText" })
    @DynamicAllowedValuesProvider(
        PortalsGlobalServices.PortalsContext.toString(),
        "StringResourceAllowedValuesProvider"
    )
    @FormGroup("hw-text")
    public readonly componentText!: ILocalizedStringsModel;

    @DisplayName("en", "Color", true)
    @DisplayName("de", "Farbe")
    @Description("en", "The component text color.", true)
    @Description("de", "Die Textfarbe der Komponente.")
    @ComponentProperty({ name: "componentColor" })
    @IsColor()
    @FormGroup("hw-color")
    public readonly componentColor!: string;
```

Additional attributes such as `DynamicAllowedValuesProvider` or `IsColor` control how the component configuration can look.
In this case, an example would be a text input field or a color picker.

The full list of supported Smint.io Portals annotations can be found [here](docs/smintio-annotations.md).

`ILocalizedStringsModel` is a custom object type defined by Smint.io that can return the correct text value of a component according to the selected language by the user.

#### Shipping your own string resources

A text property can be given a **string resource id** as its default, so the component is not
blank the moment it is dropped onto a page. The example does this in
[resources/definition.ts](ui-example-hello-world-1/resources/definition.ts), pointing
`componentText` at the resource id `component_text`:

```javascript
uiComponentBuilder.setFormFieldValues({
    values: [
        {
            id: "componentText",
            dataType: "resource_id",
            resourceIdValue: "component_text",
        },
    ],
} as IFormFieldValuesModel);
```

The id has to resolve to a resource that actually exists, and there are two sources:

- **resources that already exist in the portal**, maintained on the Smint.io side. You can
  reference these but you cannot add to them.
- **resources your component ships itself.** This is the route for any text of your own, and
  it is easy to miss — your `resources` folder looks empty because only *file* resources
  (images, videos, audio, documents) live there as files.

Declare one with `addEmbeddedResource`, inside `defineResources` and **before** the
`loadFileResources` call:

```javascript
import type { IResourceDefinitionBuilder } from "@smintio/portals-resource-builder-cli";
import { IEmbeddedResourceModel, IFormFieldValuesModel } from "@smintio/portals-resource-builder-cli";

// ...
await uiComponentBuilder.defineResources(async (resourceBuilder) => {
    resourceBuilder.addEmbeddedResource({
        id: "text_hello_world_greeting",
        resource: "string",
        name: {
            "x-default": "Greeting text",          // the label shown in the page editor
        },
        formFieldValues: {
            values: [
                {
                    id: "Text",                     // always "Text" for a string resource
                    dataType: "localized_strings_model",
                    localizedStringsModelValue: {
                        "x-default": "Hello world", // the text itself
                        de: "Hallo Welt",           // one key per further language
                    },
                },
            ],
        } as IFormFieldValuesModel,
        localizedResourceAssets: undefined,         // required, and always undefined here
    } as IEmbeddedResourceModel);

    await resourceBuilder.loadFileResources(async (fileResources) => {
        await fileResources.verifyLoadedResources(async (resource) => {
            return resource;
        });
    });
});
```

You can then reference it from the property itself:

```javascript
    @Implements("ILocalizedStringsModel")
    @ComponentProperty({ name: "componentText" })
    @DefaultValue("text_hello_world_greeting")
    public readonly componentText!: ILocalizedStringsModel;
```

Things worth knowing before you write one:

| | |
|---|---|
| `IEmbeddedResourceModel` and `IFormFieldValuesModel` | import them as values, **not** with `import type` — the `as` casts need them at runtime |
| `resource` | `string` for a **plain text** — a label, a button caption, a heading. `text` for a **rich text** — long form body copy carrying markup, the kind you pair with `IsRichText` and render with `v-html`. `image`, `video`, `audio` and `document` are file based and come from your `resources` folder instead |
| matching the provider | the dropdown an editor sees comes from the **property**, not from the resource. `StringResourceAllowedValuesProvider` lists string resources, `TextResourceAllowedValuesProvider` lists rich text ones. If they do not match, the resource you shipped will not appear in the list |
| `name` versus the value | `name` is the label an editor sees when picking a resource from a list. The text itself is the `formFieldValues` entry with `id: "Text"` |
| id naming | prefix with `text_` and keep it unique to your component, for example `text_<yourcompany>_<what>` |
| languages | `"x-default"` is the fallback, then one key per further language |
| `DefaultValue` versus `setFormFieldValues` | `DefaultValue` is the property's default and applies wherever the component is used. `setFormFieldValues` pre-fills the field the moment an editor drops the component onto a page |

Adding or changing a resource is an **annotation level change** — see the table under
[What the dev server covers, and what still needs publishing](#what-the-dev-server-covers-and-what-still-needs-publishing).
It will not show up in the page editor until you publish your component.

Page templates ship string resources in exactly the same way, through
`buildPageTemplateResourceDefinition`.

By default, [package.json](ui-example-hello-world-1/package.json) is used by the npm CLI (and others) to identify the component and how to handle its relevant dependencies.

# Crafting a page template instead

A page template is the same kind of npm package, built with the same SDK. Start from the same
`ui-example-hello-world-1` directory and change the following:

| | UI component | Page template |
|---|---|---|
| Directory name prefix | `ui-` | `page-` |
| Source file | `src/PortalsUiComponent.vue` | `src/PortalsPage.vue` |
| Class decorator | `@PortalsUiComponent` | `@PortalsPageTemplateComponent` |
| `type` | a UI component type | a page type |
| Shared rollup config imported by your `rollup.config.js` | `../config/rollup/rollup-config.ts` | `../config/rollup/rollup-config-page.ts` |
| `main` in `package.json` | `./lib/portals-ui-component.umd.min.js` | `./lib/portals-page.umd.js` |
| Resource builder call in `resources/definition.ts` | `buildUIComponentResourceDefinition` | `buildPageTemplateResourceDefinition` |
| Generated resource file | `portals-ui-component.json` | `portals-page-template.json` |

The shared rollup config for page templates is already part of this repository, so the only
change to your `rollup.config.js` is the import:

```javascript
import rollupConfig from "../config/rollup/rollup-config-page.ts";
```

Instead of configuration properties alone, a page template declares its *slots*:

```javascript
import type { IUIComponentInfo } from "@smintio/portals-component-sdk";
import { UIComponentSlot } from "@smintio/portals-component-sdk";

@UIComponentSlot({
    slotId: "header",
    minimumItems: 0,
    maximumItems: 1,
    allowedUiComponentTypes: ["ui-type-header"],
})
public headerSlot: IUIComponentInfo[] = [];

@UIComponentSlot({ slotId: "content", minimumItems: 1 })
public contentSlot: IUIComponentInfo[] = [];
```

and renders them with the slot components from `@smintio/portals-components`:

```vue
<s-header-slot :ui-slot="headerSlot" />

<v-main>
    <s-generic-slot
        :ui-slot="contentSlot"
        :content-gap="contentGap"
        :apply-top-content-gap="true"
        :apply-bottom-content-gap="true"
    />
</v-main>
```

`@UIComponentSlot` does not create an ordinary data property — it replaces the field with a computed property that
reads the slot's components from the page context, so declare it with an `= []` initializer and never assign to it.

Remember that the page template — not the UI component — owns all padding and margins between the components it hosts.

To hand data and event handlers to the components in a slot, use `ui-slot-data`, a Vue render data object:

```vue
<s-generic-slot
    :ui-slot="searchResultSlot"
    :ui-slot-data="{
        props: { currentSearch, results, isSearching },
        on: { 'next-search-page': onNextSearchPage },
    }"
/>
```

This is how a page fulfils the contract that the UI components of its page type expect.

# Building a section component

A *section* is the one sanctioned way for a UI component to wrap other UI components. The
portal editor adds your *section start* component, then any number of components, and finally
the generic *section end* component `smintio-ui-generic-section-end-1`. The runtime collects
everything in between and hands it to your component.

Declare your component with `type: "ui-type-section"`, and receive the collected components in
a plain Vue prop named `subUiComponentInfos`:

```javascript
import type { IUIComponentInfo } from "@smintio/portals-component-sdk";
import { Prop } from "vue-property-decorator";
import { SGenericSlot } from "@smintio/portals-components";

@PortalsUiComponent({
    type: "ui-type-section",
    key: "smintio-ui-example-section-start-1",
    displayName: { [DefaultCulture]: "Start example section" },
    components: {
        SGenericSlot,
    },
    inheritAttrs: false,
})
export default class PortalsUiComponentImplementation extends Mixins(SHtmlProps) {
    @Prop({ default: [] })
    public readonly subUiComponentInfos!: IUIComponentInfo[];
}
```

Then render them wherever your layout wants them, with the same `s-generic-slot` renderer a
page template uses. Pass `$attrs` and `$listeners` through, so that the components inside your
section still receive whatever the page handed to the section as a whole:

```vue
<s-generic-slot
    :ui-slot="subUiComponentInfos"
    :ui-slot-data="{
        attrs: $attrs,
        on: $listeners,
    }"
/>
```

Sections may be nested — the runtime tracks the nesting level and matches each section start
to its own section end. Because your component decides where the collected components go, you
can build quite sophisticated constructs this way: our own
`smintio-ui-generic-section-start-table-1` distributes them over the cells of a responsive
table, and there are further variants for expansion panels, tab panels and conditional
display.

Please remember that a section start component still follows all the usual UI component rules
— in particular, it must fill its layout box and add no white space of its own.

### Build your custom frontend component

Please note that access to the SDKs is restricted. Get in contact with [Smint.io](https://www.smint.io)
and request access. Access will be granted to either Smint.io Solution Partners or to all our Smint.io Portals
Enterprise plan customers.

You will need an account with Microsoft Visual Studio cloud offerings (Azure DevOps), as the SDKs are hosted there.

1. In the component folder open a command prompt or terminal window
2. Make sure you use node version 12.22.10 (use [NVM](https://github.com/nvm-sh/nvm) if you use different node versions)
3. Make sure that your component .npmrc file contains the proper references to our NPM SDK repo:

```
@smintio:registry=https://smintio.pkgs.visualstudio.com/_packaging/Portals-Components-Public/npm/registry/
always-auth=true
```

4. Authorize your NPM for use of our `Portals-Components-Public` NPM SDK repo. You will find more info by accessing the [Azure DevOps location of our NPM SDK repo](https://smintio.visualstudio.com/SmintIo-UIComponents/_artifacts/feed/Portals-Components-Public) and by then clicking `Connect to feed` -> `npm` -> select `Windows` or `Other` for instructions)
5. If applicable, make sure that your component .npmrc file contains the proper reference to your partner NPM repo (replace `partner-id` by your partner ID):

```
@[partner-id]:registry=https://smintio.pkgs.visualstudio.com/_packaging/Portals-Components-Partners-[partner-id]/npm/registry/ 
always-auth=true
```

6. If applicable, authorize your NPM for use of your partner NPM repo. You will find more info by accessing the Azure DevOps location of your partner repo (https://smintio.visualstudio.com/SmintIo-UIComponents/_artifacts/feed/Portals-Components-Partners-[partner-id]) and by then clicking `Connect to feed` -> `npm` -> select `Windows` or `Other` for instructions)
7. Run `npm i` at the first time, or when you update dependencies

	- If there is any authorization issues you are running into, you will have done something wrong in step 3-6. Please revisit your settings
	- If you absolutely cannot manage to get going, please get in touch at [support@smint.io](mailto:support@smint.io)
	
8. Run `npm run build` or `npm run watch` to build your frontend component

`npm run build` runs two steps: `build:dist` (rollup, producing the bundle under `lib/`) and `build:resources`
(the `portals-resource-builder-cli`, producing `portals-ui-component.json` or `portals-page-template.json` from
`resources/definition.ts`).

`npm run watch` runs **rollup only**. It does not regenerate the resource definition file. After changing
`resources/definition.ts`, run `npm run build:resources` or a full `npm run build`.

Never edit `portals-ui-component.json` or `portals-page-template.json` by hand — they are build output and are
overwritten on the next build.

#### If you also have a newer node installed

`npm run build` runs the shims in `node_modules/.bin`, and those pick whichever `node` comes
first on your `PATH` — not necessarily the version you selected with NVM. On a machine that
also has a modern node, rollup then fails with:

```
Error loading `tslib` helper library.
[!] Error: Package subpath './package.json' is not defined by "exports" in .../tslib/package.json
```

The fix is to run the two build tools with the node 12 binary directly, from your component
folder:

```
<path-to-node-12>/node.exe node_modules/rollup/dist/bin/rollup -c --environment BUILD:production
<path-to-node-12>/node.exe node_modules/@smintio/portals-resource-builder-cli/bin/resource-builder-cli.js
```

Those two commands are exactly what `build:dist` and `build:resources` do.

### Publish your custom frontend component

1. In the component folder open a command prompt or terminal window
1. Run `npm run build` to ensure the latest version will be published
1. Run `npm run smint-io-pc` to publish to the Smint.io `Staging` environment

	- If the component already exists in the npm registry the component can be deployed by running `npm info --json | %SMINT_IO_SDK_HOME%\\SmintIo.Portals.SDK.PublishComponent.CLI.exe -env staging` (for Windows users) or `npm info --json | $SMINT_IO_SDK_HOME/SmintIo.Portals.SDK.PublishComponent.CLI -env staging` (for Mac or Linux users)

1. If you run into any issues in running the command, it is likely one of the following causes:

	- You did not properly provide the SMINT_IO_SDK_HOME environment variable
	- You did not adjust the package.json scripts to handle Mac or Linux - note the default scripts are tuned for Windows users
	- You did not use the proper tool builds for your operating system
	- There is authorization issues with your NPM setup (see previous chapter for more instructions)
	- If you absolutely cannot manage to get going, please get in touch at [support@smint.io](mailto:support@smint.io)
	
1. A browser window will open for you to authenticate to allow the component to be published
    - If successful, the window will close after a few seconds
    - The message *You can now return to the application.* will also indicate success
1. Go back to the command prompt or terminal window and validate that component is published using the correct version
1. `Done` will indicate that the publication was successful

Running `npm run smint-io-pc` will package the component in the defined npm registry and then trigger a REST API request to register the package with Smint.io.

Please note that calling the command repeatedly with the same package version will result in an error.

With each code change, the version number must be increased in the `package.json` file.

#### Which tenant your component is published for

Nothing in your component's source decides this, and there is nothing you need to add to it.
The tenant comes from the configuration of the *Portals-SDK-PublishComponent-CLI tool*.

Next to the CLI executable sits an `appsettings.json` as a template, plus one file per
environment — `appsettings.Development.json`, `appsettings.Staging.json` and
`appsettings.Production.json`. The `-env` argument in the `smint-io-pc` scripts chooses which
one is used. Each file holds:

| Setting | What it does |
|---|---|
| `SmintIo.ApiUrl` | the **tenanted** Smint.io API URL the component is registered with — this is what decides which tenant receives it |
| `SmintIo.Auth.Authority`, `ClientId`, `ClientSecret` | the OAuth client the publish authenticates with |
| `SmintIo.AuthorizationHeader` | the authorization header for your npm repository, where one is required |
| `RedirectUrl` | the local callback the authentication returns to — this is the browser window that opens during the publish and closes itself again |

**Your component is published for your own tenant only.** It becomes available in the page
editor of that tenant's portals, and nowhere else. This is exactly what you want for a
component you built for one customer or one portal: there is no additional step needed to keep
it private, and no setting in the component to get wrong.

If a component of yours should become available more widely, please get in touch at
[support@smint.io](mailto:support@smint.io) — that is something we arrange on our side.

> Please treat these settings files as secrets. They contain an OAuth client secret and your
> npm repository authorization header. Do not commit them, and do not paste their contents into
> a support request — tell us what you see, not what is in the file.

More information about the *Portals-SDK-PublishComponent-CLI tool* can be found [here](../../Tools/Portals-SDK-PublishComponent-CLI/Release/).

### Local development

This is the part that makes developing Smint.io Portals frontend components pleasant: the dev
server reroutes the requests that load your component's JavaScript to the files in your own
working folder. Your local build then runs inside a **real portal**, against real data.

#### What the dev server covers, and what still needs publishing

Please read this before you start, it saves a lot of confusion:

| What you changed | What is needed |
|---|---|
| The template, the script body, the styles — your component's **HTML and JavaScript** | Rebuild and reload the browser. The dev server serves your local files. **No publishing.** |
| **Annotations** — a new or renamed configuration property, a changed `DisplayName`, `Description`, `DefaultValue`, `AllowedValues`, `VisibleIf` or `FormGroup`, the component metadata, or a page template's slots | **Publish your component.** The configuration form and the component metadata live on the Smint.io server, not in your bundle. Until you publish, the portal editor will not offer the new setting, nor show a changed label. |
| A **brand new component** | **Publish your component**, and add it to the dev server's `ComponentMappings` so the dev server can find your local build at all. |

So: publish once when you create the component and whenever you change its settings, then
iterate freely on markup and behaviour without publishing. When you are finished, publish once
more, so that what is registered with Smint.io matches your final result.

1. Get in touch with [support@smint.io](mailto:support@smint.io) so that we can set up a development portal for you
1. Download the [Portals Dev-Server](../../Tools/Portals-DevServer/Release/) and install the .NET 8 runtime
1. Trust the `rootCA.pem` shipped with the dev server, so that your browser accepts its local HTTPS listener
1. In the dev server's `appsettings.json`, set `RootDirectory` to the folder that contains your component folders
1. In the same file, add one entry under `ComponentMappings` for each component you want to
   develop locally, mapping the component id to its built bundle, relative to `RootDirectory`:

```
"ComponentMappings": {
    "ui-example-hello-world-1": "ui-example-hello-world-1/lib/portals-ui-component.umd.min.js",
    "page-example-hello-world-1": "page-example-hello-world-1/lib/portals-page.umd.js"
}
```

   The path must match the `main` entry of your component's `package.json`. Please note that
   the dev server reads `appsettings.json` when it starts, so restart it after you add a mapping.

1. Start the dev server — `SmintIo.Portals.DevServer.exe` on Windows, `dotnet Portals-DevServer.dll` on Mac or Linux.
   It listens on `https://development-host.smint.io:8000` and logs every request it serves
1. Build and publish your component once, so that Smint.io knows it and the portal editor can offer it
1. Run `npm run watch` for continuous building of your frontend component
1. Turn off your browser's cache
1. Navigate to your development portal, add your frontend component to a page
1. Change some code in your frontend component, and refresh the browser page. Enjoy! :)
1. Whenever you change an annotation, build and publish again before you continue
1. When you are finished, build and publish the final result

If your component does not show up, the dev server's request log is the first place to look:
it will show whether the request was rerouted to your file or served from the published
package. The usual causes are a mapping that does not match, a bundle that has not been built
yet, or a dev server that was not restarted after the mapping was added.

If instead your component shows up but a *setting* you added is missing from the configuration
form, that is the publishing boundary described above — build and publish, and it will appear.

### Common problems

| What you see | What it usually means |
|---|---|
| `Some properties lack type definition: [...]` when the component loads | one of your configuration properties has no `Is...` or `Implements` annotation |
| A property you added does not appear in the configuration form | it has no `FormGroup`, or its `VisibleIf` condition is never true |
| Settings that portals had already saved are suddenly empty | the TypeScript property was renamed without keeping the same `ComponentProperty` name |
| `portals-ui-component.json` or `portals-page-template.json` is out of date | `npm run watch` does not run the resource builder — run `npm run build:resources` |
| Your local change does not show up in the portal | the dev server mapping is missing or points at the wrong bundle path, the dev server was not restarted after the mapping was added, or the browser cache is on |
| A setting you added is not in the configuration form, or your new component is not offered in the editor | the component has not been published since you changed its annotations — the configuration form lives on the Smint.io server, not in your bundle |
| Publishing fails right away | the `version` in `package.json` was not increased, or `SMINT_IO_SDK_HOME` is not set |
| Your component does not appear in the page editor of the portal you expected | it was published against a different `SmintIo.ApiUrl` — the tenant comes from the CLI's `appsettings.<Env>.json`, not from the component |
| Unexpected rollup or babel errors when building | the wrong node version — see the build chapter above |
| Two identical `CSS class` fields in the configuration form | `SCssProps` and `SHtmlProps` were mixed in together; use only one of them |

## Extras for Smint.io Certified partners

If you are one of our `Smint.io Certified` partners, you will also get access to the source code of our own Smint.io Portals components. You can then check how we did things, and you can use our code
as a reference for you to solve your requirements. In this case, we recommend you to check out that source code from [here](https://smintio.visualstudio.com/SmintIo-UIComponents/_git/SmintIo-UIComponents).

1. Check out the `page-templates` directory to find the implementations of our page templates

	- Here you can find a list of our page template components (not up-to-date, but helpful): https://github.com/smintio/Portals-Components-SDK/blob/main/Examples/Frontend/docs/smintio-page-templates.md
	
2. Check out the `ui-components` directory to find the implementations of our UI components

	- Here you can find a list of our UI components (not up-to-date, but helpful): https://github.com/smintio/Portals-Components-SDK/blob/main/Examples/Frontend/docs/smintio-ui-components.md
	
3. Check out the `portals-components` directory to find the implementation of our shared component library
4. Please do not forget to pull this repository regularily, as we constantly update our codebase

If you have any questions about the code, on how to build those components, or whatever else, please do not hesitate to get in touch at [support@smint.io](mailto:support@smint.io)!

## Problems

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yanko Belov, Smint.io GmbH
- Yosif Velev, Smint.io GmbH