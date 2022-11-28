Component types
---------------

* [UI component types](#ui-component-types)
* [UI component types for the login system](#ui-component-types-for-the-login-system)
* [Page types](#ui-page-types)
* [Page types for the login system](#ui-page-types-for-the-login-system)

## UI component types

| Name                               | Type                                                 | Description                                                |
|------------------------------------|------------------------------------------------------|------------------------------------------------------------|
| `Header`                           | `ui-type-header`                                     |   **Header**                                               |
| `Footer`                           | `ui-type-footer`                                     |   **Footer**                                               |                     
| `Banner`                           | `ui-type-banner`                                     |   **Banner**                                               |
| `PageTitle`                        | `ui-type-page-title`                                 |   **Page title**                                           |
| `Text`                             | `ui-type-text`                                       |   **Text block**                                           |
| `TextWithColor`                    | `ui-type-text-with-color`                            |   **Color indicator with text**                            |
| `SideMenu`                         | `ui-type-side-menu`                                  |   **Side menu**                                            |
| `CategoryChooser`                  | `ui-type-category-chooser`                           |   **Chooser**                                              |
| `SearchForm`                       | `ui-type-search-form`                                |   **Search form**                                          |
| `SearchBar`                        | `ui-type-search-bar`                                 |   **Search bar**                                           |
| `SearchResult`                     | `ui-type-search-result`                              |   **Search result display**                                |
| `AssetDetailsActionBar`            | `ui-type-asset-details-action-bar`                   |   **Action bar for asset details view**                    |
| `AssetDetailsPreview`              | `ui-type-asset-details-preview`                      |   **Asset preview for asset details view**                 |
| `AssetDetailsText`                 | `ui-type-asset-details-text`                         |   **Details text for asset details view**                  |
| `AssetDetailsTagViewer`            | `ui-type-asset-details-tag-viewer`                   |   **Tag viewer for asset details view**                    |
| `AssetDetailsMetadataViewer`       | `ui-type-asset-details-metadata-viewer`              |   **Metadata viewer for asset details view**               |
| `AssetsPreview`                    | `ui-type-assets-preview`                             |   **Asset overview**                                       |
| `Image`                            | `ui-type-image`                                      |   **Image**                                                |
| `ImageWithText`                    | `ui-type-image-with-text`                            |   **Image with text**                                      |
| `Video`                            | `ui-type-video`                                      |   **Video**                                                |
| `CollectionsOverview`              | `ui-type-collections-overview`                       |   **Collections overview**                                 |
| `CollectionsQuickview`             | `ui-type-collections-quickview`                      |   **Collections quickview**                                |
| `CollectionDetails`                | `ui-type-collection-details`                         |   **Collection details**                                   |
| `ShareDetails`                     | `ui-type-share-details`                              |   **Share link details**                                   |
| `CollectionComments`               | `ui-type-collection-comments`                        |   **Comments view**                                        |
| `SharedLinks`                      | `ui-type-shared-links`                               |   **Share links overview**                                 |
| `RequestAccessForm`                | `ui-type-request-access-form`                        |   **Request access form**                                  |
| `RequestPermissionForm`            | `ui-type-request-permission-form`                    |   **Request permission form**                              |
| `RequestDownloadForm`              | `ui-type-request-download-form`                      |   **Request download form**                                |
| `Location`                         | `ui-type-location`                                   |   **Location / address**                                   |
| `Custom`                           | `ui-type-custom`                                     |   **Custom component**                                     |
| `Section`                          | `ui-type-section`                                    |   **Section**                                              |
---

## UI component types for the login system

| Name                               | Type                                                 | Description                                               |
|------------------------------------|------------------------------------------------------|-----------------------------------------------------------|
| `AccountLogin`                     | `ui-type-account-login-form`                         |   **Account log in form**                                 |
| `AccountLogout`                    | `ui-type-account-logout-form`                        |   **Logout confirmation form**                            |
| `AccountManage`                    | `ui-type-account-manage-form`                        |   **Manage account form**                                 |
| `AccountManageChangePassword`      | `ui-type-account-manage-change-password-form`        |   **Change password form**                                |
| `AccountAccessDenied`              | `ui-type-account-access-denied-form`                 |   **Access denied display**                               |
| `AccountConfirmEmail`              | `ui-type-account-confirm-email-form`                 |   **Confirm email address form**                          |
| `AccountEmailConfirmationSent`     | `ui-type-account-email-confirmation-sent-form`       |   **Email confirmation message sent display**             |
| `AccountEmailNotConfirmed`         | `ui-type-account-email-not-confirmed-form`           |   **Email address not yet confirmed display**             |
| `AccountForgotPassword`            | `ui-type-account-forgot-password-form`               |   **Forgot password form**                                |
| `AccountForgotPasswordConfirmation`| `ui-type-account-forgot-password-confirmation-form`  |   **Password reset message sent display**                 |
| `AccountRegister`                  | `ui-type-account-register-form`                      |   **Registration form**                                   |
| `AccountResetPassword`             | `ui-type-account-reset-password-form`                |   **Reset password form**                                 |
| `AccountResetPasswordConfirmation` | `ui-type-account-reset-password-confirmation-form`   |   **Password has been reset display**                     |
| `AccountInvitationCollection`      | `ui-type-account-invitation-collection-form`         |   **Accept or decline invitation to collection form**     |
| `AccountAcceptTermsForm`           | `ui-type-account-accept-terms-form`                  |   **Accept terms form**                                   |
| `AccountCookieConsent`             | `ui-type-account-cookie-consent`                     |   **Cookie consent banner**                               |
| `AccountManageCookieConsent`       | `ui-type-account-manage-cookie-consent`              |   **Manage cookie consent**                               |
| `Error`                            | `ui-type-error-form`                                 |   **Application error prompt**                            |
---

## Page types

| Name                               | Type                                                 | Description                                               |
|------------------------------------|------------------------------------------------------|-----------------------------------------------------------|
| `Main`                             | `page-type-main`                                     |   **Cover page**                                          |
| `AssetsSearch`                     | `page-type-assets-search`                            |   **Search assets**                                       |
| `AssetDetails`                     | `page-type-asset-details`                            |   **Asset details**                                       |
| `CollectionsOverview`              | `page-type-collections-overview`                     |   **Collections overview**                                |
| `CollectionDetails`                | `page-type-collection-details`                       |   **Collection details**                                  |
| `SharedLinks`                      | `page-type-shared-links`                             |   **Share links overview**                                |
| `ShareDetails`                     | `page-type-share-details`                            |   **Share link details**                                  |
| `RequestAccess`                    | `page-type-request-access`                           |   **Request access**                                      |
| `RequestPermission`                | `page-type-request-permission`                       |   **Request permission**                                  |
| `RequestDownload`                  | `page-type-request-download`                         |   **Request download**                                    |
| `Generic`                          | `page-type-generic`                                  |   **Content page**                                        |
| `GenericDialog`                    | `page-type-generic-dialog`                           |   **Generic dialog**                                      |
---

## Page types for the login system

| Name                               | Type                                                 | Description                                               |
|------------------------------------|------------------------------------------------------|-----------------------------------------------------------|
| `AccountLogin`                     | `page-type-account-login`                            |   **Login**                                               |
| `AccountLogout`                    | `page-type-account-logout`                           |   **Logout**                                              |
| `AccountManage`                    | `page-type-account-manage`                           |   **Manage account**                                      |
| `AccountManageChangePassword`      | `page-type-account-manage-change-password`           |   **Change password**                                     |
| `AccountAccessDenied`              | `page-type-account-access-denied`                    |   **Access denied**                                       |
| `AccountConfirmEmail`              | `page-type-account-confirm-email`                    |   **Confirm email address**                               |
| `AccountEmailConfirmationSent`     | `page-type-account-email-confirmation-sent`          |   **Email confirmation message sent**                     |
| `AccountEmailNotConfirmed`         | `page-type-account-email-not-confirmed`              |   **Email address not yet confirmed**                     |
| `AccountForgotPassword`            | `page-type-account-forgot-password`                  |   **Forgot password**                                     |
| `AccountForgotPasswordConfirmation`| `page-type-account-forgot-password-confirmation`     |   **Password reset message sent**                         |
| `AccountRegister`                  | `page-type-account-register`                         |   **Register**                                            |
| `AccountResetPassword`             | `page-type-account-reset-password`                   |   **Reset password**                                      |
| `AccountResetPasswordConfirmation` | `page-type-account-reset-password-confirmation`      |   **Password has been reset**                             |
| `AccountInvitationCollection`      | `page-type-account-invitation-collection`            |   **Accept or decline invitation to collection**          |
| `AccountAcceptTerms`               | `page-type-account-accept-terms`                     |   **Accept terms**                                        |
| `AccountCookieConsent`             | `page-type-account-cookie-consent`                   |   **Cookie consent banner**                               |
| `Error`                            | `page-type-error`                                    |   **Error**                                               |
---

Please note that if a new type is required, you can contact us at [support@smint.io](mailto:support@smint.io)