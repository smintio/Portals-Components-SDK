Smint.io Portals portal templates and resources
==============================================

Current version of this document is: 1.1.0 (as of 15th of September, 2026)

Two component types that sit at the top of the templating stack. A **portal template** describes
a whole portal, so that a new one can be created from it. A **resource** is a typed piece of
content or styling — a string, a rich text, an image, a menu item, an email, a font, a style —
that a portal holds once and many components refer to.

They are documented together because they are used together: a portal template ships the
resources a portal starts with, and a resource is the thing a component configuration points at
instead of duplicating text.

The templating stack, bottom to top: page types → page templates → **portal templates**, with
**resources** supplying the content and styling that the pages render. The first two are
frontend components — see
[developing frontend components](../../Frontend/Legacy/README.md).

1. [Portal templates](#user-content-portal-templates)
1. [The portal template contract](#user-content-the-portal-template-contract)
1. [The two ways a portal template is built](#user-content-the-two-ways-a-portal-template-is-built)
1. [`PortalTemplateConfiguration`](#user-content-portaltemplateconfiguration)
1. [Resources](#user-content-resources)
1. [The resource contract](#user-content-the-resource-contract)
1. [Resource assets](#user-content-resource-assets)
1. [Referring to a resource from a component](#user-content-referring-to-a-resource-from-a-component)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## Portal templates

A portal template is what an administrator picks when creating a new portal. It is a **backend**
component even though everything it produces is visible in the frontend, because the code that
instantiates a portal runs on the server.

It renders nothing itself. It is used exactly once per portal — at creation — and after that the
portal is an ordinary portal that the administrator edits freely. A change to the template does
not reach portals already created from it.

## The portal template contract

```C#
namespace SmintIo.Portals.PortalTemplateSDK.PortalTemplates
{
    public interface IPortalTemplate : IPortalTemplateComponent
    {
        PortalDefaultSettingsModel GetDefaultSettings();
        ResourceModel[] GetResources();
        PageModel[] GetPages();
        UIComponentModel[] GetUIComponents(string pageId, string slotId);
    }

    public interface IPortalTemplateStartup : IComponentStartup
    {
        string Type { get; }                        // a PortalType constant
        string[] ScreenshotUrls { get; }
        bool IsDeprecated { get; }
        long? LivePortalUuidProduction { get; }
        long? LivePortalUuidStaging { get; }
        long? LivePortalUuidDevelopment { get; }
        bool IsSmintedUi { get; }
    }
}
```

| Startup member | Set it to |
|---|---|
| `Type` | one of `PortalType.MediaGallery` (`media_gallery`), `PortalType.BrandPortal` (`brand_portal`), `PortalType.PressPortal` (`press_portal`), `PortalType.Empty` (`empty`). This is what the administrator is choosing between |
| `ScreenshotUrls` | screenshots for the template picker. Recommended size 1100×700. This is the entire basis on which the template is chosen — supply them |
| `IsDeprecated` | `true` hides the template from the picker without breaking portals already created from it. **This is how a template is retired**; never delete one |
| `LivePortalUuid…` | see below |
| `IsSmintedUi` | the template targets the Sminted UI frontend runtime rather than the current one |

`PageModel` describes one page: `Id`, `Name`, the `PageTemplateType` and `PageTemplate` to use,
`DefaultSettings`, `FormFieldValues`, and `IsEntryPoint` for the portal's landing page.
`ResourceModel` describes one resource the portal starts with: `Id`, the `Resource` component
key, `Name`, `LocalizedResourceAssets` and `FormFieldValues`.

## The two ways a portal template is built

This is the part that surprises people, so it is worth being direct about it.

**In practice, portal templates are built by cloning a live portal.** The startup points at an
existing, fully configured portal by its uuid — one per environment — and creating a portal from
the template copies that portal's configuration. `BaseLivePortalTemplate`
(`…PortalTemplates.Prefab`) implements all four `IPortalTemplate` methods as `return null`, and
the template class derives from it and adds nothing:

```C#
public class MyPortalTemplate : BaseLivePortalTemplate
{
}
```

with all the substance on the startup. Every portal template Smint.io ships works this way. The
reason is practical: a portal is composed in the WYSIWYG editor, and describing the result in
C# — every page, every slot, every component's configuration — would be a large amount of code
that has to be kept in step with a thing that is far easier to edit visually.

**The declarative path is real but dormant.** `GetPages`, `GetResources`, `GetUIComponents` and
`GetDefaultSettings` are called and honoured; nothing shipped uses them. If you implement them,
note that an unresolvable page template reference or UI component type reference fails the
instantiation — the template is validated against what is actually registered.

Two consequences for a partner:

- **Do not start by implementing the four methods.** Read them as an advanced option. Build the
  portal you want, then ask Smint.io to make it a template.
- **The live portal uuids are per environment and come from Smint.io.** They identify portals on
  the Smint.io side; you cannot derive them, and a uuid from one environment is meaningless in
  another. Get in touch at [support@smint.io](mailto:support@smint.io).

## `PortalTemplateConfiguration`

`SmintIo.Portals.PortalTemplateSDK.PortalTemplates.PortalTemplateConfiguration` is the base
configuration most templates reuse or extend. It covers the settings every portal needs at
creation, so that you do not redeclare them:

| Area | What it holds |
|---|---|
| Branding | logo, favicon and apple-touch-icon resource references |
| Email sending | whether mail is sent by Smint.io, through the customer's domain with SPF and DKIM, or through the customer's own SMTP server — with the host, ports, security type and credentials for the last |
| Transactional emails | resource references for the portal's emails: invitations, download-available notices, access and permission requests, and the rest |
| Analytics | the tracking mode, and the measurement identifiers where one applies |

Extend it rather than replacing it when your template needs settings of its own.

## Resources

A resource is a piece of portal content that exists **once per portal** and is referred to by
many components. The point is reuse: a button caption, a legal text, a logo or a colour scheme
that appears on twenty pages is defined once and changed once.

> **Four things in this documentation are called "resource". They are not the same.**
>
> | | What it is |
> |---|---|
> | **`Resource`** — the component type | what this section describes: a typed piece of portal content an administrator creates and components point at |
> | **resource asset** | a `Resource` that is *also* published as an asset, so it appears in searches — see [resource assets](#user-content-resource-assets) |
> | **`IResources…`** | a [data adapter public API interface family](smintio-data-adapter-interfaces.md#user-content-collections-shares-and-the-rest) for reading and searching those resources |
> | **`ConfigurationMessages.resx` / `MetamodelMessages.resx`** | .NET *resource files* — the translated strings your own component's code ships with. Nothing to do with portal content; see [translating with resource files](smintio-backend-annotations.md#user-content-translating-with-resource-files) |
>
> Separately, `IResourceAssets…` is a third interface family, for reading resource assets through
> the asset interfaces. If you are building a UI component, the portal resources you point a
> property at are these same `Resource` components.

`ResourceType` constants (`…ResourceSDK.Resources.Models`):

| Constant | Value | For |
|---|---|---|
| `String` | `string` | a short plain text — a label, a button caption, a heading |
| `Text` | `text` | a rich text |
| `MenuItem` | `menu_item` | a navigation entry, which may point at a portal page |
| `Image`, `Video` | `image`, `video` | media |
| `Email` | `email` | a transactional email |
| `Category` | `category` | a category |
| `Font` | `font` | a font |
| `Style` | `style` | a style |

`ResourceKey` additionally names the well-known singleton slots a portal template refers to —
the logo, the favicon, the apple touch icon, the imprint, the privacy policy, the terms and
conditions, and the portal's transactional emails.

**The distinction between a `String` resource and an ordinary localized text property matters.**
A text used only by one component is a property on that component. A text that several
components must show identically is a resource. Reaching for a resource by default produces a
portal whose content is scattered across resource definitions nobody can find.

## The resource contract

```C#
namespace SmintIo.Portals.ResourceSDK.Resources
{
    public interface IResource : IResourceComponent { }      // no members

    public interface IResourceStartup : IComponentStartup
    {
        string MdiIcon { get; }
        string[] AllowedPortalTypes { get; }
        bool IsResourceAsset { get; }
    }
}
```

`IResource` declares **nothing**, and that is not an oversight: a resource has no behaviour. It
is typed configuration. Consequently `ComponentImplementation` on the startup is routinely
`null`, and the resource is entirely described by its `ConfigurationImplementation` — the fields
the administrator fills in — and its `ConfigurationMessages`.

If you are looking for a hook where a resource is processed or rendered, there is none. The
component that *uses* the resource renders it.

| Startup member | Set it to |
|---|---|
| `MdiIcon` | the icon in the resource picker |
| `AllowedPortalTypes` | restrict the resource to certain portal types, or `null` for no restriction |
| `IsResourceAsset` | see below |

## Resource assets

A resource can additionally be published as an **asset**, so that it appears in searches and can
be shown by the ordinary asset components rather than by a component written for it. A press
release, a content kit or a curated item is a resource that is also an asset.

Derive the startup from `ResourceAssetStartup` (`…Resources.Prefab`), which sets
`IsResourceAsset => true` and adds two abstract members:

```C#
public abstract ResourceAssetMetamodel GetResourceAssetMetamodel();

public abstract Task FillAssetDataObjectAsync(
    AssetDataObject assetDataObject,
    IComponentConfiguration componentConfiguration,
    IPageReferenceResolver pageReferenceResolver,
    IEntityModelProvider entityModelProvider);
```

`GetResourceAssetMetamodel` returns a `ResourceAssetMetamodel` — a `ConnectorMetamodel` with a
version — describing the resource's own fields, exactly as a connector describes an external
system's schema. Read [the connector meta-model](smintio-connector-metamodel.md); everything
there applies, and the version is what tells the platform the schema has changed.

`FillAssetDataObjectAsync` turns one configured resource into an `AssetDataObject`. The rules
are the ones in [the asset data model](smintio-asset-data-model.md): set a `contentType`, fill
the matching metadata object, set the availability flags rather than the URLs. The
`IPageReferenceResolver` you are handed resolves a configured page reference into something the
asset can carry, which is how a resource asset links to a portal page.

**Bump the version whenever the meta-model changes.** That is what `ResourceAssetMetamodel`'s
version argument is for, and a schema change without one leaves the old schema in place.

## Referring to a resource from a component

From a component's configuration, declare a property of type `IResourceReference` (or
`IResourceReference[]`) and the administrator gets a resource picker:

```C#
[DisplayName("en", "Header image", IsDefault = true)]
[FormGroup("appearance")]
[DynamicAllowedValuesProvider(typeof(ImageResourceAllowedValuesProvider))]
public IResourceReference HeaderImage { get; set; }
```

The provider narrows the picker to one resource type. There is one per type —
`StringResourceAllowedValuesProvider`, `TextResourceAllowedValuesProvider`,
`ImageResourceAllowedValuesProvider`, `VideoResourceAllowedValuesProvider`,
`MenuItemResourceAllowedValuesProvider`, `EmailResourceAllowedValuesProvider`,
`CategoryResourceAllowedValuesProvider`, `FontResourceAllowedValuesProvider`,
`StyleResourceAllowedValuesProvider` — all in
`SmintIo.Portals.ResourceSDK.Resources.AllowedValuesProviders`.

`IResourceReference` gives you the resolved `ResourceDataObject` and its `ResourceIdentifier`.

**Pick the provider that matches what you will do with the value.** A property that renders rich
text should use the text provider, not the string one; a property that renders a plain label
should use the string one. Mismatching them offers the administrator resources that will not
render correctly.

## Things that are easy to get wrong

- **Do not implement `GetPages`/`GetResources`/`GetUIComponents` as a first move.** Portal
  templates are built by cloning a configured live portal; the declarative API is an advanced
  alternative nothing currently uses.
- **Retire a portal template with `IsDeprecated`, never by deleting it.** Portals created from it
  keep working, but the template must stay registered.
- **Supply `ScreenshotUrls`.** A template with no screenshots is a template nobody picks.
- **A resource has no behaviour.** `IResource` is empty and `ComponentImplementation` is normally
  `null`. Stop looking for the processing hook.
- **Resource, or plain property?** Resource when several components must show the same text;
  property when one component owns it.
- **Check what an existing resource actually renders before defaulting a property to it.** The id
  reads like a label, but the wording is whatever that portal defined, and it was usually written
  for the component that introduced it.
- **Version a resource asset meta-model when you change it.**
- **Match the allowed-values provider to the resource type** your property expects.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
