Components
--------------

* [Generic components](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#generic-componentns)  
   * [Assets overview](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#assets-preview)  
   * [Banner with search bar](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#banner-with-search-bar)
   * [Category chooser](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#category-chooser)  
   * [Category carousel](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#category-carousel)  
   * [Collection details](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#collection-details)  
   * [Collections overview](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#collections-overview)  
   * [Color indicator with text](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#color-indicator-with-text)  
   * [Footer](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#footer)  
   * [Header](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#header)  
   * [Image](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#image)  
   * [Image with text](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#image-with-text)  
   * [Location](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#location)  
   * [Menu item chooser](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#menu-item-chooser)  
   * [Page title](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#page-title)  
   * [Search bar](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#search-bar)  
   * [Side menu](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#side-menu)  
   * [Text block](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#text-block)  
   * [Text block with up to 3 columns](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#text-block-with-up-to-3-columns)  
   * [Video](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#video)
* [Asset details components](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#asset-details-components)
  * [Action bar](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#action-bar)
  * [Asset preview](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#asset-preview)
  * [Details text](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#details-text)
  * [Metadata viewer](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#metadata-viewer)
  * [Tag viewer](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#tag-viewer)
* [Media center components](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#media-center-components)
   * [Collections quickview](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#collections-quickview)    
   * [Facet based search form](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#facet-based-search-form)  
   * [Search result](https://github.com/smintio/Portals-UIComponents-Overview/blob/main/docs/smintio-ui-components.md#search-result)  

Current version of this document is: 1.0.0 (as of 3rd of March, 2022)

## Generic components  

### Assets preview
This component shows multiple assets in a neat preview. The user can then easily download or collect assets directly from the preview.

![Assets preview](../images/components/assets-preview-with-action-bar.gif "Assets preview")

| Key                                              | Type                         |
|--------------------------------------------------|------------------------------|
| `smintio-ui-generic-assets-preview-1`            | `ui-type-assets-preview`     |

#### Props

| Prop name                | Type                    | Default     | Description                                                                                                                                                                                                                                                                                                                                                                                   |
|--------------------------|-------------------------|-------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `assetDetailPage`        | `IPageReference`        |             | **Asset detail page** <br/><br/> Choose a reference to the customizable asset detail page.                                                                                                                                                                                                                                                                                                    |
| `assetsReference`        | `IAssetsReferenceModel` |             | **Assets to display** <br/><br/> Choose individual assets, a folder or configure an asset search.                                                                                                                                                                                                                                                                                             |
| `approximateAssetHeight` | `string`                | `'one_row'` | **Masonry row height** <br/><br/> The masonry asset viewer adjusts the asset height automatically for the assets to nicely match to the masonry. However, you can give an approximate target asset height value here, and the algorithm will try to match it as good as possible.<br/><br/> *Allowed values:* `'half_row', 'three_quarter_row', 'one_row', 'one_and_a_half_rows', 'two_rows'` |
| `masonryGap`             | `string`                | `'medium'`  | **Masonry gap** <br/><br/>  *Allowed values:* `'small', 'medium', 'large', 'xlarge'`                                                                                                                                                                                                                                                                                                          |
---

### Banner with search bar

This component displays a banner with an optional title and a search bar.

![Banner with search bar](../images/components/banner-with-search-bar-video-fading-small.gif "Banner with search bar")

| Key                                      | Type             |
|------------------------------------------|------------------|
| `smintio-ui-generic-banner-searchbar-1`  | `ui-type-banner` |

#### Props

| Prop name                          | Type                         | Default    | Description                                                                                                                                                                                                                                                |
|------------------------------------|------------------------------|------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `bannerText`                       | `ILocalizedStringsModel`     |            | **Banner title** <br/><br/> The main title for the banner.                                                                                                                                                                                                 | 
| `hasSearchbar`                     | `boolean`                    | `true`     | **Show search bar** <br/><br/> If you enable this setting, the search bar input field will be shown.                                                                                                                                                       | 
| `searchBarAutoCompletion`          | `IAssetsSearch`              |            | **Data source for auto completion** <br/><br/> The data source to query the search bar auto completion suggestions from. <br/><br/> *Used IF:* `hasSearchbar === true`                                                                                     |
| `searchPage`                       | `IPageReference`             |            | **Search page** <br/><br/> The page that will be opened when the user confirms the search bar input. <br/><br/> *Used IF:* `hasSearchbar === true`                                                                                                         |
| `bannerHeight`                     | `string`                     | `'tall'`   | **Height** <br/><br/> Height of the banner. <br/><br/> *Allowed values:* `'low', 'medium', 'tall', 'very_tall', 'maximum'`                                                                                                                                 |
| `backgroundColor`                  | `string`                     |            | **Color** <br/><br/> Background color of the banner.                                                                                                                                                                                                       |
| `backgroundType`                   | `string`                     | `'none'`   | **Background type** <br/><br/> *Allowed values:* `'none', 'random_asset', 'fixed_asset', 'random_image', 'random_video_asset', 'fixed_video_asset', 'random_video'`                                                                                        | 
| `backgroundImageRandomAsset`       | `IAssetsReadRandom`          |            | **Random asset data source** <br/><br/> The data source to query the random asset for the background from. <br/><br/> *Used IF:* `['random_asset', 'random_video_asset'].include(backgroundType)`                                                          |
| `backgroundImageRandomResources`   | `IImage[]`                   |            | **Images from resources for random choice** <br/><br/> *Used IF:* `backgroundType === 'random_image'`                                                                                                                                                      |
| `backgroundVideoRandomResources`   | `IVideo[]`                   |            | **Videos from resources for random choice** <br/><br/> *Used IF:* `backgroundType === 'random_video'`                                                                                                                                                      |
| `imageAssetIdentifiers`            | `IAssetIdentifier[]`         |            | **Images assets for random choice** <br/><br/> *Used IF:* `backgroundType === 'fixed_asset'`                                                                                                                                                               |
| `videoAssetIdentifiers`            | `IAssetIdentifier[]`         |            | **Video assets for random choice** <br/><br/> *Used IF:* `backgroundType === 'fixed_video_asset'`                                                                                                                                                          |
| `backgroundPosition`               | `string`                     | `'center'` | **Assets viewport** <br/><br/> *Used IF:* `backgroundType !== 'none'` <br/><br/> *Allowed values:* ''top-left', 'center'`                                                                                                                                  |
| `automaticFading`                  | `boolean`                    | `false`    | **Automatically fade assets** <br/><br/> Determines if the background assets should automatically fade through assets <br/><br/> *Used IF:* `backgroundType !== 'none'`                                                                                    |
| `automaticFadingIntervalInSeconds` | `number`                     | `10`       | **Assets fading interval** <br/><br/> The interval in seconds to wait before fading to the next background asset.<br/><br/>  *Min value:* `1` <br/><br/> *Max value:* `300`<br/><br/>  *Used IF:* `backgroundType !== 'none' AND automaticFading === true` |
| `maxNumberOfRandomAssetsToFade`    | `number`                     | `3`        | **Maximum number of assets to query for fading** <br/><br/> *Min value:* `1` <br/><br/> *Max value:* `10`<br/><br/>  *Used IF:* `['random_asset', 'random_video_asset'].include(backgroundType) AND automaticFading === true`                              |
| `backgroundBrightness`             | `string`                     | `'light'`  | **Darken assets**<br/><br/> *Allowed values:* `'none', 'light', 'strong'` <br/><br/> *Used IF:* `backgroundType !== 'none'`                                                                                                                                |
| `bannerShadow`                     | `string`                     | `'none'`   | **Shadow at the bottom** <br/><br/> *Allowed values:* `'none', 'light', 'strong'` <br/><br/> *Used IF:* `backgroundType !== 'none'`                                                                                                                        |
| `transparentHeader`                | `boolean`                    | `false`    | **Is the header transparent when the page scroll is 0**                                                                                                                                                                                                    |

---

### Category chooser

This component displays a column-based category chooser for one or more categories.

![Category chooser](../images/components/category-chooser.png "Category chooser")

| Key                                      | Type                        |
|------------------------------------------|-----------------------------|
| `smintio-ui-generic-category-chooser-1`  | `ui-type-category-chooser`  |

#### Props

| Prop name           | Type          | Default     | Description                                                                           |
|---------------------|---------------|-------------|---------------------------------------------------------------------------------------|
| `categories`        | `ICategory[]` |             | **Categories**<br/><br/> The categories to display to the user.                       |
| `categoryGap`       | `string`      | `'default'` | **Gap between categories**<br/><br/> *Allowed values:* `'default', 'medium', 'large'` |
| `categoriesShadow`  | `string`      | `'none'`    | **Shadow**<br/><br/> *Allowed values:* `'none', 'light', 'strong'`                    |

#### Note

The component has margin on it's X axis intentionally because the components are centered inside.

---

### Category carousel

This component displays a category carousel for one or more categories.

![Category carousel](../images/components/category-slider.png "Category carousel")

| Key                                     | Type                      |
|-----------------------------------------|---------------------------|
| `smintio-ui-generic-category-slider-1`  | `ui-type-category-slider` |

#### Props

| Prop name            | Type          | Default     | Description                                                                                                                                                                                         |
|----------------------|---------------|-------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `categories`         | `ICategory[]` |             | **Categories**<br/><br/> The categories to display to the user.                                                                                                                                     |
| `categoryGap`        | `string`      | `'default'` | **Gap between categories**<br/><br/> *Allowed values:* `'default', 'medium', 'large'`                                                                                                               |
| `categoriesShadow`   | `string`      | `'none'`    | **Shadow**<br/><br/> *Allowed values:* `'none', 'light', 'strong'`                                                                                                                                  |
| `cycle`              | `boolean`     | `true`      | **Automatic sliding**<br/><br/> Determines if the carousel should automatically slide through the categories.                                                                                       |
| `intervalInSeconds`  | `number`      | `10`        | **Sliding interval**<br/><br/> The interval in seconds to wait before sliding to the next category.<br/><br/> *Min value:* `1` <br/><br/> *Max value:* `300`<br/><br/>  *Used IF:* `cycle === true` |


#### Note

The component has margin on it's X axis intentionally because the slide arrows are centered inside.

---

### Collection details

This component displays the details and the content of a collection.

![Collection details](../images/components/collection-details.png "Collection details")

| Key                                          | Type                           |
|----------------------------------------------|--------------------------------|
| `smintio-ui-generic-collection-details-1`    | `ui-type-collection-details`   |

#### Props

| Prop name             | Type              | Default | Description                                                                              |
|-----------------------|-------------------|---------|------------------------------------------------------------------------------------------|
| `assetDetailPage`     | `IPageReference`  |         | **Asset detail page**<br/><br/>Choose a reference to the customizable asset detail page. |

---

### Collections overview

This component displays an overview of all collections of the user.

![Collections overview](../images/components/collections-overview.png "Collections overview")

| Key                                          | Type                            |
|----------------------------------------------|---------------------------------|
| `smintio-ui-generic-collections-overview-1`  | `ui-type-collections-overview`  |

#### Props

| Prop name                 | Type               | Default | Description                                                                                      |
|---------------------------|--------------------|---------|--------------------------------------------------------------------------------------------------|
| `collectionDetailPage`    | `IPageReference`   |         | **Collections overview**<br/><br/>Choose a reference to the customizable collection detail page. |

---

### Color indicator with text

This component displays a color indicator with text to the left or to the right.

![Color](../images/components/color.png "Color indicator with text")

| Key                            | Type                       |
|--------------------------------|----------------------------|
| `smintio-ui-generic-color-1`   | `ui-type-text-with-color`  |

#### Props

| Prop name                   | Type                     | Default    | Description                                                                               |
|-----------------------------|--------------------------|------------|-------------------------------------------------------------------------------------------|
| `color`                     | `string`                 |            | **Color**                                                                                 |
| `colorPosition`             | `string`                 | `'left'`   | **Color position**<br/><br/> *Allowed values:* `'left', 'right'`                          | 
| `colorShadow`               | `string`                 | `'none'`   | **Shadow**<br/><br/> *Allowed values:* `'none', 'light', 'strong'`                        | 
| `colorTextGap`              | `string`                 | `'medium'` | **Gap between color and text**<br/><br/> *Allowed values:* `'default', 'medium', 'large'` | 
| `contentVerticalAlignment`  | `string`                 | `'top'`    | **Vertical alignment of content**<br/><br/> *Allowed values:* `'top', 'middle', 'bottom'` | 
| `headerText`                | `ILocalizedStringsModel` |            | **Header**                                                                                |
| `subHeaderText`             | `ILocalizedStringsModel` |            | **Sub header**                                                                            |
| `continuousText`            | `ILocalizedStringsModel` |            | **Continuous text**                                                                       |
| `alignment`                 | `string`                 | `'left'`   | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`                     | 
---

### Footer

This component displays a page footer.

![Footer](../images/components/footer.png "Footer")

| Key                            | Type              |
|--------------------------------|-------------------|
| `smintio-ui-generic-footer-1`  | `ui-type-footer`  |

#### Props

| Prop name    | Type            | Default | Description                                        |
|--------------|-----------------|---------|----------------------------------------------------|
| `menuItems`  | `IMenuItem[]`   |         | **Menu items**<br/><br/>The menu items to display. |

#### Colors

| Color name        | Default                                                                    | CSS variable             | Description                     |
|-------------------|----------------------------------------------------------------------------|--------------------------|---------------------------------|
| Footer text       | ![#FFFFFF](https://via.placeholder.com/15/FFFFFF/000000?text=+) `#FFFFFF`  | `--v-s-header-base`      | Background color of the footer  |
| Footer background | ![#000000](https://via.placeholder.com/15/000000/000000?text=+) `#000000`  | `--v-s-header-text-base` | Color of the text in the footer |

---

### Header

This component displays a page header.

![Header](../images/components/header.png "Header")

| Key                            | Type              |
|--------------------------------|-------------------|
| `smintio-ui-generic-header-1`  |  `ui-type-header` |

#### Props

| Prop name                | Type          | Default     | Description                                                                                                                                                                                              |
|--------------------------|---------------|-------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `headerHeight`           | `string`      | `'medium'`  | **Height**<br/><br/>*Allowed values:* `'low', 'medium', 'tall'`                                                                                                                                          |
| `separatorStyle`         | `string`      | `'border'`  | **Height**<br/><br/>*Allowed values:* `'elevation', 'border', 'elevation-and-border', 'none'`                                                                                                            |
| `menuItemsLeft`          | `IMenuItem[]` |             | **Menu items to the left**<br/><br/>The menu items to display to the left.                                                                                                                               |
| `menuItemsRight`         | `IMenuItem[]` |             | **Menu items to the right**<br/><br/>The menu items to display to the right.                                                                                                                             |
| `maxSubMenuDepth`        | `string`      | `'1'`       | **Maximum menu depth**<br/><br/>Menus can have many sub menu levels. Here you can specify how many sub menu levels will be shown at a maximum. <br/><br/> *Allowed values:* `'0', '1', '2', '3', 'none'` | 
| `useShortLanguagesNames` | `boolean`     | `false`     | **Use short language names**<br/><br/>Use short character codes for language chooser (eg. EN, DE).                                                                                                       | 

#### Colors

| Color name        | Default                                                                    | CSS variable             | Description                      |
|-------------------|----------------------------------------------------------------------------|--------------------------|----------------------------------|
| Header background | ![#FFFFFF](https://via.placeholder.com/15/FFFFFF/000000?text=+) `#FFFFFF`  | `--v-s-header-base`      | Background color of the header   |
| Header text       | ![#000000](https://via.placeholder.com/15/000000/000000?text=+) `#000000`  | `--v-s-header-text-base` | Color of the text in the header  |
| Header accent     | ![#000000](https://via.placeholder.com/15/FF0000/000000?text=+) `#FF0000`  | `--v-accent-base`        | Color of the bottom border       |

---

### Image

This component displays an image.

![Image](../images/components/image.png "Image")

| Key                             | Type            |
|---------------------------------|-----------------|
| `smintio-ui-generic-image-1`    | `ui-type-image` |

#### Props

| Prop name          | Type               | Default       | Description                                                                                                                               |
|--------------------|--------------------|---------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `imageSource`      | `string`           | `'resource'`  | **Type**<br/><br/>*Allowed values:* `'asset', 'resource'`                                                                                 |
| `image`            | `IImage`           |               | **Image to display from resources**<br/><br/>*Used ID:* `imageSource==='resource'`                                                        |
| `assetIdentifier`  | `IAssetIdentifier` |               | **Image asset to display**<br/><br/>*Used ID:* `imageSource==='asset'`                                                                    |
| `imageAlignment`   | `string`           | `'center'`    | **Alignment**<br/><br/>*Allowed values:* `'left', 'center', 'right'`                                                                      |
| `imageWidth`       | `number`           | `75`          | **Width**<br/><br/>*Allowed values:* `50, 58, 66, 75, 83, 92, 100, 105, 110, 120`                                                         |
| `imageHeightLimit` | `string`           | `'one_row'`   | **Height limit**<br/><br/>*Allowed values:* `'half_row', 'three_quarters_row', 'one_row', 'one_and_a_half_rows', 'two_rows', 'unlimited'` |
| `imageDisplayMode` | `string`           | `'contained'` | **Display mode**<br/><br/>*Allowed values:* `'contained', 'cut_to_fit'`                                                                   |
| `imageShadow`      | `string`           | `'none'`      | **Display mode**<br/><br/>*Allowed values:* `'none', 'light', 'strong'`                                                                   |
---

### Image with text

This component displays an image with text to the left or to the right.

![Image with text](../images/components/image-with-text.png "Image with text")

| Key                                     | Type                       |
|-----------------------------------------|----------------------------|
| `smintio-ui-generic-image-with-text-1`  | `ui-type-image-with-text`  |

#### Props

| Prop name                   | Type                     | Default       | Description                                                                                                                              |
|-----------------------------|--------------------------|---------------|------------------------------------------------------------------------------------------------------------------------------------------|
| `imageSource`               | `string`                 | `'resource'`  | **Type**<br/><br/>*Allowed values:* `'asset', 'resource'`                                                                                |
| `image`                     | `IImage`                 |               | **Image to display from resources**<br/><br/>*Used ID:* `imageSource==='resource'`                                                       |
| `assetIdentifier`           | `IAssetIdentifier`       |               | **Image asset to display**<br/><br/>*Used ID:* `imageSource==='asset'`                                                                   |
| `allowAssetUserInteraction` | `boolean`                | `false`       | **Allow user interaction with the asset (e.g. collect & download, visit asset detail page)**<br/><br/>*Used ID:* `imageSource==='asset'` |
| `imagePosition`             | `string`                 | `left`        | **Position**<br/><br/>*Allowed values*: `left`, `right`                                                                                  |
| `imageWidth`                | `number`                 | `50`          | **Width**<br/><br/>*Allowed values:* `25, 33, 50, 66, 75`                                                                                |
| `imageHeightLimit`          | `string`                 | `'one_row'`   | **Height limit**<br/><br/>*Allowed values:* `'one_row', 'one_and_a_half_rows', 'two_rows', 'unlimited'`                                  |
| `imageDisplayMode`          | `string`                 | `'contained'` | **Display mode**<br/><br/>*Allowed values:* `'contained', 'cut_to_fit'`                                                                  |
| `imageShadow`               | `string`                 | `'none'`      | **Shadow**<br/><br/>*Allowed values:* `'none', 'light', 'strong'`                                                                        |
| `gapAroundImage`            | `string`                 | `'none'`      | **Gap around image**<br/><br/>*Allowed values:* `'none', 'light', 'strong'`                                                              |
| `imageTextGap`              | `string`                 | `'medium'`    | **Gap between image and text**<br/><br/>*Allowed values:* `'default', 'medium', 'large'`                                                 |
| `contentVerticalAlignment`  | `string`                 | `'top'`       | **Vertical alignment of content**<br/><br/> *Allowed values:* `'top', 'middle', 'bottom'`                                                |
| `assetDetailPage`           | `IPageReference`         |               | **Asset detail page** <br/><br/> Choose a reference to the customizable asset detail page.                                               |
| `headerText`                | `ILocalizedStringsModel` |               | **Header**                                                                                                                               |
| `subHeaderText`             | `ILocalizedStringsModel` |               | **Sub header**                                                                                                                           |
| `continuousText`            | `ILocalizedStringsModel` |               | **Continuous text**                                                                                                                      |
| `alignment`                 | `string`                 | `'left'`      | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`                                                                    | 
---

### Location

This component displays a location.

![Location](../images/components/location.png "Location")

| Key                                | Type                |
|------------------------------------|---------------------|
| `smintio-ui-generic-location-1`    | `ui-type-location`  |

#### Props

| Prop name                  | Type                     | Default         | Description                                                                                                                                 |
|----------------------------|--------------------------|-----------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| `shouldShowAddress`        | `boolen`                 | `true`          | **Show address**                                                                                                                            |
| `headerText`               | `ILocalizedStringsModel` |                 | **Header**<br/><br/> *Used IF:* `shouldShowAddress === true`                                                                                |
| `subHeaderText`            | `ILocalizedStringsModel` |                 | **Sub header**<br/><br/> *Used IF:* `shouldShowAddress === true`                                                                            |
| `continuousText`           | `ILocalizedStringsModel` |                 | **Continuous text**<br/><br/> *Used IF:* `shouldShowAddress === true`                                                                       |
| `alignment`                | `string`                 | `'left'`        | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`<br/><br/> *Used IF:* `shouldShowAddress === true`                     |
| `mapTextGap`               | `string`                 | `'medium'`      | **Gap between image and Map**<br/><br/> *Allowed values:* `'default', 'medium', 'large'`<br/><br/> *Used IF:* `shouldShowAddress === true`  |
| `contentVerticalAlignment` | `string`                 | `'top'`         | **Vertical alignment of content**<br/><br/> *Allowed values:* `'top', 'middle', 'bottom'`<br/><br/> *Used IF:* `shouldShowAddress === true` |
| `shouldShowMap`            | `boolen`                 | `true`          | **Show map**                                                                                                                                |
| `longitude`                | `number`                 |                 | **Longitude**<br/><br/> *Used IF:* `shouldShowMap === true`                                                                                 |
| `latitude`                 | `number`                 |                 | **Latitude**<br/><br/> *Used IF:* `shouldShowMap === true`                                                                                  |
| `mapProvider`              | `string`                 | `openstreetmap` | **Map provider**<br/><br/>*Allowed values*: `openstreetmap`, `google-maps`<br/><br/> *Used IF:* `shouldShowMap === true`                    |
| `googleMapsToken`          | `number`                 |                 | **Google Maps Token**<br/><br/> *Used IF:* `shouldShowMap === true`                                                                         |
| `mapPosition`              | `string`                 | `'left'`        | **Map position**<br/><br/>*Allowed values:* `'right', 'right', 'top', 'bottom'`<br/><br/> *Used IF:* `shouldShowMap === true`               |
| `mapWidth`                 | `number`                 | `75`            | **Width**<br/><br/>*Allowed values:* `25, 33, 50, 66, 75` <br/><br/> *Used IF:* `shouldShowMap === true`                                    |
| `mapHeight`                | `string`                 | `normal`        | **Height**<br/><br/>*Allowed values:* `'low', 'normal', 'big'` <br/><br/> *Used IF:* `shouldShowMap === true`                               |
| `mapZoom`                  | `number`                 | `16`            | **Map zoom**<br/><br/>*Allowed values:* `20, 18, 16, 13, 10`<br/><br/> *Used IF:* `shouldShowMap === true`                                  |
| `mapShadow`                | `string`                 | `'none'`        | **Shadow**<br/><br/>*Allowed values:* `'none', 'light', 'strong'`<br/><br/> *Used IF:* `shouldShowMap === true`                             |

---

### Menu item chooser

This component displays a column-based menu item chooser for one or more menu items and sub menu items.

![Menu item chooser](../images/components/menu-item-chooser.png "Menu item chooser")

| Key                                      | Type                       |
|------------------------------------------|----------------------------|
| `smintio-ui-generic-menu-item-chooser-1` | `ui-type-category-chooser` |

#### Props

| Prop name                  | Type          | Default             | Description                                                                                                                                                                                                |
|----------------------------|---------------|---------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `menuItems`                | `IMenuItem[]` |                     | **Menu items**<br/><br/>The menu items to display to the user.                                                                                                                                             |
| `alwaysShowSubMenuItems`   | `boolean`     | `false`             | **Always show sub menu items**<br/><br/>By default, sub menu items are only shown if the user interacts with the parent menu item. However, you can turn on this option to always show the sub menu items. |
| `colorScheme`              | `string`      | `'category-viewer'` | **Color scheme**<br/><br/> *Allowed values:* `'category-viewer', 'solid-button'`                                                                                                                           |
| `menuGap`                  | `string`      | `'small`            | **Gap between menu items**<br/><br/> *Allowed values:* `'no-gap', 'small', 'medium', 'large'`                                                                                                              |
| `menusShadow`              | `string`      | `'none'`            | **Shadow**<br/><br/> *Allowed values:* `'none', 'light', 'strong'`                                                                                                                                         | 
| `alignment`                | `string`      | `'left'`            | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`                                                                                                                                      | 
| `contentVerticalAlignment` | `string`      | `'top'`             | **Vertical alignment of content**<br/><br/> *Allowed values:* `'top', 'middle', 'bottom'`                                                                                                                  |  

---

### Page title

This component displays a page title and subtitle.

![Page title](../images/components/page-title.png "Page title")

| Key                                      | Type                   |
|------------------------------------------|------------------------|
| `smintio-ui-generic-page-title-1`        | `ui-type-page-title`   |

#### Props

| Prop name        | Type                     | Default  | Description                                                           |
|------------------|--------------------------|----------|-----------------------------------------------------------------------|
| `headerText`     | `ILocalizedStringsModel` |          | **Header**                                                            |
| `subHeaderText`  | `ILocalizedStringsModel` |          | **Sub header**                                                        |
| `continuousText` | `ILocalizedStringsModel` |          | **Continuous text**                                                   |
| `alignment`      | `string`                 | `'left'` | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'` | 

---

### Search bar

This component displays a search bar for assets.

![Search bar](../images/components/search-bar.png "Search bar")

| Key                                | Type                  |
|------------------------------------|-----------------------|
| `smintio-ui-generic-search-bar-1`  | `ui-type-search-bar`  |

#### Props

| Prop name                 | Type                      | Default | Description                                                                                                             |
|---------------------------|---------------------------|---------|-------------------------------------------------------------------------------------------------------------------------|
| `searchBarText`           | `ILocalizedStringsModel`  |         | **Hint for the search bar**                                                                                             |
| `searchBarAutoCompletion` | `IAssetsSearch`           |         | **Data source for auto completion**<br/><br/> The data source to query the search bar auto completion suggestions from. |
| `searchPage`              | `IPageReference`          |         | **Search page**<br/><br/> The page that will be opened when the user confirms the search bar input.                     |

---

### Side menu

This component displays a multi level side menu.

![Side menu](../images/components/side-menu.png "Side menu")

| Key                              | Type                 |
|----------------------------------|----------------------|
| `smintio-ui-generic-side-menu-1` | `ui-type-side-menu`  |

#### Props

| Prop name      | Type            | Default      | Description                                        |
|----------------|-----------------|--------------|----------------------------------------------------|
| `menuItems`    | `IMenuItem[]`   |              | **Menu items**<br/><br/>The menu items to display. |

---


### Text block

This component displays a text block with header, sub header and continuous text.

![Text block](../images/components/text-block.png "Text block")

| Key                         | Type           |
|-----------------------------|----------------|
| `smintio-ui-generic-text-1` | `ui-type-text` |

#### Props

| Prop name        | Type                     | Default   | Description                                                                                                   |
|------------------|--------------------------|-----------|---------------------------------------------------------------------------------------------------------------|
| `headerText`     | `ILocalizedStringsModel` |           | **Header**                                                                                                    |
| `subHeaderText`  | `ILocalizedStringsModel` |           | **Sub header**                                                                                                |
| `continuousText` | `ILocalizedStringsModel` |           | **Continuous text**                                                                                           |
| `alignment`      | `string`                 | `'left'`  | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`                                         | 
| `textWidth`      | `number`                 | `100`     | **Width**<br/><br/> *Allowed values:* `25, 33, 50, 66, 75, 100`                                               |
| `textPosition`   | `string`                 | `'left'`  | **Position**<br/><br/>  *Used IF:* `textWidth < 100` <br/><br/> *Allowed values:* `'left', 'center', 'right'` | 

---

### Text block with up to 3 columns

This component displays a text block with up to 3 columns.

![Text block with up to 3 columns](../images/components/text-columns.png "Text block with up to 3 columns")

| Key                                 | Type           |
|-------------------------------------|----------------|
| `smintio-ui-generic-text-columns-1` | `ui-type-text` |

#### Props

| Prop name          | Type                     | Default  | Description                                                                                                                                     |
|--------------------|--------------------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| `numberOfColumns`  | `number`                 | `1`      | Allowed values: `1, 2, 3`                                                                                                                       |
| `useSingleHeader`  | `boolean`                | `false`  | **Use same header for all columns**                                                                                                             |
| `headerText`       | `ILocalizedStringsModel` |          | **Header**<br/><br/> *Used IF:* `useSingleHeader === true`                                                                                      |
| `subHeaderText`    | `ILocalizedStringsModel` |          | **Sub header**<br/><br/> *Used IF:* `useSingleHeader === true`                                                                                  |
| `alignment`        | `string`                 | `'left'` | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`<br/><br/> *Used IF:* `useSingleHeader === true`                           | 
| `headerTextC1`     | `ILocalizedStringsModel` |          | **Header**<br/><br/> *Used IF:* `useSingleHeader === false`                                                                                     |
| `subHeaderTextC1`  | `ILocalizedStringsModel` |          | **Sub header**<br/><br/> *Used IF:* `useSingleHeader === false`                                                                                 |
| `continuousTextC1` | `ILocalizedStringsModel` |          | **Continuous text**                                                                                                                             |
| `alignmentC1`      | `string`                 | `'left'` | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`<br/><br/> *Used IF:* `useSingleHeader === false`                          |
| `headerTextC2`     | `ILocalizedStringsModel` |          | **Header**<br/><br/> *Used IF:* `useSingleHeader === false AND numberOfColumns >= 2`                                                            |
| `subHeaderTextC2`  | `ILocalizedStringsModel` |          | **Sub header** <br/><br/> *Used IF:* `useSingleHeader === false AND numberOfColumns >= 2`                                                       |
| `continuousTextC2` | `ILocalizedStringsModel` |          | **Continuous text**<br/><br/> *Used IF:* `numberOfColumns >= 2`                                                                                 |
| `alignmentC2`      | `string`                 | `'left'` | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`<br/><br/> *Used IF:* `useSingleHeader === false AND numberOfColumns >= 2` |
| `headerTextC3`     | `ILocalizedStringsModel` |          | **Header**<br/><br/> *Used IF:* `useSingleHeader === false AND numberOfColumns >= 3`                                                            |
| `subHeaderTextC3`  | `ILocalizedStringsModel` |          | **Sub header**<br/><br/> *Used IF:* `useSingleHeader === false AND numberOfColumns >= 3`                                                        |
| `continuousTextC3` | `ILocalizedStringsModel` |          | **Continuous text**<br/><br/> *Used IF:* `numberOfColumns >= 3`                                                                                 |
| `alignmentC3`      | `string`                 | `'left'` | **Alignment**<br/><br/> *Allowed values:* `'left', 'center', 'right'`<br/><br/> *Used IF:* `useSingleHeader === false AND numberOfColumns >= 3` |
---


### Video

This component displays a video.

![Video](../images/components/video.png "Video")

| Key                          | Type            |
|------------------------------|-----------------|
| `smintio-ui-generic-video-1` | `ui-type-video` |

#### Props

| Prop name            | Type               | Default       | Description                                                                                                                               |
|----------------------|--------------------|---------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `videoType`          | `string`           | `'resource'`  | **Type**<br/><br/>*Allowed values:* `'asset', 'resource'`                                                                                 |
| `videoResource`      | `IVideo`           |               | **Video to display from resources**<br/><br/>*Used ID:* `videoType==='resource'`                                                          |
| `assetIdentifier`    | `IAssetIdentifier` |               | **Video asset to display**<br/><br/>*Used ID:* `videoType==='asset'`                                                                      |
| `videoAlignment`     | `string`           | `'center'`    | **Alignment**<br/><br/>*Allowed values:* `'left', 'center', 'right'`                                                                      |
| `videoWidth`         | `number`           | `75`          | **Width**<br/><br/>*Allowed values:* `50, 58, 66, 75, 83, 92, 100, 105, 110, 120`                                                         |
| `videoShadow`        | `string`           | `'none'`      | **Display mode**<br/><br/>*Allowed values:* `'none', 'light', 'strong'`                                                                   |
---

## Asset details components

### Action bar

This asset details action bar component is best suited for media centers. It was originally designed for the Imagination media center.

![Action bar](../images/components/asset-actions-bar.png "Action bar")

| Key                                                   | Type                               |
|-------------------------------------------------------|------------------------------------|
| `smintio-ui-media-gallery-asset-details-action-bar-1` | `ui-type-asset-details-action-bar` |

---

### Asset preview

This asset preview component for the asset details view is best suited for media centers. It was originally designed for the Imagination media center.

![Asset preview](../images/components/asset-preview.png "Asset preview")

| Key                                                | Type                            |
|----------------------------------------------------|---------------------------------|
| `smintio-ui-media-gallery-asset-details-preview-1` | `ui-type-asset-details-preview` |

---

### Details text

This details text component for the asset details view is best suited for media centers. It was originally designed for the Imagination media center.

![Details text](../images/components/asset-text.png "Details text")

| Key                                             | Type                         |
|-------------------------------------------------|------------------------------|
| `smintio-ui-media-gallery-asset-details-text-1` | `ui-type-asset-details-text` |

#### Props

| Prop name                      | Type                         | Default | Description                                                                                                                                                                                            |
|--------------------------------|------------------------------|---------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `nameAttribute`                | `IMetadataAttributeModel[]`  |         | **Name attributes**<br/><br/>Please select the metadata attributes that contain the name of the asset. If you give no setting, defaults will be used. The first match will be shown.                   |
| `descriptionAttribute`         | `IMetadataAttributeModel[]`  |         | **Description attribute**<br/><br/>Please select the metadata attributes that contain the description of the asset. If you give no setting, defaults will be used. The first match will be shown.      |
| `displayDescriptionNewlines`   | `boolean`                    | `true`  | **Display description newlines**<br/><br/>If you enable this setting, newlines contained in the description text will be displayed. Otherwise, newlines contained in the description will be ignored.  |

---

### Metadata viewer

This metadata viewer component for the asset details view is best suited for media centers. It was originally designed for the Imagination media center.

![Metadata viewer](../images/components/asset-metadata-viewer.png "Metadata viewer")

| Key                                                        | Type                                    |
|------------------------------------------------------------|-----------------------------------------|
| `smintio-ui-media-gallery-asset-details-metadata-viewer-1` | `ui-type-asset-details-metadata-viewer` |

#### Props

| Prop name                      | Type                          | Default | Description                                                                                                                                                                                     |
|--------------------------------|-------------------------------|---------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `headerText`                   | `ILocalizedStringsModel`      |         | **Header**                                                                                                                                                                                      |
| `expandedByDefault`            | `boolean`                     | `true`  | **Expanded by default**<br/><br/>Check this to make the metadata viewer expansion panel expanded by default.                                                                                    |
| `metadataAttributeDisplayList` | `IMetadataAttributeModel[]`   |         | **Attributes to display**<br/><br/>Please select all the metadata attributes that the user should see. If you specify no attributes, only the basic attributes of the asset will be displayed.  |

---

### Tag viewer

This tag viewer component for the asset details view is best suited for media centers. It was originally designed for the Imagination media center.

![Tag viewer](../images/components/asset-tags.png "Tag viewer")

| Key                                                   | Type                               |
|-------------------------------------------------------|------------------------------------|
| `smintio-ui-media-gallery-asset-details-tag-viewer-1` | `ui-type-asset-details-tag-viewer` |

#### Props

| Prop name                   | Type                        | Default      | Description                                                                                                                                                                                                         |
|-----------------------------|-----------------------------|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `keywordAttributeList`      | `IMetadataAttributeModel[]` |              | **Tag attributes**<br/><br/>Please select all the metadata attributes that tags should be queried from. All the tags will then be added to one list in alphabetical order, duplicates being removed in the process. |
| `automaticSplitting`        | `boolean`                   | `true`       | **Automatic splitting**<br/><br/>If automatic splitting is enabled, texts will be automatically split into individual keywords if separators (comma, semicolon) are being detected.                                 |

---

## Media center components

### Collections quickview

This collections quickview component is best suited for media centers. It was originally designed for the Imagination media center.

![Collections quickview](../images/components/collections-quickview.png "Collections quickview")

| Key                                                | Type                            |
|----------------------------------------------------|---------------------------------|
| `smintio-ui-media-gallery-collections-quickview-1` | `ui-type-collections-quickview` |

#### Props

| Prop name             | Type               | Default | Description                                                                                |
|-----------------------|--------------------|---------|--------------------------------------------------------------------------------------------|
| `assetDetailPage`     | `IPageReference`   |         | **Asset detail page** <br/><br/> Choose a reference to the customizable asset detail page. |

---

### Facet based search form

This facet based search form is best suited for media centers. It was originally designed for the Imagination media center.

![Facet based search form](../images/components/search-facet-form.png "Facet based search form")

| Key                                            | Type                  |
|------------------------------------------------|-----------------------|
| `smintio-ui-media-gallery-search-form-facet-1` | `ui-type-search-form` |

#### Props

| Prop name                      | Type             | Default | Description                                                                                                                                                                                                                                                                 |
|--------------------------------|------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `formItemAllowList`            | `string[]`       |         | **Allowed search fragments**<br/><br/>Please select all the search fragments that the user is allowed to search for. If you give none, the user is allowed to search for all available search fragments.                                                                    |
| `formItemDenyList`             | `string[]`       |         | **Not allowed search fragments**<br/><br/>Optionally here you can specify search fragments, that the user dedicatedly is not allowed to search for.                                                                                                                         |
| `searchGroupsOfAdvancedMode`   | `string[]`       |         | **Advanced search fragments**<br/><br/>Here you can specify a list of advanced or rarely used search fragments. Those fragments will be collapsed by default. This helps improving usability of the search form.                                                            |
| `hideUncheckedValuesGroupIds`  | `string[]`       |         | **Hide choices by default for search fragments**<br/><br/>Only applies to multiple choice search fragments. Here you can specify a list of search fragments, that will show available choices only once the user entered some search term to the fragment search input box. |
| `formItemDoNotSortList`        | `string[]`       |         | **Non-sorted search fragments**<br/><br/>If you do not want to sort values of search fragments, you can specify that search fragments here.                                                                                                                                 |
| `showAllowedValueResultCount`  | `boolean`        |         | **Show result count for values** <br/><br/> If you want to show the result count for values of search fragments, please check this option.                                                                                                                                  |
| `assetDetailPage`              | `IPageReference` |         | **Asset detail page** <br/><br/> Choose a reference to the customizable asset detail page.                                                                                                                                                                                  |
---

### Search result

This search result display component is best suited for media centers. It was originally designed for the Imagination media center.

![Search result](../images/components/search-result.png "Search result")

| Key                                        | Type                    |
|--------------------------------------------|-------------------------|
| `smintio-ui-media-gallery-search-result-1` | `ui-type-search-result` |

#### Props

| Prop name                | Type             | Default     | Description                                                                                                                                                                                                                                                                                                                   |
|--------------------------|------------------|-------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `approximateAssetHeight` | `string`         | `'default'` | **Asset row height** <br/><br/> The asset viewer adjusts the asset height automatically for the assets to nicely match to the layout. However, you can give an approximate target asset height value here, and the algorithm will try to match it as good as possible.<br/><br/> *Allowed values:* `'low', 'default', 'high'` |
| `assetGap`               | `string`         | `'small'`   | **Asset gap** <br/><br/> *Allowed values:* `'small', 'medium', 'large', 'xlarge'`                                                                                                                                                                                                                                             |
| `assetDetailPage`        | `IPageReference` |             | **Asset detail page** <br/><br/> Choose a reference to the customizable asset detail page.                                                                                                                                                                                                                                    |
---


Contributors
============

- Yanko Belov, Smint.io. GmbH
- Reinhard Holzner, Smint.io GmbH
