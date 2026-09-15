Smint.io Portals identity providers
===================================

Current version of this document is: 1.0.1 (as of 15th of September, 2026)

An identity provider component federates the authentication of a portal's **end users** to an
external identity system. It is what stands behind single sign-on into a Smint.io Portal.

Smint.io ships generic OIDC and generic SAML implementations, and they cover most cases: a
customer with a standards-compliant identity provider needs configuration, not a component. You
write one of these when the authentication workflow itself is different — a proprietary
handshake, a non-standard discovery document, a provider that needs particular parameters
computed per login.

This component type authenticates **portal users**. It is not how Smint.io Portals
administrators sign in.

1. [The contract](#user-content-the-contract)
1. [`AuthorizationValuesModel` — what you fill in](#user-content-authorizationvaluesmodel--what-you-fill-in)
1. [Starting from OIDC](#user-content-starting-from-oidc)
1. [Starting from SAML](#user-content-starting-from-saml)
1. [Configuration mixins](#user-content-configuration-mixins)
1. [External user groups](#user-content-external-user-groups)
1. [Pass-through authentication](#user-content-pass-through-authentication)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## The contract

```C#
namespace SmintIo.Portals.IdentityProviderSDK.IdentityProviders
{
    public interface IIdentityProvider : IIdentityProviderComponent
    {
        Task PerformPostConfigurationChecksAsync(AuthorizationValuesModel authorizationValuesModel);
    }

    public interface IIdentityProviderStartup : IComponentStartup
    {
        LocalizedStringsModel SetupDocumentationUrl { get; }
    }
}
```

That is the whole surface. **One method**, and a startup that adds nothing to
`IComponentStartup` except a link to your setup instructions.

That method is doing more than its name suggests. `PerformPostConfigurationChecksAsync` is
called when an administrator saves the identity provider's configuration, and it has two jobs:

1. **validate** the configuration against the external identity system — fetch the discovery
   document, resolve the metadata URL, confirm the client id is known — and throw an
   `ExternalDependencyException` when it does not hold up;
2. **fill in the `AuthorizationValuesModel`** it is handed, which is what the platform uses from
   then on to actually run the login.

So the component is not called per login. It is called once, at configuration time, and what it
writes onto the model is the login behaviour.

Unlike a connector or a data processor, `IIdentityProviderStartup` has **no `Type` property** —
there is no classification of identity providers.

Supply `SetupDocumentationUrl`. Configuring single sign-on always requires work on the customer's
side, and a link to instructions is the difference between a configuration form an administrator
can complete and one they cannot.

## `AuthorizationValuesModel` — what you fill in

`SmintIo.Portals.IdentityProviderSDK.Models.AuthorizationValuesModel`:

| Member | Meaning |
|---|---|
| `IdentityServerUrl` | the authority — the base URL of the external identity system |
| `EndSessionEndpointUrl` | where to send the user to sign out. Some providers do not advertise this in discovery, and this is where you supply it |
| `RequiredScopes` | the scopes to request |
| `UsePkce` | run the authorization code flow with PKCE |
| `ClaimMappings` | how the external system's claims map to the ones the portal expects |
| `PeerEntityId` | the SAML peer entity id |
| `AcrValues`, `AcrValuesAppendix`, `TriggerAcrValuesAppendixByUrlParameter` | authentication context class values, and an appendix that can be switched on by a URL parameter — for a provider that has to be told *how* to authenticate, sometimes per link |
| `AdditionalParameters` | anything else the provider needs on the authorize request |
| `UsePassThroughAuthentication` | see [pass-through authentication](#user-content-pass-through-authentication) |
| `SkipEmailConfirmationCheck` | do not ask the user to confirm their email address — correct when the external system has already verified it |

Set only what the provider needs. Each of these changes the login for every user of every portal
using the configuration, so a value set speculatively is a bug waiting for a customer.

## Starting from OIDC

```C#
public class MyIdentityProvider : OidcIdentityProvider
{
    public MyIdentityProvider(MyIdentityProviderConfiguration configuration)
        : base(configuration)
    {
    }

    public override async Task PerformPostConfigurationChecksAsync(
        AuthorizationValuesModel authorizationValuesModel)
    {
        var configuration = (MyIdentityProviderConfiguration)GetOidcConfiguration();

        // fetch and validate the discovery document at configuration.Authority,
        // throwing ExternalDependencyException if it does not resolve

        authorizationValuesModel.IdentityServerUrl = configuration.Authority;
        authorizationValuesModel.RequiredScopes = new[] { "openid", "profile", "email" };
        authorizationValuesModel.UsePkce = true;
    }
}
```

`OidcIdentityProvider` (`…IdentityProviders.Prefab`) takes an
`IOidcIdentityProviderConfiguration`, exposes it through `GetOidcConfiguration()`, implements
external user group resolution, and leaves `PerformPostConfigurationChecksAsync` abstract. That
is all it does — the base class is thin on purpose.

**Validate the discovery document rather than trusting the URL.** An authority that is a typo, a
tenant the client is not registered in, or a provider that answers but does not offer the scopes
you need are all things that are trivial to detect here and painful to diagnose later, when the
symptom is a portal user who cannot sign in.

## Starting from SAML

`SamlIdentityProvider` is the same shape against `ISamlIdentityProviderConfiguration`, whose one
member is `MetadataUrl`. Resolve the metadata, confirm it parses, and set `PeerEntityId` and
`ClaimMappings` from it.

## Configuration mixins

Your configuration class implements whichever of these apply, on top of
`IComponentConfiguration`:

| Interface | Adds |
|---|---|
| `IOidcIdentityProviderConfiguration` | `ClientId` |
| `IOidcClientSecretIdentityProviderConfiguration` (: the above) | `ClientSecret` — for a confidential client |
| `ISamlIdentityProviderConfiguration` | `MetadataUrl` |
| `IExternalUserGroupsIdentityProviderConfiguration` | `ExternalUserGroupResolutionEnabled`, `ExternalUsersRead`, `ExternalUserLookupRetries` |
| `IPassThroughIdentityProviderConfiguration` | `UsePassThroughAuthentication` |

Everything else — the authority, the scopes, the claim mapping settings the administrator can
change — you declare yourself with
[the ordinary annotations](smintio-backend-annotations.md).

## External user groups

A portal frequently needs to know which groups a user belongs to *in the external system*,
because that is what drives their permissions. Implement
`IExternalUserGroupsIdentityProviderConfiguration` on your configuration and the base class does
the rest:

| Member | |
|---|---|
| `ExternalUserGroupResolutionEnabled` | the administrator's switch. Group resolution only runs when this is on |
| `ExternalUsersRead` | an `IExternalUsersRead` data adapter — the component that actually asks the external system. **A data adapter picker in the configuration form**, because the lookup is an integration concern, not an authentication one |
| `ExternalUserLookupRetries` | how many times to retry. A user who has just been created in the external system is often not visible to the API for a few seconds |

So group resolution is a collaboration: the identity provider authenticates, and a data adapter
implementing
[`IExternalUsersRead`](smintio-data-adapter-interfaces.md#user-content-collections-shares-and-the-rest)
answers `GetUserGroupMembershipAsync`. Keep them separate; do not try to resolve groups inside
the identity provider.

`ExternalUserLookupRetries` exists because of a real and recurring failure: a user signs in for
the first time, the identity system has them, the API that lists their groups does not yet, and
they land in the portal with no permissions. Default it to a small number rather than zero.

## Pass-through authentication

When `UsePassThroughAuthentication` is set, the access token the user obtained at login is
carried through to the external system, rather than the connector using its own service
credentials. Every user then reaches exactly what they are entitled to on the other side, and
nothing more.

This is an arrangement between two components: the identity provider declares it here, and a
connector implementing `IPassThroughAuthenticationConnector` names the identity provider keys it
accepts tokens from. Both halves are needed.

It is the right answer when the external system's own permission model should govern, and the
wrong one when the portal is meant to present a curated view that differs from what users see in
the external system.

## Things that are easy to get wrong

- **The component runs at configuration time, not per login.** If you are looking for a hook to
  run on every sign-in, there is none here — what you write onto the
  `AuthorizationValuesModel` *is* the per-login behaviour.
- **Validate against the external system and throw.** An identity provider that accepts any
  configuration produces a portal nobody can sign in to, and the error surfaces to end users
  rather than to the administrator who caused it.
- **Do not resolve user groups in the identity provider.** That is what the `ExternalUsersRead`
  data adapter is for.
- **Set `ExternalUserLookupRetries` to something greater than zero**, or first-time sign-ins
  will intermittently arrive with no groups.
- **Only set what the provider needs** on the `AuthorizationValuesModel`. `SkipEmailConfirmationCheck`
  in particular is a security decision: set it only when the external system genuinely verifies
  email addresses.
- **Check whether generic OIDC or generic SAML already does it.** A new identity provider
  component is justified by a non-standard workflow, not by a new customer.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
