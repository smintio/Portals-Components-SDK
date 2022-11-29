Annotations
-----------

Current version of this document is: 1.0.0 (as of 29th of November, 2022)

| Annotation                         | Description                                                                                                                    |
|------------------------------------|--------------------------------------------------------------------------------------------------------------------------------|
| `AllowedDateTime`                  | **Gives an allowed date time value of a form item**                                                                            |
| `AllowedUtcDateTimes`              | **Gives multiple allowed UTC date time values of a form item**                                                                 |
| `AllowedValue`                     | **Gives an allowed value of a form item**                                                                                      |
| `AllowedValueDisplayName`          | **Specifies the display name of an allowed value of a form item in different languages**                                       |
| `AllowedValueDescription`          | **Specifies the description of an allowed value of a form item in different languages**                                        |
| `AllowedValues`                    | **Gives multiple allowed values of a form item**                                                                               |
| `DefaultValue`                     | **Specifies the default value of a form item**                                                                                 |
| `Description`                      | **Specifies the description of a form item in different languages**                                                            |
| `DisableTrimming`                  | **Disables the trimming of strings for a form item (which is done by default)**                                                |
| `DisplayName`                      | **Specifies the display name of a form item in different languages**                                                           |
| `DynamicAllowedValuesProvider`     | **Specifies the name of the provider of dynamic allowed value lists of a form item**                                           |
| `FormGroup`                        | **Specifies the form group a form item belongs to**                                                                            |
| `FormGroupDeclaration`             | **Defines a new form group (displayed as separate tab or section, depending on the viewer implementation)**                    |
| `FormGroupDescription`             | **Specifies the description of a form group in different languages**                                                           |
| `FormGroupDisplayName`             | **Specifies the display name of a form group in different languages**                                                          |
| `FormGroupVisibleIf`               | **Specifies the visibility condition of a form group**                                                                         |
| `FormItemVisibility`               | **Specifies the visibility level of a form item**                                                                              |
| `IsBoolean`                        | **Specifies for a form item to be of boolean data type (auto-detected if possible)**                                           |
| `IsColor`                          | **Specifies for a string form item to be of color data type**                                                                  |
| `IsDataAdapter`                    | **Specifies for a form item to reference a data adapter public API interface (auto-detected if possible)**                     |
| `IsDataAdapterArray`               | **Specifies for a form item to reference an array of  data adapter public API interfaces (auto-detected if possible)**         |
| `IsDate`                           | **Specifies for a form item to be of date data type (auto-detected if possible)**                                              |
| `IsDecimal`                        | **Specifies for a form item to be of decimal data type (auto-detected if possible)**                                           |
| `IsEmailAddress`                   | **Specifies for a string form item to be an email address**                                                                    |
| `IsInt32`                          | **Specifies for a form item to be of int data type (auto-detected if possible)**                                               |
| `IsInt32Array`                     | **Specifies for a form item to be of int array data type (auto-detected if possible)**                                         |
| `IsInt64`                          | **Specifies for a form item to be of long data type (auto-detected if possible)**                                              |
| `IsInt64Array`                     | **Specifies for a form item to be of long array data type (auto-detected if possible)**                                        |
| `IsJson`                           | **Specifies for a string form item to be a JSON string**                                                                       |
| `IsPhoneNumber`                    | **Specifies for a string form item to be a phone number**                                                                      |
| `IsRichText`                       | **Specifies for a string form item to be rich text**                                                                           |
| `IsString`                         | **Specifies for a form item to be of string data type (auto-detected if possible)**                                            |
| `IsStringArray`                    | **Specifies for a form item to be of string array data type (auto-detected if possible)**                                      |
| `IsUri`                            | **Specifies for a string form item to be an URI**                                                                              |
| `MaxLength`                        | **Restricts the max length of a value of a form item**                                                                         |
| `MaxValue`                         | **Restricts the upper boundary of a value of a form item**                                                                     |
| `MinValue`                         | **Restricts the lower boundary of a value of a form item**                                                                     |
| `RegularExpression`                | **Forces a value of a form item to satisfy a certain regular expression**                                                      |
| `Required`                         | **Forces the user to set a value for a form item**                                                                             |
| `RequiredPermissions`              | **Enforces certain permissions to be present when calling a data adapter public API interface**                                |
| `SlotType`                         | **Defines the details of a page template slot**                                                                                |
| `SortPosition`                     | **Defines the sort order of a form item within its form group**                                                                |
| `UtcAfter`                         | **Restricts the lower boundary (exclusive) of a UTC date time value of a form item**                                           |
| `UtcAfterOrEqual`                  | **Restricts the lower boundary (inclusive) of a UTC date time value of a form item**                                           |
| `UtcBefore`                        | **Restricts the upper boundary (exclusive) of a UTC date time value of a form item**                                           |
| `UtcBeforeOrEqual`                 | **Restricts the upper boundary (inclusive) of a UTC date time value of a form item**                                           |
| `UtcDateTime`                      | **Specifies for the string form item to be of UTC date time data type**                                                        |
| `VisibleIf`                        | **Specifies the visibility condition for a form item**                                                                         |
---

Please note, that not all annotations are available for both frontend and backend components. 
Please get in touch at [support@smint.io](mailto:support@smint.io) if you are missing some annotation, or if you need a new one.