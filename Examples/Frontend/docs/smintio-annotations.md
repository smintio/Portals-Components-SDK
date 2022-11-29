Annotations
-----------

Current version of this document is: 1.0.0 (as of 29th of November, 2022)

| Annotation                         | Description                                                                                                                    |
|------------------------------------|--------------------------------------------------------------------------------------------------------------------------------|
| `AllowedDateTime`                  | **Restricts allowed date time values**                                                                                         |
| `AllowedUtcDateTimes`              | **Restricts multiple allowed UTC date time values**                                                                            |
| `AllowedValue`                     | **Restricts to an allowed value**                                                                                              |
| `AllowedValueDisplayName`          | **Specifies an allowed value display name in different languages**                                                             |
| `AllowedValueDescription`          | **Specifies an allowed value description in different languages**                                                              |
| `AllowedValues`                    | **Restricts to multiple allowed values**                                                                                       |
| `DefaultValue`                     | **Specifies the default value**                                                                                                |
| `Description`                      | **Specifies the description for the form item in different languages**                                                         |
| `DisableTrimming`                  | **Disables the trimming of strings which is done by default**                                                                  |
| `DisplayName`                      | **Specifies the display name of the form item in different languages**                                                         |
| `DynamicAllowedValuesProvider`     | **Specifies the name of the provider of dynamic allowed value lists**                                                          |
| `FormGroup`                        | **Specifies the form group the form item belongs to**                                                                          |
| `FormGroupDeclaration`             | **Defines a new form group (displayed as separate tab or section, depending on the viewer implementation)**                    |
| `FormGroupDescription`             | **Specifies the description of the form group in different languages**                                                         |
| `FormGroupDisplayName`             | **Specifies the display name of the form group in different languages**                                                        |
| `FormGroupVisibleIf`               | **Specifies the visibility condition for the form group**                                                                      |
| `FormItemVisibility`               | **Specifies the visibility level for the form item**                                                                           |
| `IsBoolean`                        | **Specifies for the form item to be of boolean data type (auto-detected if possible)**                                         |
| `IsColor`                          | **Specifies for the string form item to be of color data type**                                                                |
| `IsDataAdapter`                    | **Specifies for the form item to reference a data adapter public API interface (auto-detected if possible)**                   |
| `IsDataAdapterArray`               | **Specifies for the form item to reference an array of  data adapter public API interfaces (auto-detected if possible)**       |
| `IsDate`                           | **Specifies for the form item to be of date data type (auto-detected if possible)**                                            |
| `IsDecimal`                        | **Specifies for the form item to be of decimal data type (auto-detected if possible)**                                         |
| `IsEmailAddress`                   | **Specifies for the string form item to be an email address**                                                                  |
| `IsInt32`                          | **Specifies for the form item to be of int data type (auto-detected if possible)**                                             |
| `IsInt32Array`                     | **Specifies for the form item to be of int array data type (auto-detected if possible)**                                       |
| `IsInt64`                          | **Specifies for the form item to be of long data type (auto-detected if possible)**                                            |
| `IsInt64Array`                     | **Specifies for the form item to be of long array data type (auto-detected if possible)**                                      |
| `IsJson`                           | **Specifies for the string form item to be a JSON string**                                                                     |
| `IsPhoneNumber`                    | **Specifies for the string form item to be a phone number**                                                                    |
| `IsRichText`                       | **Specifies for the string form item to be rich text**                                                                         |
| `IsString`                         | **Specifies for the form item to be of string data type (auto-detected if possible)**                                          |
| `IsStringArray`                    | **Specifies for the form item to be of string array data type (auto-detected if possible)**                                    |
| `IsUri`                            | **Specifies for the string form item to be an URI**                                                                            |
| `MaxLength`                        | **Restricts the max length of a value**                                                                                        |
| `MaxValue`                         | **Restricts the upper boundary of a value**                                                                                    |
| `MinValue`                         | **Restricts the lower boundary of a value**                                                                                    |
| `RegularExpression`                | **Forces a value to satisfy a certain regular expression**                                                                     |
| `Required`                         | **Forces the user to set a value for the form item**                                                                           |
| `RequiredPermissions`              | **Enforces certain permissions to be present when calling a data adapter public API interface**                                |
| `SlotType`                         | **Defines the details of a page template slot**                                                                                |
| `SortPosition`                     | **Defines the sort order of a form item within its form group**                                                                |
| `UtcAfter`                         | **Restricts the lower boundary (exclusive) of a UTC date time value**                                                          |
| `UtcAfterOrEqual`                  | **Restricts the lower boundary (inclusive) of a UTC date time value**                                                          |
| `UtcBefore`                        | **Restricts the upper boundary (exclusive) of a UTC date time value**                                                          |
| `UtcBeforeOrEqual`                 | **Restricts the upper boundary (inclusive) of a UTC date time value**                                                          |
| `UtcDateTime`                      | **Specifies for the string form item to be of UTC date time data type**                                                        |
| `VisibleIf`                        | **Specifies the visibility condition for the form item**                                                                       |
---

Please note, that not all annotations are available for both frontend and backend components. 
Please get in touch at [support@smint.io](mailto:support@smint.io) if you are missing some annotation, or if you need a new one.