Smint.io Portals backend component annotations
=============================================

Current version of this document is: 1.6.0 (as of 15th of September, 2026)

Annotations describe a backend component's configuration to Smint.io Portals: what fields the
portal administrator sees when configuring your connector, data adapter, data processor,
identity provider, portal template or resource, what they are called in each language, what
values they accept and when they are shown.

Every annotation listed here lives in `SmintIo.Portals.SDK.Core.Configuration.Annotations` (the
validation ones in the nested `.Validation` namespace) and is applied to properties of a class
implementing `IComponentConfiguration`.

This is the backend counterpart of
[the frontend component annotations](../../Frontend/Legacy/docs/smintio-annotations.md). The two
sets overlap deliberately — the same configuration form is produced either way — but they are
not identical, and the differences are the first thing to read if you know one and are now
writing the other.

Please get in touch at [support@smint.io](mailto:support@smint.io) if you are missing an
annotation, or if you need a new one.

1. [How a backend configuration differs from a frontend one](#user-content-how-a-backend-configuration-differs-from-a-frontend-one)
1. [Which component's configuration a setting belongs in](#user-content-which-components-configuration-a-setting-belongs-in)
1. [Configuration is stored encrypted](#user-content-configuration-is-stored-encrypted)
1. [The property type is the data type](#user-content-the-property-type-is-the-data-type)
1. [Labels and help text](#user-content-labels-and-help-text)
1. [Translating with resource files](#user-content-translating-with-resource-files)
1. [How a resource key has to be named](#user-content-how-a-resource-key-has-to-be-named)
1. [Three ways to turn a resource key into a localized string](#user-content-three-ways-to-turn-a-resource-key-into-a-localized-string)
1. [Values and validation](#user-content-values-and-validation)
1. [Editor hints on string properties](#user-content-editor-hints-on-string-properties)
1. [Form layout and visibility](#user-content-form-layout-and-visibility)
1. [Visibility and the component setup wizard](#user-content-visibility-and-the-component-setup-wizard)
1. [Dynamic allowed values](#user-content-dynamic-allowed-values)
1. [Permissions on a public API interface method](#user-content-permissions-on-a-public-api-interface-method)
1. [Recommended annotation order](#user-content-recommended-annotation-order)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## How a backend configuration differs from a frontend one

Three differences, and they account for most of the confusion:

| | Frontend component | Backend component |
|---|---|---|
| **Data type** | declared with `@Is…()` or `@Implements("…")`, because TypeScript types are erased | **inferred from the C# property type.** There are no `Is…` data type annotations on the backend, and you never declare one |
| **Persisted name** | pinned with `@ComponentProperty({ name })` | **the C# property name is the persisted name.** There is no `ComponentProperty` annotation — which means renaming a property is exactly as destructive, with nothing to protect you |
| **Where it is declared** | on the component class itself | on a separate class implementing `IComponentConfiguration`, which the component's `IComponentStartup.ConfigurationImplementation` points at |

The backend also has annotations the frontend does not: `FormItemLayout`,
`LocalizedStringsDefaultValue`, `IsCss`, `IsCssDimension`, `IsCssGradient`, `IsCssShadow`,
`IsFont`, `IsLiquid`, `IsJavascript` and `RequiredPermissions`.

## Which component's configuration a setting belongs in

**Every backend component type has its own configuration class.** A connector has one, and so
does *each data adapter*, each data processor, each identity provider, each portal template,
each resource and each task handler action — a class implementing `IComponentConfiguration`,
pointed at by that component's `IComponentStartup.ConfigurationImplementation`, rendered as its
own form when an administrator configures that component. Nothing has to be piled onto the
connector, and a setting on the wrong component is one of the harder things to undo, because
[the property name is persisted permanently](#user-content-how-a-backend-configuration-differs-from-a-frontend-one).

The connector's configuration is for what the *connector* does: the credentials and the service
endpoint that establish the trust context, and the tenant, channel or site that decides which
content its schema describes. Everything else belongs to the component that actually reads it.

A **data adapter's** configuration is the right home for:

- **settings that differ per adapter**, even when both adapters sit on the same connector — the
  output format allow-lists, the metadata attributes to preserve, the integration mode, upload
  settings, an AI search setting. These come from
  [the configuration marker interfaces](smintio-data-adapter-interfaces.md#user-content-configuration-marker-interfaces);
- **customer- or deployment-specific settings** that only this adapter interprets — a limit, a
  naming scheme, a behaviour one customer wants and another does not;
- **secrets the adapter itself needs**, most commonly the key it verifies an inbound callback
  with, or the key it signs a time-limited link with. That is not a contradiction of "a data
  adapter never holds a credential": that rule is about authenticating to the *external system*,
  which stays inside the connector. See
  [two kinds of security](smintio-data-adapter-interfaces.md#user-content-two-kinds-of-security-and-which-one-is-yours)
  and [accepting a callback](smintio-backend-recipes.md#user-content-how-do-i-accept-a-callback-from-the-external-system).

The practical test is **one connector, several data adapters**. A property on the connector
forces the same value on every data adapter configured against it, and changing it can trigger a
re-validation of the connection and a re-index. A property on a data adapter is set once per
configured adapter, which is what an administrator expects of a setting that only changes how
*that* adapter behaves.

## Configuration is stored encrypted

Component configurations are **persisted encrypted at rest** in the Smint.io database — for
connectors, for data adapters and for every other component type alike. A password, an API key
or a signing secret typed into a configuration form is stored securely, so a setting of that
kind is a legitimate configuration property and not something to work around.

**A secret is never shown again either.** Once it has been saved, a configured secret is not
transmitted back to the administration user interface — the administrator sees a placeholder and
can replace the value, but cannot read it back. There is no annotation for this and no scheme of
your own is needed; what decides it is **the name of the property**.

A `string` property is treated as a secret when its name contains any of these, case-insensitively:

| | |
|---|---|
| `password` | `secret` |
| `key` | `token` |
| `credential` | `authconfig` |
| `sasuri` | |

So `ClientSecret`, `ApiKey`, `LinkSigningKey`, `AccessToken` and `ServicePassword` are all
protected without you doing anything. A property holding a secret under a name that contains
none of them — `Signature`, `Passphrase`, `Salt` — is **not**, and is shown back like any other
value. Name the property so that it is covered; it is the only control you have over this.

Two limits worth knowing: the rule applies to single `string` properties, not to `string[]`, so
do not keep a list of secrets in one; and the rule matches on a *substring*, so an innocent
property whose name happens to contain one of the words — `KeyboardShortcut`, `MonkeyName` —
becomes unreadable for the administrator after saving. Rename it if that happens.

The one thing this does not cover is what *your component* does with the value. Once you have
read it out of the configuration it is a string like any other: never log it, never put it into
an exception message, and never return it from a public API interface method, where it would
travel to a browser.

## The property type is the data type

The type of the C# property is what decides the form item Smint.io Portals renders. These are
the types that are understood:

| C# property type | Form item |
|---|---|
| `string`, `string[]` | text field, list of text fields |
| `LocalizedStringsModel` | one text field per portal language |
| `int`, `int?`, `int[]` | number field |
| `long`, `long?`, `long[]` | number field |
| `decimal`, `decimal?` | number field |
| `bool`, `bool?` | switch |
| `DateTimeOffset`, `DateTimeOffset?` | date and time picker |
| any `enum`, and arrays of it | dropdown, built from the enum's members |
| `IDataAdapterInterface` and any interface deriving from it, and arrays | **data adapter picker** — the administrator chooses a configured data adapter that publishes that public API interface |
| `IPageReference` | page picker |
| `IResourceReference`, `IResourceReference[]` | resource picker |
| `AssetIdentifier`, `AssetIdentifier[]` | asset picker |
| `FolderIdentifier`, `FolderIdentifier[]` | folder picker |
| `AssetsReferenceModel` | the combined "which assets" picker — a fixed list, a folder, a search spec or a relationship of the current asset |
| `SearchAssetsSpecModel` | search specification builder |
| `MetadataAttributeModel`, `MetadataAttributeModel[]` | metadata attribute picker, resolved against the connector meta-model |

A property whose type is not in this table does not become a form item. If you need one that is
missing, get in touch rather than working around it.

Nullable and non-nullable are both accepted for the value types; use the nullable form when
"not set" is meaningful and different from zero or `false`.

**A non-nullable value type is implicitly required**, because it has no way to express "not
answered" — so give it a `DefaultValue`. Without one, a `bool` the administrator never touches
counts as unanswered, and the failure is at its most confusing on a property marked `Advanced` or
`Expert`: those are not shown in the component setup wizard, so the wizard fails on a field nobody
was offered. Either carry a `DefaultValue`, or make the property nullable and mean it.

```C#
[Serializable]
public class MyConnectorConfiguration : IComponentConfiguration
{
    [DisplayName("en", "Your service URL", IsDefault = true)]
    [DisplayName("de", "Ihre Service-URL")]
    [Description("en", "The base URL of your service.", IsDefault = true)]
    [Required]
    [IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
    public string ServiceUrl { get; set; }

    [DisplayName("en", "Your access token", IsDefault = true)]
    [MaxLength(100)]
    [Required]
    public string AccessToken { get; set; }
}
```

Mark the configuration class `[Serializable]`. Every shipped component does, and it costs
nothing to be consistent.

## Labels and help text

| Annotation | What it does |
|---|---|
| `DisplayName` | the label of the form item, per language |
| `Description` | the help text of the form item, per language |
| `FormGroupDisplayName` | the label of a form group, per language |
| `FormGroupDescription` | the help text of a form group, per language |

All four take the same two shapes:

```C#
[DisplayName("en", "Channel", IsDefault = true)]     // language, text, default-culture marker
[DisplayName("de", "Kanal")]

[DisplayName(translationKey: nameof(ConfigurationMessages.c_myconnector_channel_display_name))]
```

**Exactly one language per item must be marked as the default culture**, and in practice that
is always `"en"`. A component with no default-culture translation on an item fails to publish.

The second shape resolves the text from a resource file instead, which is the better choice as
soon as you support more than two languages — see the next section.

## Translating with resource files

Instead of repeating a `DisplayName` per language on every property, put the strings in .NET
resource files and reference them by key.

These `.resx` files are **not** Smint.io Portals *resources* — the portal content type an
administrator creates and components point at. These are ordinary .NET resource files, carrying
the translated strings your own component's code ships with. See
[portal templates and resources](smintio-portal-templates-and-resources.md#user-content-resources)
for the other meaning.

1. Add `ConfigurationMessages.resx` under a `Resources` folder in your project, plus one file
   per additional culture — `ConfigurationMessages.de.resx`, `ConfigurationMessages.fr.resx`
   and so on.
2. Point your startup class at it:

   ```C#
   public Type ConfigurationMessages => typeof(ConfigurationMessages);
   ```

3. Reference the keys from the annotations:

   ```C#
   [DisplayName(translationKey: nameof(ConfigurationMessages.c_myconnector_service_url_display_name))]
   [Description(translationKey: nameof(ConfigurationMessages.c_myconnector_service_url_description))]
   [Required]
   [IsUri(EnforceHttps = true, RemovePathAndQueryString = true)]
   public string ServiceUrl { get; set; }
   ```

### How a resource key has to be named

Resource keys are **not free-form**. Every key in `ConfigurationMessages` and in
`MetamodelMessages` starts with a prefix built from two things — the kind of component, and the
component's own `Key`:

```
<component kind>_<your component Key, hyphens replaced by underscores>_<what it labels>
```

The kind is a fixed abbreviation:

| Component | Prefix |
|---|---|
| Connector | `c` |
| Data adapter | `da` |
| Data processor | `dp` |
| Identity provider | `idp` |
| Task handler | `th` |
| Portal template | `pot` |
| Resource | `r` |
| Page template | `pgt` |
| UI component | `uic` |

So a connector whose `Key` is `hello-world` names its keys `c_hello_world_name`,
`c_hello_world_description`, `c_hello_world_setup_documentation_url`, and a data adapter whose
`Key` is `assets` names its keys `da_assets_name`, `da_assets_description`. Note the **hyphens of
the component key become underscores** — a data processor keyed `asset-download-change-file-name`
prefixes its keys `dp_asset_download_change_file_name_`.

The remainder is yours: `_display_name` and `_description` for a configuration property,
`_form_group_display_name` for a form group, whatever reads well for a meta-model entity.

The prefix is not decoration. The resource files of every registered component are loaded **side
by side with each other and with the SDK's own**, and a key is looked up by its bare name. Two
components that both ship a `service_url_display_name` collide, and one silently wins — which is
exactly what the prefix prevents. It also means:

- **Get the prefix right before you ship.** Renaming a resource key later is safe in itself, but
  every annotation, every `ResourceLocalizedStringsModel` and every
  `FullyResolveToLocalizedStringsModel` call that names it has to move with it.
- **The component `Key` is issued by Smint.io** and cannot change after release, so the prefix
  cannot either. Ask for the key before you write the resource file, not after.
- **One prefix per component, not per project.** A project that ships a connector and two data
  adapters has three prefixes — `c_<connector key>_`, `da_<first adapter key>_`,
  `da_<second adapter key>_` — normally in three separate resource files, one per component.

There is a second resource file with the same mechanics, `MetamodelMessages`, pointed at by
`IConnectorStartup.MetamodelMessages` (and `IDataAdapterStartup.MetamodelMessages`). It carries
the labels of the entities and properties of
[the connector meta-model](smintio-connector-metamodel.md), which are resolved at a different
point in the lifecycle than configuration labels — that is the whole reason the two are
separate files. Use `ResourceLocalizedStringsModel` to reference one from a meta-model builder:

```C#
var rootEntityLabels = new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_myconnector_root_entity));
```

### Three ways to turn a resource key into a localized string

The annotations above take a *key*. Elsewhere you have to hand the platform a
`LocalizedStringsModel` **object** — an allowed value's display name, a permission's name, a
download option's description. Three forms do that, and picking the wrong one is the usual cause
of a label that shows up in one language only.

| Form | What you get | Use it for |
|---|---|---|
| `new ResourceLocalizedStringsModel(nameof(X.key))` | a **reference** to the key; the platform resolves it when it needs it | anything the platform resolves on its own schedule: the startup's `Name` and `Description`, meta-model entity and property labels |
| `X.ResourceManager.FullyResolveToLocalizedStringsModel(nameof(X.key))` | a **finished model carrying every language you ship** | anything you build yourself and hand over complete: allowed values, permissions, download item descriptions |
| `X.key.Localize()` | a single-culture model, in the **current request's** culture | a value that only ever has to be right for the caller in front of you |

The middle one is what you want whenever a translated label has to exist in full, and it is a
single call:

```C#
using SmintIo.Portals.SDK.Core.Extensions;

var name = MetamodelMessages.ResourceManager
    .FullyResolveToLocalizedStringsModel(nameof(MetamodelMessages.da_myadapter_recent_uploads));
```

It reads that key out of **every** satellite resource file your component ships and returns one
model containing all of them, so you never assemble a dictionary of languages by hand. The SDK
itself uses it for the built-in permission names and the built-in output format names.

Three things to know about it:

- **It is anchored on `en-US`.** If the key has no `en-US` value the method returns `null` and the
  label silently disappears — a key that exists only in `.de.resx` produces nothing at all.
- **Languages are keyed two-letter** (`de`, not `de-AT`), and a culture whose value is identical to
  the default is left out rather than duplicated.
- **It walks every culture, so it is not free.** Resolve once into a `static readonly` field or a
  cached value — not per request, and certainly not per row of a result.

`Localize()` is the request-time alternative. It works because the request culture is already set
before your method runs, so the resource manager resolves the culture-specific literal — or falls
back to the default one:

```C#
Name = MetamodelMessages.c_myconnector_advanced.Localize()
```

Use it when the value is consumed immediately and never stored. Use the fully-resolved form when
the model is persisted, indexed, or read back later by a caller in another language.

**A component that declares `ConfigurationMessages` is the norm, not the exception.** Every
component Smint.io ships has one: the startup's own `Name` and `Description` come from it, as
`new ResourceLocalizedStringsModel(nameof(ConfigurationMessages.da_myadapter_name))`.
`MetamodelMessages` is the opposite — declare it only once you actually ship one.

## Values and validation

| Annotation | What it does |
|---|---|
| `Required` | the administrator must supply a value |
| `DefaultValue` | the value the form item starts with. Overloads for `bool`, `int`, `long`, `double`, `string`, `DateTimeOffset` and the array forms |
| `LocalizedStringsDefaultValue` | the default for a `LocalizedStringsModel` property, one per language, with the same default-culture marker as `DisplayName` |
| `AllowedValues` | a fixed list of permitted values. Overloads for `int[]`, `long[]`, `double[]`, `decimal[]`, `string[]` |
| `AllowedDateTime`, `AllowedUtcDateTimes` | the same for date and time values |
| `MinLength`, `MaxLength` | string length bounds |
| `MinValue`, `MaxValue` | numeric bounds. Both have an overload taking a divisor, for a value stored in one unit and entered in another |
| `RegularExpression` | the value must match, given as a pattern string or a `Regex` |
| `UtcAfter`, `UtcAfterOrEqual`, `UtcBefore`, `UtcBeforeOrEqual` | bounds on a UTC date and time value |
| `UtcDateTime` | marks a string form item as carrying a UTC date and time |
| `DisableTrimming` | keeps leading and trailing whitespace, which is trimmed by default |
| `Unsealed` | marks the value as unsealed |

Use `AllowedValueDisplayName` and `AllowedValueDescription` to label the individual entries of
an `AllowedValues` list per language, with the same default-culture rule as `DisplayName`.

Validation runs when the administrator saves the configuration form. It is not the only check
that happens then — your connector's `PerformPostConfigurationChecksAsync` runs too, and that is
where anything requiring a call to the external system belongs. Annotations cover what can be
decided from the value alone.

## Editor hints on string properties

These do not change the stored type — the property is still a `string` — but they change the
editor the administrator gets and the validation applied.

| Annotation | What it does |
|---|---|
| `IsUri` | the value is a URL. `EnforceHttps` rejects plain HTTP; `RemovePathAndQueryString` reduces a pasted URL to its origin, which is what you almost always want for a service base URL |
| `IsEmailAddress` | the value is an email address |
| `IsPhoneNumber` | the value is a phone number |
| `IsJson` | the value is JSON, and is validated as such |
| `IsRichText` | the value is sanitized, validated HTML. Takes an optional `HtmlTagWhitelist` |
| `IsColor` | a colour picker |
| `IsCss`, `IsCssDimension`, `IsCssGradient`, `IsCssShadow` | CSS, a CSS length, a CSS gradient, a CSS box shadow |
| `IsFont` | a font specification |
| `IsLiquid` | a Liquid template |
| `IsJavascript` | JavaScript source |

`IsLiquid` is worth knowing about even if you are not writing a data processor: it is how the
shipped processors let an administrator express a file naming pattern or a metadata
transformation without writing a component.

## Form layout and visibility

| Annotation | What it does |
|---|---|
| `FormGroup` | puts the form item into a form group. The id must match a `FormGroupDeclaration` on the class |
| `FormGroupDeclaration` | declares a form group on the configuration class. `isDefault` marks the group that properties carrying no `FormGroup` fall into |
| `SortPosition` | the order of a form item within its group |
| `FormItemVisibility` | the visibility level of a form item — `Basic`, `Advanced`, `Expert`, `Hidden` |
| `FormItemLayout` | column counts for desktop and mobile, and whether to break the line before or after the item |
| `VisibleIf` | show the item only when another property has a given value |
| `FormGroupVisibleIf` | the same for a whole form group; declared on the class, naming the group |

`VisibleIf` and `FormGroupVisibleIf` take a `VisibleIfOperators` value: `Equal`, `NotEqual`,
`GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `OneOf`. Both have overloads
for every supported value type, including the array forms that pair with `OneOf`.

```C#
[FormGroupDeclaration("connection", isDefault: true)]
[FormGroupDisplayName("connection", "en", "Connection", IsDefault = true)]
[FormGroupDeclaration("advanced")]
[FormGroupVisibleIf("advanced", nameof(UseAdvancedMode), VisibleIfOperators.Equal, true)]
[Serializable]
public class MyConnectorConfiguration : IComponentConfiguration
{
    [DisplayName("en", "Use advanced mode", IsDefault = true)]
    [FormGroup("connection")]
    [DefaultValue(false)]
    public bool UseAdvancedMode { get; set; }

    [DisplayName("en", "Batch size", IsDefault = true)]
    [FormGroup("advanced")]
    [FormItemVisibility(Visibility = FormItemVisibilityEnum.Advanced)]
    [MinValue(1)]
    [MaxValue(500)]
    [DefaultValue(50)]
    public int BatchSize { get; set; }
}
```

Use `FormItemVisibility` rather than leaving a rarely needed setting in the basic form. An
administrator who never opens the advanced section is the intended reader of the basic one.

### Visibility and the component setup wizard

**The setup wizard shows `Basic` items only.** It is the short form an administrator fills in
when first creating the component; `Advanced`, `Expert` and `Hidden` items are reached afterwards,
in the component's full configuration form. A visibility level is therefore also a statement about
*when* a value can first be supplied, and that has one consequence:

**A required property that the wizard does not show must be able to supply its own value.** The
requirement is still enforced there, on a field the administrator never sees, so without a value
the component cannot be created at all and the error names a property nobody was offered. A
`DefaultValue` settles it: **`Required` together with `Advanced` or `Expert` is fine as long as
the property has a default**, which is why that combination appears throughout the shipped
components.

There are two ways for a property to be required, and only one of them is visible in the code:

- **explicitly**, with `Required`;
- **implicitly**, because
  [a non-nullable value type](#user-content-the-property-type-is-the-data-type) has no way to
  express "not answered". This is the one that gets missed — the word `Required` appears nowhere,
  and an `Advanced` `bool` without a `DefaultValue` breaks the wizard exactly as an explicit
  `Required` would.

The fix is the same either way: give the property a `DefaultValue`. Where no default is safe — a
signing key or a password has no sensible one, and inventing a default secret would be worse than
the problem — drop `Required`, make the property optional, and give its absence a defined,
safe behaviour that the component logs clearly. Refusing to serve is usually that behaviour; say
so in the property's description, because it is the administrator who has to act on it.

## Dynamic allowed values

When the permitted values are not known until the external system is asked — a channel, a site,
a list, an output format — declare a provider instead of a static `AllowedValues` list.

```C#
[DisplayName(translationKey: nameof(ConfigurationMessages.c_myconnector_channel_display_name))]
[DynamicAllowedValuesProvider(typeof(ChannelAllowedValuesProvider))]
[FormGroup("connection")]
public string Channel { get; set; }
```

The provider is a class implementing `IDynamicValueListProvider<T>`. It is called while the
administrator is filling in the configuration form, so it runs against a connector that is
configured but not necessarily fully authorized yet — handle that case rather than throwing.

`DynamicAllowedValuesProvider` has overloads that additionally take a parameter type, a
parameter JSON literal, or the name of another property to feed the provider from. The last one
is how a dependent dropdown is built: a "list" picker that only offers the lists of the site
already chosen above it.

## Permissions on a public API interface method

`RequiredPermissions` is the one annotation here that does not go on a configuration property.
It goes on a **method of a data adapter public API interface**, and declares which permissions a
portal user must hold for that method to be callable:

```C#
[RequiredPermissions(DataAdapterPermission.ReadAssetDetailsPermissionUuid,
                     DataAdapterPermission.SearchAssetsPermissionUuid)]
Task<SearchAssetsResult> SearchAssetsAsync(SearchAssetsParameters parameters);
```

The standard Smint.io interfaces already carry these annotations, so an adapter that implements
only standard interfaces declares no permissions of its own. You need this only when you publish
a custom public API interface — see
[the data adapter public API interfaces](smintio-data-adapter-interfaces.md#user-content-custom-public-api-interfaces).

The annotation goes on the **interface**, not on your implementation of it. That is where the
platform reads it.

## Recommended annotation order

Order does not change behaviour on properties. The order below is the one the Smint.io
components use, and following it keeps a configuration class with forty properties readable:

1. `DisplayName` — default culture first
2. `Description`
3. `Required`
4. validation and editor hints — `IsUri`, `MaxLength`, `RegularExpression`, …
5. `AllowedValues` / `DynamicAllowedValuesProvider`, with `AllowedValueDisplayName`
6. `FormItemVisibility`
7. `FormItemLayout`
8. `FormGroup`
9. `VisibleIf`
10. `DefaultValue` — last, so the value a field starts with is easy to find

Class-level annotation order **does** matter for `FormGroupDeclaration`: declare the groups in
the order you want the administrator to see them.

## Things that are easy to get wrong

- **A `FormGroup` id that matches no `FormGroupDeclaration` is a mistake**, and a typo in one of
  the two is easy to miss. A property with *no* `FormGroup` at all is fine: it lands in the group
  declared `isDefault`, or in an automatically created one if you declared none — no backend
  property is ever an orphan. (This is the opposite of the frontend, where a form item without a
  form group does not appear at all. If you know the frontend rule, unlearn it here.)
- **Renaming a property orphans every saved configuration.** The C# property name is the key
  the value is stored under. There is no annotation that decouples the two, so a rename after
  release silently drops the value for every portal already configured against your component.
- **Exactly one default culture per translated item.** Zero fails the publish; more than one is
  ambiguous.
- **`Required` is not the same as a check against the external system.** It only proves the
  administrator typed something. Whether that access token works belongs in
  `PerformPostConfigurationChecksAsync`.
- **A required property the setup wizard does not show needs a `DefaultValue`.** The wizard shows
  `Basic` items only but still enforces the requirement, so without a default the component cannot
  be created and the error names a field nobody was offered. `Required` plus `Advanced` is fine
  *with* a default; where no default is safe, make the property optional instead. See
  [visibility and the component setup wizard](#user-content-visibility-and-the-component-setup-wizard).
- **A non-nullable value type is already required, so give it a `DefaultValue`.** A `bool` with
  neither a default nor a nullable type cannot express "not answered", and on an `Advanced` or
  `Expert` property the setup wizard then fails on a field it never showed. See
  [the property type is the data type](#user-content-the-property-type-is-the-data-type).
- **Do not reach for a `string` with a hand-rolled format when a typed property exists.**
  `AssetIdentifier`, `FolderIdentifier`, `IResourceReference`, `IPageReference` and
  `MetadataAttributeModel` all give the administrator a picker instead of asking them to paste
  an identifier, and they keep working when the underlying identifier format changes.
- **`IsUri(RemovePathAndQueryString = true)` on every service base URL.** Administrators paste
  the URL they happen to be looking at, complete with a path and a query string.
- **A secret is recognised by the property's name**, so a secret under a name that carries none
  of the recognised words is shown back to the administrator, and an ordinary property whose
  name happens to contain one of them is not. See
  [configuration is stored encrypted](#user-content-configuration-is-stored-encrypted).
- **Put a setting on the component that interprets it.** A data adapter has its own
  configuration, and a property put on the connector instead is forced on every data adapter
  configured against it. See
  [which component's configuration a setting belongs in](#user-content-which-components-configuration-a-setting-belongs-in).
- **Start with few properties.** You can add a setting in a later version; you cannot remove one
  without breaking the portals that set it.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
