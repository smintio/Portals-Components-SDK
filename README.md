Description
===========

Current version of this document is: 1.4.1 (as of 15th of September, 2026)

This repository contains examples and tools for rapid developing of custom components targeting the Smint.io Portals solution.

Please find guides about Smint.io Portals component development here:

- [What are Smint.io Portals components?](Overview/)

- [Developing frontend components](Frontend/Legacy/)

	- Page templates
	- UI components
	- [Recipes: how do I …?](Frontend/Legacy/docs/smintio-frontend-recipes.md)

- [Developing backend components](Backend/)

	- Connectors
	- Data adapters
	- Data processors
	- Task handlers
	- Portal templates
	- Resources
	- Identity providers
	- [Recipes: how do I …?](Backend/docs/smintio-backend-recipes.md)

This is the tools you need for developing Smint.io Portals components:

- The [Data Adapter Exporter CLI tool](Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/) is used to generate TypeScript public API interface definitions from backend component assemblies
- The [Portals-SDK-PublishComponent-CLI tool](Tools/Portals-SDK-PublishComponent-CLI/Release/) is used to compile, package and deploy components
- The [Portals-DevServer](Tools/Legacy/Portals-DevServer/Release/) is used for local distribution of frontend components

Please note that future tools may be added or existing ones may be updated based on user feedback.

Contributors
============

- Reinhard Holzner, Smint.io GmbH
- Yosif Velev, Smint.io GmbH