import type { IResourceDefinitionBuilder, IFormFieldValuesModel } from "@smintio/portals-resource-builder-cli";

export default (builder: IResourceDefinitionBuilder): Promise<void> =>
    builder.buildUIComponentResourceDefinition(async (uiComponentBuilder) => {
        uiComponentBuilder.setFormFieldValues({
            values: [
                {
                    id: "componentText",
                    dataType: "resource_id",
                    resourceIdValue: "component_text",
                },
            ],
        } as IFormFieldValuesModel);

        uiComponentBuilder.setDefaultSettings(uiComponentBuilder.createDefaultSettings());

        await uiComponentBuilder.defineResources(async (resourceBuilder) => {
            await resourceBuilder.loadFileResources(async (fileResources) => {
                await fileResources.verifyLoadedResources(async (resource) => {
                    return resource;
                });
            });
        });
    });
