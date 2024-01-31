How-to implement the `Connector`
================================

Current version of this document is: 1.0.1 (as of 23rd of January, 2024)

## SharePoint `Connector` Basics

For Sharepoint (or rather: the Microsoft Graph API) offers OAuth2 authorization with several flows. In order to 
leverage user-specific access rules and to provide a fine-grained security context, we opted for the OAuth2 Authorization 
Code Flow.

The `OAuth2AuthenticationCodeFlowWithPKCEConnector` can be used for a scaffolding a OAuth2 flow. Note, that you can implement
any authentication flow, but this class gives you a prefab implementation of this specific flow. All the steps required
to finalize the authentication flow will be driven by Smint.io Portals (e.g. redirects), so you do not need to care about
that low level details at all.

## Authentication process

This section describes how to set up the `Connector` to use the OAuth2's Authorization Code flow. It is easiest to
extend the pre-existing `OAuth2AuthenticationCodeFlowWithPKCEConnector` and customize it to your specific needs. 

The Authorization Code Flow consists of two main steps:

1. obtain an Authorization Code
2. use the Authorization Code to obtain an Access Token and a Refresh Token
3. (use the refresh token to get a new access token)

The Smint.io Portals framework does all that for you. You just need to provide the detailed implementation of the
specific external system you're integrating to.

### Get the Authorization Code

**Step 1** usually involves displaying some sort of consent screen in the browser and sending the Authorization Code as
POST query parameter to the redirect url.

If the system you're trying to connect to has special requirements, you'll need to override the `GetRedirectUrlAsync`
method. For example Microsoft requires that the `/autorize` request be prefixed with the Azure-AD `tenant_id` :

```http request
https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/authorize...."
```

### Get the Access Token
**Step 2** then uses the (short-lived) Autorization code and obtains an Access Token and a Refresh Token.

Again, if your external system requires something special to be configured, as is the case with Microsoft, override
the `InitializeAuthorizationValuesAsync` method. Same as before, Microsoft requires the `tenant_id` to be included in
the URL as well as the `redirect_uri` to be included as POST parameter:

```c#
// the bootstrapAuthorizationValuesModel is populated by the Portals Framework
var identityServerUrl = GetIdentityServerUrl(bootstrapAuthorizationValuesModel);
var clientId = GetClientId(bootstrapAuthorizationValuesModel);
var code = GetCode(bootstrapAuthorizationValuesModel);
var originalRedirectUrl = GetOriginalRedirectUrl(bootstrapAuthorizationValuesModel);
var tenantId = GetTenantId(); // stored in the Configuration

var restSharpClient = new RestSharpClient(new Uri(identityServerUrl)); //stored in the Configuration
var request = new RestRequest($"/{tenantId}/oauth2/v2.0/token", Method.POST);

// Microsoft Identity Platform requires that the redirect_uri be present
request.AddParameter("client_id", clientId, ParameterType.GetOrPost);
request.AddParameter("code", code, ParameterType.GetOrPost);
request.AddParameter("grant_type", "authorization_code", ParameterType.GetOrPost);
request.AddParameter("redirect_uri", originalRedirectUrl, ParameterType.GetOrPost);
request.AddParameter("client_secret", GetClientIdAndSecret(bootstrapAuthorizationValuesModel).ClientSecret);

request.AcceptApplicationJson();

var postResponse = await restSharpClient.ExecuteTaskAsync<OAuth2GetAccessTokenResponse>(request)
    .ConfigureAwait(false);
```

Since the `ExecuteTaskAsync` is parameterized, the `response.Data` object will hold the model-bound token response. We simply need to populate the `bootstrapAuthorizationValuesModel`
with that data and return it: 
```c#
// error handling omitted for legibility
bootstrapAuthorizationValuesModel.AccessToken = postResponse.Data.AccessToken;
bootstrapAuthorizationValuesModel.RefreshToken = postResponse.Data.RefreshToken;
bootstrapAuthorizationValuesModel.ExpiresAt = postResponse.Data.ExpiresAt;
return bootstrapAuthorizationValuesModel
```

### Refresh the Access Token
**Step 3** is only necessary once the access token has expired resulting in the GraphAPI returning HTTP 401 errors. If you need some custom handling, simply override the
`RefreshAuthorizationValuesAsync` method: 

