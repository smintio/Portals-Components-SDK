Smint.io Portals task handlers
=============================

Current version of this document is: 1.0.0 (as of 15th of September, 2026)

A task handler is a **state machine** that the Smint.io Portals task framework runs. It is the
component type behind approval workflows: a portal user asks for something, somebody decides,
and the portal acts on the decision.

Requesting access to a portal, requesting a permission, requesting a download of assets a user
may not take directly, submitting assets for review before they are created — each of those is a
task handler defining the states a request can be in, the actions available in each state, and
what happens when one is taken.

The contract lives in **`SmintIo.Portals.DataAdapterSDK.TaskHandlers`**, alongside the data
adapter SDK rather than in an SDK of its own, because a task handler is always paired with a
data adapter that stores the tasks.

1. [Two halves: the handler and the data adapter](#user-content-two-halves-the-handler-and-the-data-adapter)
1. [The contract](#user-content-the-contract)
1. [Task types](#user-content-task-types)
1. [States and actions](#user-content-states-and-actions)
1. [The prefab states and actions](#user-content-the-prefab-states-and-actions)
1. [The call sequence](#user-content-the-call-sequence)
1. [The specialised handler interfaces](#user-content-the-specialised-handler-interfaces)
1. [Who may create a task](#user-content-who-may-create-a-task)
1. [Things that are easy to get wrong](#user-content-things-that-are-easy-to-get-wrong)

## Two halves: the handler and the data adapter

| | |
|---|---|
| **The task handler** | the workflow. Which states exist, which actions each state offers, what taking one does |
| **A data adapter implementing `ITaskManagement…`** | the storage. Creating, reading, searching, updating and deleting the tasks themselves |

The handler declares which pairing it belongs to, statically, on its startup:

```C#
public string ConnectorKey   => MyConnectorStartup.MyConnector;
public string DataAdapterKey => MyTasksDataAdapterStartup.MyTasksDataAdapter;
```

The platform checks those keys when a request arrives for a task handler. This is unlike a
[data processor](smintio-data-processors.md), which an administrator attaches at configuration
time — a task handler's pairing is fixed in code.

Smint.io ships a tasks data adapter that stores tasks in the platform, and the shipped handlers
are bound to it. A handler bound to your own connector and your own tasks data adapter is
equally valid, and is what you want when the workflow's state must live in your system rather
than in Smint.io's.

## The contract

```C#
namespace SmintIo.Portals.DataAdapterSDK.TaskHandlers
{
    public interface ITaskHandler : ITaskHandlerComponent
    {
        Task<List<ITaskActionDescriptorModel>> GetTaskActionDescriptorsAsync(string taskState);

        Task<TaskPreActionResultModel> PerformTaskPreActionAsync(
            ITaskContextModel taskContextModel, string taskState,
            FormFieldValuesModel oldFormFieldValuesModel, bool isNew);

        Task<ITaskStateDescriptorModel> PerformTaskActionAsync(
            ITaskContextModel taskContextModel, string oldTaskState, string taskAction,
            FormFieldValuesModel oldFormFieldValuesModel, FormFieldValuesModel formFieldValuesModel,
            IProgressMonitor progressMonitor = null);

        Task<TaskPostActionResultModel> PerformTaskPostActionAsync(ITaskContextModel taskContextModel);
    }

    public interface ITaskHandlerStartup : IComponentStartup
    {
        string Type { get; }                                        // a TaskType constant
        string ConnectorKey { get; }
        string DataAdapterKey { get; }
        string MdiIcon { get; }
        List<ITaskStateDescriptorModel> TaskStateDescriptors { get; }
        List<ITaskActionDescriptorModel> TaskActionDescriptors { get; }
        ITaskActionDescriptorModel CreateTaskActionDescriptor { get; }
        List<ITaskActionDescriptorModel> GetTaskActionDescriptors(string taskState);
        bool AllowsAnonymousUsers { get; }
        bool AllowsRegisteredUsers { get; }
    }
}
```

`ITaskContextModel` tells you who is acting and on what: `TaskId`, `PerformerUserUuid`,
`PerformerEmailAddress`, `PerformerDisplayName`.

Note that `GetTaskActionDescriptors` appears **twice** — synchronously on the startup and
asynchronously on the handler. That is deliberate and it matters; see
[states and actions](#user-content-states-and-actions).

## Task types

`Type` classifies the handler. `SmintIo.Portals.DataAdapterSDK.TaskHandlers.Models.TaskType`:

| Constant | Value |
|---|---|
| `RequestAccess` | `task-type-request-access` |
| `RequestPermission` | `task-type-request-permission` |
| `RequestDownload` | `task-type-request-download` |
| `RequestGeneric` | `task-type-request-generic` |
| `UploadAssets` | `task-type-upload-assets` |
| `Generic` | `task-type-generic` |

The type is what the portal uses to find the handler for a given situation — which handler runs
when a user asks for access, which when they submit assets. Pick the one that describes the
situation; use `Generic` only when none of the specific ones does.

## States and actions

A state descriptor (`ITaskStateDescriptorModel`) names a state the task can be in. An action
descriptor (`ITaskActionDescriptorModel`) names something that can be done to a task. Both
derive from `ITaskDescriptorModel`, which carries the localized `Name` and `Description`, an
`MdiIcon`, and the `ConfigurationMessages` type the names are resolved from.

An action descriptor adds:

| Member | |
|---|---|
| `Action` | the action's identifier |
| `OpenExternalUrl` | taking the action sends the user to an external URL rather than transitioning in place |
| `ConfigurationImplementation` | a configuration class describing the **form the user fills in when taking this action** — a reason for declining, a comment, a date |

That last one is what makes a task handler more than a set of buttons: the decline action can
require a reason, the request action can collect what is being requested, all described with
[the ordinary annotations](smintio-backend-annotations.md).

**Which actions are available depends on the state**, and that question is asked in two places
for two different reasons:

| | Where | When |
|---|---|---|
| `ITaskHandlerStartup.GetTaskActionDescriptors(taskState)` | on the startup, **synchronous** | for a list of tasks — a search result with hundreds of rows. **It must be fast**: no I/O, decide purely from the state string |
| `ITaskHandler.GetTaskActionDescriptorsAsync(taskState)` | on the handler, **asynchronous** | for one task being looked at. Here you may consult the external system |

Implement both. Getting the fast one wrong is the difference between a task list that loads and
one that times out.

## The prefab states and actions

`…TaskHandlers.Prefab` ships the common ones, so that handlers agree on their vocabulary.

States: `TaskStateNewStateDescriptorModel`, `TaskStateApprovedStateDescriptorModel`,
`TaskStateDeclinedStateDescriptorModel`, `TaskStateIgnoredStateDescriptorModel`,
`TaskStateDoneStateDescriptorModel`, `TaskStateUploadedStateDescriptorModel`,
`TaskStateAssetsCreatedStateDescriptorModel`.

Actions: `TaskActionApproveActionDescriptorModel`, `TaskActionDeclineActionDescriptorModel`,
`TaskActionIgnoreActionDescriptorModel`, `TaskActionDoneActionDescriptorModel`,
`TaskActionSubmitActionDescriptorModel`, `TaskActionUploadActionDescriptorModel`,
`TaskActionCreateAssetsActionDescriptorModel`, `TaskActionDownloadActionDescriptorModel`,
`TaskActionRequestAccessActionDescriptorModel`,
`TaskActionRequestPermissionActionDescriptorModel`,
`TaskActionRequestDownloadActionDescriptorModel`.

With matching action configurations — `TaskApproveConfiguration`, `TaskDeclineConfiguration`,
`TaskIgnoreConfiguration`, `TaskDoneConfiguration`, `TaskCreateAssetsConfiguration` — and
initial configurations for the download and upload task types.

**Use these rather than defining your own equivalents.** A decline action that is your own type
rather than the prefab one looks different, is labelled differently, and does not match the rest
of the portal.

A minimal approval workflow is: states `New`, `Approved`, `Declined`, `Ignored`; actions
`Approve`, `Decline`, `Ignore` offered in `New` and nothing offered afterwards.

## The call sequence

```
task created or opened
   → PerformTaskPreActionAsync(context, taskState, oldFormFieldValues, isNew)
        returns TaskPreActionResultModel { Links }

user takes an action
   → PerformTaskActionAsync(context, oldTaskState, taskAction,
                            oldFormFieldValues, formFieldValues, progressMonitor)
        returns the new ITaskStateDescriptorModel — or null when the task is finished

   → PerformTaskPostActionAsync(context)
        returns TaskPostActionResultModel { Email }
```

- **`PerformTaskPreActionAsync`** runs before the user sees the task, with `isNew` telling you
  whether it is being created. Returning `Links` in the result is how you put a link in front of
  the user — the thing they are approving, the download they requested.
- **`PerformTaskActionAsync`** performs the transition and returns the state the task is now in.
  **Returning `null` means the task is finished** and there is no next state. It takes an
  optional `IProgressMonitor`, because an action may do real work — creating assets from an
  upload, preparing a download.
- **`PerformTaskPostActionAsync`** runs after a successful transition, and returning an
  `EmailModel` is how the workflow notifies someone. This is the notification hook; do not send
  mail yourself from inside the action.

## The specialised handler interfaces

Five interfaces extend `ITaskHandler` with the properties their task type needs. Implement the
one matching your `Type`:

| Interface | Adds |
|---|---|
| `IRequestAccessTaskHandler` | `Done`, `Declined`, `Reason` |
| `IRequestPermissionTaskHandler` | `Done`, `Declined`, `Reason` |
| `IRequestGenericTaskHandler` | `Done` |
| `IRequestDownloadTaskHandler` | `Done`, `Declined`, `Reason`, `DownloadUuid`, `ImmediateDownloadPossible`, `AssetIds`, `UserUuid`, `FirstName`, `LastName`, `EmailAddress`, `TransactionDate` |
| `IUploadAssetsTaskHandler` | `CreateAssets`, `Done`, `Declined`, `Reason`, `AssetUploads`, `UserUuid`, `FirstName`, `LastName`, `EmailAddress`, `TransactionDate` |

These are how the platform reads the *outcome* of a task without knowing your state names.
`Done` and `Declined` are what tell it the request was granted or refused, and `Reason` is what
the user is shown. A handler that transitions correctly but never sets them leaves the portal
unable to act on the decision.

`ImmediateDownloadPossible` on a download handler is worth calling out: set it when the download
can proceed without an approval step, so the user is not made to wait for a decision that will
always be yes.

## Who may create a task

```C#
public bool AllowsAnonymousUsers  => false;
public bool AllowsRegisteredUsers => true;
```

`CreateTaskActionDescriptor` is the action that creates a task, and its
`ConfigurationImplementation` is the form the requester fills in.

Think about the anonymous case explicitly. A request-access workflow is often exactly the thing
an anonymous visitor needs — they cannot sign in yet, that is what they are asking for — while a
request-permission workflow by definition applies to someone who is already signed in.

## Things that are easy to get wrong

- **`ITaskHandlerStartup.GetTaskActionDescriptors` must be fast.** It runs once per row of a
  task list. Decide from the state string alone; the async one on the handler is where a lookup
  belongs.
- **Returning `null` from `PerformTaskActionAsync` ends the task.** Return the new state
  descriptor for every transition that is not terminal.
- **Set `Done`, `Declined` and `Reason`.** They are how the platform and the portal read the
  outcome. Transitioning states without setting them is a workflow nobody can act on.
- **Send notifications from `PerformTaskPostActionAsync`**, by returning an `EmailModel` — not
  from inside the action, where a later failure would leave the mail sent and the transition
  undone.
- **Use the prefab state and action descriptors** instead of defining equivalents.
- **The connector and data adapter keys are fixed in code**, not configured. A handler declaring
  a pairing that does not exist is never reached.
- **Give every action a `ConfigurationImplementation` when it needs input**, rather than
  collecting a reason afterwards or not at all.
- **Decide the anonymous case deliberately** rather than leaving `AllowsAnonymousUsers` at its
  default.

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run
into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
