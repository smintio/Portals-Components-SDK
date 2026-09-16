# Administrator-side work for Smint.io Portals

This directory covers the configuration that carries logic — **custom forms** and **Dynamic
Content Routing** — as opposed to components, which are built and published from `../Backend/`
and `../Frontend/`. It is written for an agent doing the work, and for the developer reading over
its shoulder.

| | |
|---|---|
| `README.md` | what administrator-side work is, and where the boundary with component development runs |
| `CustomForms/README.md` | the full contract: attachment points, the JSON shape, data types, allowed values, conditional visibility, the API |
| `DynamicContentRouting/README.md` | what routing is for, the two scripts, configuration, design guidance, troubleshooting |
| `DynamicContentRouting/docs/smintio-dcr-script-reference.md` | every function available to a script, what it returns, what it does to the request |
| `DynamicContentRouting/docs/smintio-dcr-recipes.md` | worked, paired scripts for the rules that come up |

Nothing here is compiled or deployed. A change takes effect as soon as it is saved, on a live
portal, so **there is no build step to catch a mistake** — which is why the rules below are worded
as absolutes.

## Read before writing anything

| Document | Read it when |
|---|---|
| `CustomForms/README.md` | always, before designing a routing scheme — the form determines what the rule can say |
| `DynamicContentRouting/docs/smintio-dcr-recipes.md` | you know the rule and need the shape. Check it before writing a mechanism from scratch |
| `DynamicContentRouting/docs/smintio-dcr-script-reference.md` | you need the exact contract of a function, or the engine's limits |
| `../Backend/docs/smintio-data-processors.md` | you are asked to *build* a data processor rather than configure routing |

## The rule that overrides everything else

**Never implement only the prepare asset search script.**

Narrowing a search hides assets from one route to them. It does not protect them. Assets are
reached without a search by direct link, by collection, by share, as a related asset or rendition,
by a random-asset widget, by a download request, and by any caller that builds its own request.
A hidden search fragment is a hidden *control*, not a permission — omitting it from a request is
trivial.

So: every rule expressed in the prepare script has a counterpart in the validate asset access
script, and the two agree. If you are asked to write "just the search part", write both and say
why. If a rule genuinely belongs only to the search — a language or relevance filter that is not
an entitlement — say so explicitly in a comment in the script, so the next reader knows the
omission was a decision.

Write the **validate script first**. It is the security boundary. The prepare script is the
experience layer that keeps users from meeting it.

## The interview to run before writing a routing scheme

Settle these before the first line. Guessing any of them produces a scheme that has to be rebuilt.

1. **What dimension are we routing on, in the asset's own metadata?** Name the field, the entity
   and the connector instance. If the answer is "we will add a field", the routing project starts
   in the source system, not here.
2. **Is it an enum (taxonomy) or a plain string?** This decides `getEnumKeyValueByPath` versus
   `getStringValueByPath`, and whether the custom form carries keys or free text.
3. **What does an asset with no value in that field mean?** Almost always: denied. Confirm it.
4. **What does a user with no setting mean?** Almost always: sees nothing. Confirm it, then use a
   sentinel rather than skipping the filter.
5. **Is the dimension additive?** More values must mean more access, because a user's values are
   unioned across their portal user groups. A "maximum level" field does not compose.
6. **Which scope carries it — user group, page, or data adapter?** Anything that has to be
   enforced must be on the **user group**: page values are not available when an asset is read
   directly or downloaded.
7. **Does the routing configuration serve more than one data adapter or connector?** If so the
   script needs a switch on `getConnectorInstanceKey()` or `getDataAdapterInstanceKey()`, and the
   entity keys differ per instance.
8. **Should the dimension stay visible as a filter?** If yes, bound it with
   `addLimitFormFieldValueStringArrayValues`; if no, set the value and
   `addNotAllowedSearchFragment`.
9. **Does the portal offer folder navigation?** If so, every value the routing sets needs the
   `"DCR"` hint, or the folder tree advertises folders the user cannot open anything in.

## The short version — what is most often got wrong

- **Search fragment ids are abbreviated; asset data object paths are not.** `___SF.` /
  `___CR.` / `___TC_` in a search id become `___SharepointFile.` / `___CloudinaryResource.` /
  `___ThronContent_` in a path. Getting this wrong fails silently — the search half works, the
  access half reads `null`, and the scheme leaks or denies everything. Check both halves of every
  paired script for it before anything else.
- **A setter with an unknown id does nothing and reports nothing.** It adds a value nothing reads.
  Read every fragment id from the portal's own filter panel; never derive one.