```c#
var refreshToken = GetRefreshToken(authorizationValuesModel);

var request = new RestRequest($"/{tenantId}/oauth2/v2.0/token", Method.POST);
request.AddParameter("client_id", clientId, ParameterType.GetOrPost);
request.AddParameter("refresh_token", refreshToken, ParameterType.GetOrPost);
request.AddParameter("grant_type", "refresh_token", ParameterType.GetOrPost);
request.AddParameter("redirect_uri", redirectUri, ParameterType.GetOrPost);
request.AddParameter("client_secret", secret);
request.AddParameter("scope", "sites.read.all offline_access");

var restSharpClient = new RestSharpClient(new Uri(identityServerUrl));

request.AcceptApplicationJson();

var postResponse = await restSharpClient.ExecuteTaskAsync<OAuth2GetAccessTokenResponse>(request)
                .ConfigureAwait(false);
```

## The connector meta-model

Generally speaking the meta-model describes what types of *objects* exist in the external system and what properties they have. In other words: the meta-model describes the _data_ that is delivered by Sharepoint. 

This meta-model is then used throughout Smint.io Portals to interpret the external metadata delivered by the external system (e.g. also custom metadata).

Each type of object is represented by one `EntityModel`. Sharepoint is rather simple in that regard, because it only has one type of object which is the `File` (similar to a desktop file system). As a resulting
there is only one `EntityModel`. Columns (i.e. custom fields) affect all files equally. For example, if we were to add a custom choice field "Mood", that indicates the mood prevalent in an image, it would also be 
possible for `*.docx` file to have a "Mood" field. 

###  Meta-model structure

The meta-model consists of a collection of `EntityModel` objects, and each `EntityModel` has a list of `Properties`. It could be compared to a C# class definition where each `EntityModel` would be one 
class, each `Property` a class member. So if we were to model a Sharepoint file in C#, we could write it as

```c#
public class SharepointFile {
    public string DisplayName {get;set;}
    public int LikeCount {get;set;}  
    public bool IsReadOnly {get;set;}
}
```
The above code would correspond to the following `EntityModel`: 

```c#
var spf = new EntityModel("SharepointFile",...);
spf.AddProperty("DisplayName", DataType.String, ...);
spf.AddProperty("LikeCount", DataType.Int32, ...);
spf.AddProperty("IsReadOnly", DataType.Boolean, ...);
_entityModel.AddEntity(spf);
```
Note that all the "`...`" are placeholders for a `LocalizedStringsModel` and are omitted for legibility.

The meta-model could also have more complex fields, for example, imagine it having an `Owner` field: 
```c#
public class SharepointFile {
    // ...
    public User Owner {get;set;}
}

public class User {
    public string Name {get;set;}
    public string Email {get;set;}
    public int Age {get;set;}
    // ...
}
```

Then there would have to be a second `EntityModel`,  corresponding to the `User` field, which would have `Name`, `Email`, etc. as properties:

```c#
var userEntityModel = new EntityModel("user",...);
userEntityModel.AddProperty("Name", DataType.String, ...);
userEntityModel.AddProperty("Email", DataType.String, ...);
userEntityModel.AddProperty("Age", DataType.Int32, ...);

//add the "User" prop to the root entity model:
spf.AddProperty("Owner", DataType.DataObject, userEntityModel.Key, ...);
```
By passing `userEntityModel.Key` as third parameter we specify that the model for `"Owner"` is defined in another entity model, namely `userEntityModel`.  

Similarily to the `MetamodelMessages` examples from [Connector description & flow](#user-content-connector-description--flow)

`EntityModel`, `EnumEntityModel` and their properties support resource localized translations by using `ResourceLocalizedStringsModel` instance instead of `LocalizedStringsModel`.

```c#
var spf = new EntityModel("SharepointFile",..., labels: new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_sharepoint_root_entity)));
spf.AddProperty("DisplayName", DataType.String, ..., labels: new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_sharepoint_root_entity_display_name)));
spf.AddProperty("LikeCount", DataType.Int32, ..., labels: new ResourceLocalizedStringsModel(nameof(MetamodelMessages.c_sharepoint_root_entity_like_count)));
// ...
```

### Get metadata from Sharepoint

The Sharepoint datatype for a metadata field is called `ColumnDefinition`, since all metadata fields are called columns: 

```c#
IEnumerable<ColumnDefinition> colDefs = await _sharepointClient.GetMetadataAsync(_siteId)
```

It should be noted that even at this point, Sharepoint metadata is a fair bit more detailed than what is currently implemented in Smint.io Portals. For example, Sharepoint has `Text` columns, 
that have attributes like `MaxLength`, `AllowMultiline` etc. which we cannot model in Portals at the moment. This is no problem though, as long as data is only read, and no input
validation is required. Please get in touch if you need more capabilities here.

Sharepoint only knows one "root entity model", but other more complex systems may distinguish between images, video files, thus have an `EntityModel` for each one of them.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH