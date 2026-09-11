Description
===========

This repository contains examples and tools for rapid developing of custom components targeting the Smint.io Portals solution.

Please find guides about Smint.io Portals component development here:

- [What are Smint.io Portals components?](Overview/)

- [Developing frontend components](Examples/Frontend/)

	- Page templates
	- UI components
	
- [Developing backend components](Examples/Backend/)

	- Connectors
	- Data adapters
	- Data processors
	- Task handlers
	- Portal templates
	- Identity providers

	- [The connector meta-model](Examples/Backend/docs/smintio-connector-metamodel.md) — describing the external system's schema
	- [The asset data model](Examples/Backend/docs/smintio-asset-data-model.md) — what a data adapter returns

This is the tools you need for developing Smint.io Portals components:

- The [Data Adapter Exporter CLI tool](Tools/Portals-DataAdapter-SDK-DataAdapterExporter-CLI/Release/) is used to generate TypeScript public API interface definitions from backend component assemblies
- The [Portals-SDK-PublishComponent-CLI tool](Tools/Portals-SDK-PublishComponent-CLI/Release/) is used to compile, package and deploy components
- The [Portals-DevServer](Tools/Portals-DevServer/Release/) is used for local distribution of frontend components

Please note that future tools may be added or existing ones may be updated based on user feedback.

Current version of this document is: 1.1.0 (as of 11th of September, 2026)

Contributors
============

- Yosif Velev, Smint.io GmbH
- Reinhard Holzner, Smint.io GmbH