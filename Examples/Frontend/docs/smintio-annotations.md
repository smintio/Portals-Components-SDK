Smint.io Portals frontend component annotations
===============================================

Current version of this document is: 2.0.0 (as of 10th of September, 2026)

Annotations describe a frontend component's configuration to Smint.io Portals: what fields
the portal editor sees, what they are called in each language, what values they accept and
when they are shown.

All annotations listed here are exported from the frontend SDK,
`@smintio/portals-component-sdk`, and apply to both UI components and page templates unless
noted otherwise.

Please get in touch at [support@smint.io](mailto:support@smint.io) if you are missing an
annotation, or if you need a new one.

## Component (class) annotations

| Annotation                         | Description                                                                                                     |
|------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| `PortalsUiComponent`               | **Declares a UI component and its metadata (type, key, display name, description)**                             |
| `PortalsPageTemplateComponent`     | **Declares a page template and its metadata**                                                                   |
| `FormGroupDeclaration`             | **Defines a new form group (displayed as separate tab or section, depending on the viewer implementation)**      |
| `FormGroupDisplayName`             | **Specifies the display name of a form group in different languages**                                           |
| `FormGroupDescription`             | **Specifies the description of a form group in different languages**                                            |
| `SlotType`                         | **Defines the details of a page template slot**                                                                 |

Note: class decorators are applied bottom-up, so the form group declared **last in the
source appears first** in the configuration form. Declare your groups in reverse of the
order you want the tabs.

## Property annotations — labels and help text

| Annotation                         | Description                                                                                                     |
|------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| `DisplayName`                      | **Specifies the display name of a form item in different languages**                                            |
| `Description`                      | **Specifies the description of a form item in different languages**                                             |
| `AllowedValueDisplayName`          | **Specifies the display name of an allowed value of a form item in different languages**                        |
| `AllowedValueDescription`          | **Specifies the description of an allowed value of a form item in different languages**                         |

The last argument of these annotations marks a translation as the default-culture one. Set it
to `true` on exactly one language per item — in practice always `"en"`. Publishing the component
will fail if there is not at least one translation set as the default-culture one.

## Property annotations — data type

Every configuration property must declare a data type, either through `ComponentProperty`
(which can infer `string`, `boolean`, `number` and `Date`) or through one of the annotations
below. A property with no declared type makes the component fail to register with
*"Some properties lack type definition"*.

| Annotation                         | Description                                                                                                     |
|------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| `ComponentProperty`                | **Registers the property as a configuration property and pins the name it is stored under**                     |
| `Implements`                       | **Names the backend type an interface-typed form item implements**                                              |
| `ImplementsPrimitiveType`          | **Names the primitive `ValueType` of a form item**                                                              |
| `IsString`                         | **Specifies for a form item to be of string data type (auto-detected if possible)**                             |
| `IsStringArray`                    | **Specifies for a form item to be of string array data type (auto-detected if possible)**                       |
| `IsBoolean`                        | **Specifies for a form item to be of boolean data type (auto-detected if possible)**                            |
| `IsInt32`                          | **Specifies for a form item to be of int data type (auto-detected if possible)**                                |
| `IsInt32Array`                     | **Specifies for a form item to be of int array data type (auto-detected if possible)**                          |
| `IsInt64`                          | **Specifies for a form item to be of long data type (auto-detected if possible)**                               |
| `IsInt64Array`                     | **Specifies for a form item to be of long array data type (auto-detected if possible)**                         |
| `IsDecimal`                        | **Specifies for a form item to be of decimal data type (auto-detected if possible)**                            |
| `IsDate`                           | **Specifies for a form item to be of date data type (auto-detected if possible)**                               |
| `UtcDateTime`                      | **Specifies for the string form item to be of UTC date time data type**                                         |
| `IsDataAdapter`                    | **Specifies for a form item to reference a data adapter public API interface (auto-detected if possible)**      |
| `IsDataAdapterArray`               | **Specifies for a form item to reference an array of data adapter public API interfaces (auto-detected if possible)** |

`ComponentProperty` pins the name the value is persisted under. It defaults to the TypeScript
property name, which means renaming the property without pinning the name orphans the saved
configuration of every portal already using the component. Always pass it explicitly:
`@ComponentProperty({ name: "myProperty" })`.

## Property annotations — editor hints on string form items

| Annotation                         | Description                                                                                                     |
|------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| `IsColor`                          | **Specifies for a string form item to be of color data type**                                                   |
| `IsRichText`                       | **Specifies for a string form item to be rich text (optionally with an HTML tag whitelist)**                    |
| `IsUri`                            | **Specifies for a string form item to be an URI**                                                               |
| `IsEmailAddress`                   | **Specifies for a string form item to be an email address**                                                     |
| `IsPhoneNumber`                    | **Specifies for a string form item to be a phone number**                                                       |
| `IsJson`                           | **Specifies for a string form item to be a JSON string**                                                        |
| `DisableTrimming`                  | **Disables the trimming of strings for a form item (which is done by default)**                                 |
| `Unsealed`                         | **Marks the value of a form item as unsealed**                                                                  |

A property marked `IsRichText` carries *sanitized, validated* HTML. Render it with `v-html`, not with interpolation:

```vue
<p v-html="$options.filters.resolve_localized(continuousText)"></p>
```

Such a property is backed by a **text** resource — a rich text — rather than a **string**
resource, which is a plain text such as a label, a button caption or a heading. Pair it with
`TextResourceAllowedValuesProvider`, and see
[Shipping your own string resources](../README.md#shipping-your-own-string-resources).

## Property annotations — values and validation

| Annotation                         | Description                                                                                                     |
|------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| `DefaultValue`                     | **Specifies the default value of a form item.** On an `ILocalizedStringsModel` property the value is a string resource id — see [Shipping your own string resources](../README.md#shipping-your-own-string-resources) |
| `InitializationValue`              | **Specifies the value a form item is initialized with**                                                         |
| `AllowedValues`                    | **Gives multiple allowed values of a form item**                                                                |
| `AllowedDateTime`                  | **Gives an allowed date time value of a form item**                                                             |
| `AllowedUtcDateTimes`              | **Gives multiple allowed UTC date time values of a form item**                                                  |
| `DynamicAllowedValuesProvider`     | **Specifies the name of the provider of dynamic allowed value lists of a form item**                            |
| `Required`                         | **Forces the user to set a value for a form item**                                                              |
| `MinLength`                        | **Restricts the min length of a value of a form item**                                                          |
| `MaxLength`                        | **Restricts the max length of a value of a form item**                                                          |
| `MinValue`                         | **Restricts the lower boundary of a value of a form item**                                                      |
| `MaxValue`                         | **Restricts the upper boundary of a value of a form item**                                                      |
| `RegularExpression`                | **Forces a value of a form item to satisfy a certain regular expression**                                       |
| `UtcAfter`                         | **Restricts the lower boundary (exclusive) of a UTC date time value of a form item**                            |
| `UtcAfterOrEqual`                  | **Restricts the lower boundary (inclusive) of a UTC date time value of a form item**                            |
| `UtcBefore`                        | **Restricts the upper boundary (exclusive) of a UTC date time value of a form item**                            |
| `UtcBeforeOrEqual`                 | **Restricts the upper boundary (inclusive) of a UTC date time value of a form item**                            |

## Property annotations — form layout and visibility

| Annotation                         | Description                                                                                                     |
|------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| `FormGroup`                        | **Specifies the form group a form item belongs to**                                                             |
| `SortPosition`                     | **Defines the sort order of a form item within its form group**                                                 |
| `VisibleIf`                        | **Specifies the visibility condition for a form item**                                                          |
| `FormGroupVisibleIf`               | **Specifies the visibility condition of a form group**                                                          |
| `FormItemVisibility`               | **Specifies the visibility level of a form item (`Basic`, `Advanced`, `Expert`, `Hidden`)**                     |

A form item without a `FormGroup` does not appear in the configuration form at all.

`VisibleIf` and `FormGroupVisibleIf` take a `VisibleIfOperator`: `Equal`, `NotEqual`,
`GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `OneOf`.

## Page template only

| Annotation                         | Description                                                                                                     |
|------------------------------------|-----------------------------------------------------------------------------------------------------------------|
| `UIComponentSlot`                  | **Declares a page template slot: `slotId`, `minimumItems`, `maximumItems`, `allowedUiComponentTypes`, `deniedUiComponentTypes`** |

`UIComponentSlot` does not create an ordinary data property. It replaces the field with a
computed property that reads the slot's components from the page context, and declares the
`pageContext` prop the runtime fills in.

## Recommended annotation order

The Smint.io component library writes annotations in this order. Following it keeps large
components readable:

1. `DisplayName` — default culture first
2. `Description`
3. `Implements` (when the type is not primitive)
4. `ComponentProperty`
5. type and constraint annotations (`Is...`, `MaxLength`, `AllowedValues`, `AllowedValueDisplayName`, `DynamicAllowedValuesProvider`)
6. `DefaultValue`
7. `VisibleIf` / `FormItemVisibility`
8. `FormGroup`
