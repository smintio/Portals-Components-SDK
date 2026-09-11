The Smint.io Portals connector meta-model
=========================================

Current version of this document is: 1.0.0 (as of 11th of September, 2026)

How a connector describes the external system's own schema, so that Smint.io Portals can interpret
the data your data adapter delivers.

This is the reference. For the shape of the objects that flow *through* the meta-model — assets,
folders, their content types and references — see
[the asset data model](smintio-asset-data-model.md). For a worked, single-entity example built
against a real system, see the
[SharePoint connector walkthrough](../Connectors/Connector-SharePoint/README.md#meta-model-structure).

1. [Why there is a meta-model at all](#user-content-why-there-is-a-meta-model-at-all)
1. [Three layers](#user-content-three-layers)
1. [`ConnectorMetamodel` — the container](#user-content-connectormetamodel--the-container)
1. [`EntityModel` — an object type](#user-content-entitymodel--an-object-type)
1. [`PropertyModel` — a field](#user-content-propertymodel--a-field)
1. [Data types](#user-content-data-types)
1. [Enum entities](#user-content-enum-entities)
1. [Full-text indexing](#user-content-full-text-indexing)
1. [Semantic types](#user-content-semantic-types)
1. [Form groups — declaring search facets](#user-content-form-groups--declaring-search-facets)
1. [Download sizes](#user-content-download-sizes)
1. [Translating a meta-model](#user-content-translating-a-meta-model)
1. [The converter — where the meta-model does its work](#user-content-the-converter--where-the-meta-model-does-its-work)
1. [Lifecycle: when your meta-model is built, and what happens to it](#user-content-lifecycle-when-your-meta-model-is-built-and-what-happens-to-it)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## Why there is a meta-model at all

Everything that flows through Smint.io Portals — an asset, a folder, a metadata layer, an enum value
— is a `DataObject`: a bag of keys and values, not a typed class. A bag of values on its own cannot
be rendered, indexed, filtered or translated, because nothing says what the keys mean.

The meta-model is what says. It is the schema that turns `{"cf_0042": "Sunset"}` into a labelled,
typed, translatable, indexable field that a portal editor can point a component at.

Smint.io defines the meta-model for its own objects. **You define the meta-model for everything that
comes out of the external system** — which in practice means everything that ends up in an asset's
`rawData`.

## Three layers

Keeping these apart makes the rest of this document much easier to read:

| Layer | What it is | Type |
|---|---|---|
| **Instance** | the data itself — one object's keys and values | `DataObject` |
| **Description** | the schema of one object type and its fields | `EntityModel` → `PropertyModel` |
| **Container** | one whole meta-model, for one configured connector | `ConnectorMetamodel` |

A connector builds the container. A data adapter uses it to produce instances.

## `ConnectorMetamodel` — the container

You create one, fill it, and return it from `IConnector.GetConnectorMetamodelAsync()`. The
convention is to do the building in a separate class implementing `IMetamodelBuilder`, which has a
single `BuildAsync()` method:

```C#
public Task<ConnectorMetamodel> GetConnectorMetamodelAsync()
{
    var client = EnsureHelloWorldClient();

    return new HelloWorldMetamodelBuilder(_logger, client, _connectorConfiguration).BuildAsync();
}
```

The constructor takes an identifier and three feature flags:

```C#
_metamodel = new ConnectorMetamodel(
    identifier,
    isRandomAccessSupported: true,
    isFullTextSearchProposalsSupported: true,
    isFolderNavigationSupported: false);
```

| Flag | Declares that the external system can |
|---|---|
| `isRandomAccessSupported` | jump straight to an arbitrary result page, rather than only paging forward |
| `isFullTextSearchProposalsSupported` | return search-as-you-type proposals |
| `isFolderNavigationSupported` | be browsed as a folder tree |

These are the connector's declaration. The data adapter reports the *effective* values to the
frontend through `GetFeatureSupportAsync` — see
[the asset data model](smintio-asset-data-model.md#user-content-say-what-you-support). Keep the two
consistent: a portal asks the data adapter, so a flag set here and denied there simply never takes
effect.

A container holds four kinds of thing:

| Added with | Holds |
|---|---|
| `AddEntity(...)` | the object types — `EntityModel` |
| `AddFormGroup(...)` | the search facets you offer — `FormGroupModel` |
| `AddDownloadSize(...)` | rendition formats the source system offers — `DownloadSizeModel` |
| — | the three feature flags above |

## `EntityModel` — an object type

One entity per type of object in the external system. If the system has only files, you have one
entity; if it distinguishes products, images and documents, you have three.

```C#
var entity = new EntityModel("HelloWorldAsset", labels);
entity.Type = EntityType.TopLevelObject;

metamodel.AddEntity(entity);
```

or in one call:

```C#
metamodel.AddEntity("HelloWorldAsset", EntityType.TopLevelObject, parentEntityModelKey: null, labels);
```

| Member | Meaning |
|---|---|
| `Key` | unique within the meta-model. **May not contain `:`** — the constructor throws |
| `Type` | see below. Defaults to `EntityType.Unknown`, so set it deliberately |
| `Labels` | the localized display name |
| `ParentMetamodelEntityKey` | inheritance — this entity also has the parent's properties |
| `Properties` | the `PropertyModel`s, added with `AddProperty` |

`EntityType` decides how the portal treats the entity:

| `EntityType` | Use for |
|---|---|
| `TopLevelObject` | the root object of the external system — the thing an asset *is* |
| `MetadataLayer` | one of several metadata schemas that can be attached to an object. Use this when the system has multiple templates or layers rather than one flat schema |
| `Fieldset` | a reusable group of fields, referenced from another entity |
| `Enum` | a set of allowed values — set for you by `EnumEntityModel.CreateEntity` |
| `Other`, `Unknown` | fallback; `Unknown` is the default and rarely what you want |

**Inheritance** works through `ParentMetamodelEntityKey`: `GetProperty` walks up the chain, so a
child entity resolves its parent's properties too. This is how you model a system where several
object types share a common base.

**Three properties are added to every entity automatically**, by the constructor: an id, a list
display name and a detail display name (`smintIoId`, `smintIoListDisplayName`,
`smintIoDetailDisplayName`). You never declare them, you cannot remove them, and you will see them
on every object on the wire — including objects describing your own custom data. Populate at least
the id and the list display name for anything the portal will show in a list.

## `PropertyModel` — a field

Created through `AddProperty` on the entity, never with its own constructor:

```C#
entity.AddProperty("displayName", DataType.String, labels, FullTextIndexType.HighPriority);
entity.AddProperty("owner", DataType.DataObject, userEntity.Key, labels);
```

| Member | Meaning |
|---|---|
| `Key` | unique within the entity. `AddProperty` throws if it already exists |
| `DataType` | how the value is encoded — see [data types](#user-content-data-types) |
| `TargetMetamodelEntityKey` | **required** for `DataObject`, `DataObjectArray`, `Enum` and `EnumArray` — the key of the entity on the other side |
| `Labels` | the localized display name |
| `FullTextIndexType` | whether and how the value is full-text indexed |
| `SemanticType` | how the value should be *interpreted* — see [semantic types](#user-content-semantic-types) |
| `SemanticHint` | the extra detail a semantic type needs, e.g. the currency code for a currency value |
| `ProprietaryValue` | free slot for your own back-reference to the source field. Smint.io never reads it; use it to find your way back from a property to the system field it came from |
| `LinkedTranslationProperties` | other properties *on the same object* that hold translations of this one. Only meaningful on the three localized data types |

The overload you use decides what gets set — there is one without labels, one with labels and an
optional index type, and one that additionally takes the target entity key. Prefer the overloads
that take labels: a property without them shows up unlabelled in the portal.

## Data types

`DataType` is the metamodel data type enum. Eighteen values plus `Undefined`:

| Scalar | Array | Localized | Structured |
|---|---|---|---|
| `String` | `StringArray` | `LocalizedStringsModel` | `DataObject` |
| `Boolean` | `Int32Array` | `LocalizedStringsArrayModel` | `DataObjectArray` |
| `Int32` | `Int64Array` | `LocalizedEnumsArrayModel` | `Enum` |
| `Int64` | | | `EnumArray` |
| `Decimal` | | | `CurrencyModel` |
| `DateTime` | | | `GeoLocationModel` |

`Undefined` exists as the zero value; do not declare a property with it.

Four of these need a `TargetMetamodelEntityKey` and will not work without one: `DataObject`,
`DataObjectArray`, `Enum` and `EnumArray`. That is the parameter in the three-argument
`AddProperty` overload:

```C#
// "owner" is a User object; the User entity describes it
rootEntity.AddProperty("owner", DataType.DataObject, userEntity.Key, labels);
```

> **Watch out for the other `DataType`.** `FormItemModel.DataType` is a `ValueTypeEnum` — the
> *configuration* value type enum, with values like `String`, `AssetId` and `DataAdapterInstanceKey`
> — and not this one. Two different enums, same property name. Check which one you are holding.

## Enum entities

A property whose value comes from a fixed set uses `DataType.Enum` (or `EnumArray`) and points at an
`EnumEntityModel`. Create it through the static factory, which sets `Type = EntityType.Enum` for
you:

```C#
var enumEntity = EnumEntityModel.CreateEntity(key, parentEntityModelKey: null, labels);

enumEntity.AddEnumValue("red", listDisplayName, detailDisplayName);
enumEntity.AddEnumValue("green", listDisplayName, detailDisplayName);

metamodel.AddEntity(enumEntity);

rootEntity.AddProperty("colour", DataType.Enum, enumEntity.Key, labels);
```

`AddEnumValue` comes in a plain-string form, a localized form, and a generic form taking an
`EnumDataObject` subclass. Values keep the order in which you add them, which is the order the
portal offers them in.

An enum entity is a normal entity in every other respect — it gets the same three automatic
properties, and it can inherit.

## Full-text indexing

`FullTextIndexType` on a property decides whether its value reaches the search index:

| Value | Effect |
|---|---|
| `HighPriority` | indexed, and weighted up in relevance — names, titles, tags |
| `LowPriority` | indexed, weighted normally — descriptions, long text, secondary fields |
| `ForceExclude` | never indexed, even if something else would have included it |

Leave it unset and the property follows the default handling rather than being explicitly
prioritised. Set `HighPriority` on the handful of fields a user would actually type when looking for
the object, and `ForceExclude` on anything noisy, internal, or high-cardinality that would dilute
relevance.

This is easy to forget and hard to diagnose later: search quality problems in a portal usually trace
back to a meta-model that never declared what mattered.

## Semantic types

`DataType` says how a value is *encoded*. `SemanticType` says how it should be *interpreted*. It is
optional, set on the property after `AddProperty`, and the frontend keys display behaviour off it:

| `SemanticType` | Meaning |
|---|---|
| `Url` | render as a link |
| `Email` | render as a mailto link |
| `PhoneNumber` | render as a tel link |
| `Html` | the string is markup, not text |
| `DateOnly`, `DateTime`, `TimeOnly` | which part of a date value to show |
| `Relationship` | **the value is the ID of another asset** |

`SemanticHint` carries whatever extra the type needs — the currency code for a currency value, for
instance.

`Relationship` is the one that changes the data rather than its presentation, and it is the one that
needs care:

```C#
var targetIdProperty = targetEntity.AddProperty("_targetId", DataType.String, labels);
targetIdProperty.SemanticType = SemanticType.Relationship;
```

Declaring it is only half the job. Your data adapter's converter receives the semantic type on every
`Get…DataType` call, and is expected to rewrite the raw foreign key into an asset identifier the
portal can follow. **A property declared `Relationship` whose converter does not handle it emits an
ID nothing can resolve, and nothing reports an error.** Add the declaration and the conversion
together. See
[references between assets](smintio-asset-data-model.md#user-content-references-between-assets).

## Form groups — declaring search facets

The meta-model is also where a connector declares **which filters it offers**. A form group is a
facet; its form items are the fields within it.

```C#
var formItems = new List<FormItemModel>
{
    new FormItemModel(itemKey, itemLabels, ValueTypeEnum.StringArray)
};

metamodel.AddFormGroup(groupKey, groupLabels, formItems);
```

or by building the group first and calling `AddFormItem(key, labels, dataType)` on it.

| `FormGroupModel` | |
|---|---|
| `Key` | unique within the meta-model |
| `SourceId` | your own reference back to the source system's facet |
| `Labels` | localized display name |
| `FormItems` | the `FormItemModel`s |
| `IsIntegrationLayer` | set by Smint.io for index-backed groups; leave it alone in a connector |

Typically you derive form groups from the same source schema you derived the entities from — a
dedicated `FormGroupsModelBuilder` alongside the metamodel builder is the usual shape, and
`HelloWorldFormGroupsModelBuilder` is the worked example.

**The meta-model only declares that a facet exists.** The live values — the current selection, the
allowed values with their counts and their filter tokens — are produced per search and returned by
the data adapter in the search result. Those tokens are opaque and must never be constructed by
hand; see
[filters are opaque tokens](smintio-asset-data-model.md#user-content-filters-are-opaque-tokens).

Remember that `FormItemModel.DataType` is a `ValueTypeEnum`, not a metamodel `DataType`.

## Download sizes

`AddDownloadSize(key, labels)` advertises a rendition format the external system offers. It is part
of the container for completeness, but note that download options actually reach the portal through
the data adapter's `IAssetsDownload` interface, not from here — so treat this as declarative only,
and put your real effort into the download interface.

## Translating a meta-model

Entity and property labels are `LocalizedStringsModel`. You can build them inline, but for anything
beyond a handful of strings use resource files instead: create `MetamodelMessages.resx` plus a file
per culture, register the type on your connector startup, and reference the keys with
`ResourceLocalizedStringsModel`:

```C#
public Type MetamodelMessages => typeof(MetamodelMessages);
```

```C#
var labels = new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_sharepoint_root_entity));

var entity = CreateEntityModel(RootEntityKey, labels);
```

A `ResourceLocalizedStringsModel` holds only a translation key when you create it. Smint.io resolves
every one of them to real localized strings — across entities, properties, form groups, form items
and download sizes — once, after your builder returns, and caches the result. That is why the
translation has to be expressible as a key rather than resolved by you at build time.

Resource keys are conventionally `c_` plus your connector's key plus the field, e.g.
`c_picturepark_access_token`.

## The converter — where the meta-model does its work

The meta-model is only half the mechanism. The other half lives in the data adapter: the
`DataObjectConverter` family in the data adapter SDK
(`SmintIo.Portals.DataAdapterSDK.DataAdapters.Converters`).

Pick the base class that matches the shape of your source payload:

| Base class | For a payload that is |
|---|---|
| `DictionaryObjectConverter` | a plain `IDictionary<string, object>` |
| `JObjectDictionaryObjectConverter` | a dictionary of `Newtonsoft.Json` values |
| `JsonElementDictionaryObjectConverter` | a dictionary of `System.Text.Json` elements |
| `JsonObjectConverter` | a `JObject` |

You call it with an entity key and the payload:

```C#
var dataObject = GetDataObject(HelloWorldMetamodelBuilder.RootEntityKey, objectsByKey);

assetDataObject.RawData = new[] { dataObject };
```

and it walks the payload **driven by your meta-model**: for each key it looks up the `PropertyModel`,
dispatches on the declared `DataType` to the matching `Get…DataType` method, and recurses through
`TargetMetamodelEntityKey` for nested objects and enums. Each of those methods receives the
property's `SemanticType` and `SemanticHint` along with the raw value, which is how a
`Relationship` property gets rewritten on the way out.

Three consequences:

- **A value whose property is not declared is dropped.** The meta-model is a whitelist.
- **A property declared but never supplied simply produces no entry.** It is absent downstream, not
  null.
- **Recursion stops at `MaxDepth`, which defaults to 10.** Deeply nested source structures are
  truncated rather than rejected. Raise it on the converter if you genuinely need more.

Override only the methods for the data types you actually emit; `CustomFieldDictionaryObjectConverter`
in the Hello World data adapter is a compact example of exactly that, including calling `base` for
the cases it does not special-case.

## Lifecycle: when your meta-model is built, and what happens to it

`GetConnectorMetamodelAsync` is called **when a connector configuration is set up or set up again**
— not per request, and not per search. What happens to the result, in order:

1. it passes through any data processors registered for this hook, which may add or alter entities;
2. every `ResourceLocalizedStringsModel` is resolved into real localized strings and cached;
3. every entity key is rewritten to be unique to this connector configuration;
4. the result is persisted on the connector configuration record.

Two things follow, and they surprise people:

- **Your meta-model is a snapshot.** A schema change in the external system — a new column, a
  renamed field, a new enum value — does not appear in Smint.io until the connector configuration is
  set up again. If a customer adds a field and cannot see it, that is the first thing to check.
- **Building it can be expensive, and that is acceptable.** Because it happens at configuration
  time, a builder is allowed to make several calls to the external system to read its schema. Do not
  contort the builder to be fast; do make it correct.

## Things that are easy to get wrong

- **Never hard-code an entity key.** Keys are rewritten per connector configuration during setup, so
  the same connector configured twice produces two different key sets, and neither matches what your
  builder wrote. Anything that needs to find an entity must resolve it, not assume it.
- **A key may not contain `:`.** The `EntityModel` constructor throws.
- **Set `EntityType`.** It defaults to `Unknown`.
- **`TargetMetamodelEntityKey` is mandatory for the four structured data types.** Without it the
  converter has nothing to recurse into.
- **Declare `FullTextIndexType` where it matters.** It is the difference between a searchable portal
  and one where nothing is found.
- **`Relationship` needs the converter half too**, or it silently produces unusable IDs.
- **`FormItemModel.DataType` is a `ValueTypeEnum`**, not a metamodel `DataType`.
- **Your meta-model is the filter.** If data is missing in the portal, check the declaration before
  you debug the conversion — an undeclared property is dropped without a word.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run into
any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