- **Match enums on keys, never on display names.** Display names are localized and editable.
- **Let absence deny.** `!assetValues || assetValues.filter(…).length === 0` is the correct shape.
  Testing only the overlap lets an asset with no value through.
- **An empty allow-list must mean "nothing".** Substitute a sentinel that matches no asset. "No
  restriction found, so no restriction applied" is how a half-configured tenant leaks.
- **`permissionDenied()` does not stop the script.** It records the outcome; execution continues
  and a later statement can overwrite it. Always `return;` immediately after.
- **The validate script runs once per asset, and the first denial fails the whole call.** It does
  not filter the offending asset out of a batch. A rule meant to hide individual assets from a
  list belongs in the search script, with the validate script as the backstop.
- **512 statements, 500 ms, 512 KB, no recursion, no network, no `await`.** A loop counts every
  iteration. Build small arrays; do not iterate large ones.
- **A user-scope custom form read is always an array**, merged across the user's groups, and
  `null` when there are no values. Even for a `string` or `boolean` item.
- **`setRequestPageConfigurationUuid` exists only in the download flow.** Guard with
  `getMethodName() !== "GetAssetsDownloadItemMappingsAsync"` and return.
- **Custom form values on a user group reach that user's browser.** Nothing confidential goes in
  one.
- **One custom form per tenant per attachment point.** Groups differ in their values, never in
  their fields. A request for "different fields for this user group" is a request for conditional
  visibility, or for a different value.
- **A custom form item id is a contract.** Renaming it silently breaks every script that reads it.
- **Keep the two scripts structurally parallel.** Same order, same variable names, same custom
  form reads, so a reviewer can put them side by side and see they say the same thing. Most
  drift bugs are introduced by editing one half.

## Verifying a routing scheme

There is no test harness. Verify by reasoning and by observation, in this order:

1. **Read the two scripts side by side.** Every custom form id read in one is read in the other.
   Every dimension restricted in one is enforced in the other. Entity keys are abbreviated in one
   and expanded in the other.
2. **Trace the unconfigured user.** A user whose groups carry no values at all: what do they see?
   If the answer is "everything", the scheme is wrong.
3. **Trace the asset with no metadata.** An asset missing the routed field: is it denied? If not,
   the scheme is wrong.
4. **Print the values with `error(JSON.stringify(…)); return;`**, reproduce once, read the message
   in the portal, then **remove the probe**. This is the only diagnostic channel an administrator
   has, and it works by failing the request — see below.
5. **Check a direct asset link and a download**, not only the search page. That is the half of the
   scheme a search test cannot exercise.

On diagnostics, because it is easy to advise the wrong thing here:

- **`error(message)` is the administrator's channel.** The message is carried into the error the
  portal displays. `JSON.stringify` is available, so any value can be forced into view. It is the
  right answer whenever someone asks how to debug a routing script.
- **It fails the request and it is public.** The search returns nothing while the line is there,
  and the message is shown to whoever triggered the request, including ordinary portal visitors.
  Always tell the user to remove it, and never leave one in a script you hand over.
- **`debug()` and the extended logging setting both write to the Smint.io platform log, which
  portal administrators cannot read.** Do not suggest either as a debugging step. Extended logging
  is enabled at Smint.io support's request so that *support* can read the decision context; frame
  it that way, as the escalation path after `error()` has not answered the question.

## Writing for this directory

These documents are read by Smint.io Solution Partners and Enterprise-plan customers. The rules in
[the repository root `CLAUDE.md`](../CLAUDE.md) apply in full, and two of them bite here
particularly hard:

- **No customer names, tenant names, portal URLs, real UUIDs, taxonomy keys or folder paths.**
  Routing scripts are drawn from real deployments and are full of all six. Anonymise every
  identifier before it goes into a document: neutral connector instance numbers, readable
  placeholder keys, invented folder paths.
- **Describe what the platform does, never name what does it.** Name the functions a script author
  writes against and the API endpoints they call; do not name the platform classes that execute
  them.

Where new material goes: a **contract** — what a function returns, what the API accepts — belongs
in the reference document that owns that area. A **task** — how you express a particular rule, end
to end — belongs in `DynamicContentRouting/docs/smintio-dcr-recipes.md` as a
`## How do I …?` recipe with both scripts and the mistakes it invites. Do not explain the same
mechanism in both places; the reference links to the recipe and the recipe links back.

Every recipe carries **both halves**. A recipe with only a prepare script teaches the one mistake
this whole directory exists to prevent.
