Smint.io Portals backend component annotations
=============================================

Current version of this document is: 1.3.0 (as of 15th of September, 2026)

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
1. [Values and validation](#user-content-values-and-validation)
1. [Editor hints on string properties](#user-content-editor-hints-on-string-properties)
1. [Form layout and visibility](#user-content-form-layout-and-visibility)
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

Resource keys start with a short component-kind prefix followed by your component's `Key`:
`c_` for a connector, `da_` for a data adapter. So a connector whose key is `myconnector` names
its access token key `c_myconnector_access_token_display_name`. The prefix keeps keys unique
once resource files from several components are loaded side by side.

There is a second resource file with the same mechanics, `MetamodelMessages`, pointed at by
`IConnectorStartup.MetamodelMessages` (and `IDataAdapterStartup.MetamodelMessages`). It carries
the labels of the entities and properties of
[the connector meta-model](smintio-connector-metamodel.md), which are resolved at a different
point in the lifecycle than configuration labels — that is the whole reason the two are
separate files. Use `ResourceLocalizedStringsModel` to reference one from a meta-model builder:

```C#
var rootEntityLabels = new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_myconnector_root_entity));
```

A data adapter can also resolve a metamodel message to a plain localized string at request time,
which is what you want for a value that is displayed but does not have to be fully translatable
in the meta-model:

```C#
Name = MetamodelMessages.c_myconnector_advanced.Localize()
```

That works because the request culture is already set before your method runs, so the resource
manager resolves the culture-specific literal — or falls back to the default one.

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
