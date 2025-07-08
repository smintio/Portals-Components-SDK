What are Smint.io Portals components?
=====================================

This README.md serves to clarify the general concept of Smint.io Portals components.

## Overview

When you create a Smint.io Portals portal, you're not just creating static HTML/JavaScript pages. 

Each Smint.io Portals portal is a *fully-fledged portal application* that runs on the *Smint.io Portals component framework*. Each portal consists of numerous *Smint.io Portals components* that can be configured and assembled like a Lego puzzle. The *Smint.io Portals component framework* provides administrators and users with a highly functional portal application that can be *customized to individual customer needs* at any time, without the need to hard-code content into a generic codebase.

Each *Smint.io Portals component* consists of different parts:
- The *Startup* class
- The *Configuration* class
- The component implementation
- Translatable resources (texts, ...)

### The *Startup* class

The *Startup* class of a Smint.io Portals component provides a static entry point into a Smint.io Portals component. It is discovered by the Smint.io Portals component framework at runtime and provides all necessary information we need to instantiate the component, for example:

- The name of the component
- A reference to the *Configuration* class of the component
- References to the translatable resources of the component
- Further component type specific information
- ...

Please note that there is no separate *Startup* class for Smint.io Portals frontend components. Smint.io Portals frontend components contain all the necessary information directly in the component's source code, added as annotations. When a Smint.io Portals frontend component is loaded, the system then generates the frontend component's *Startup* class on the fly.

[Here](../Examples/Backend/Connectors/Connector-SharePoint/SharepointConnectorStartup.cs) you find an example of a component's *Startup* class (this one is part of our *Microsoft SharePoint* connector)

### The *Configuration* class

The *Configuration* class of a Smint.io Portals component contains all settings that a user can adjust during component configuration in the Smint.io Portals backend.

When Smint.io Portals instantiates a component, it reads the user provided component configuration from the database and provides the component with a fully initialized instance of the *Configuration* class.

Please note that there is no separate *Configuration* class for Smint.io Portals frontend components. Smint.io Portals frontend components contain all the necessary information directly in the component's source code, added as annotations. When a Smint.io Portals frontend component is loaded, the system then generates the frontend component's *Configuration* class on the fly.

[Here](../Examples/Backend/Connectors/Connector-SharePoint/SharepointConnectorConfiguration.cs) you find an example of a component's *Configuration* class (this one is part of our *Microsoft SharePoint* connector)

### Types of components

- [Frontend components](../Examples/Frontend/)

	- Page templates
	- UI components
	
- [Backend components](../Examples/Backend/)

	- Connectors
	- Data adapters
	- Data processors
	- Task handlers
	- Portal templates
	- Identity providers

## Questions

Please do not hesitate to contact us at [support@smint.io](mailto:support@smint.io) if you run into any issues.

Contributors
============

- Reinhard Holzner, Smint.io GmbH