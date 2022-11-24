<template>
    <div>
        <v-row align="center" justify="center">
            <v-col v-if="componentText" cols="10" class="component-content">
                <h1 class="hw-heading" :style="{ 'color': componentColor }">
                    {{ componentText | resolve_localized }}
                </h1>
            </v-col>
        </v-row>
    </div>
</template>

<script lang="ts">
// vue
import { Mixins } from "vue-property-decorator";
import type {
    ILocalizedStringsModel,
    IPortalsContext,
} from "@smintio/portals-component-sdk";
import {
    ComponentProperty,
    DefaultCulture,
    Description,
    DisplayName,
    DynamicAllowedValuesProvider,
    FormGroup,
    FormGroupDeclaration,
    FormGroupDisplayName,
    Implements,
    IsColor,
    PortalsGlobalServices,
    PortalsInject,
    PortalsUiComponent,
} from "@smintio/portals-component-sdk";

import { SCssProps } from "@smintio/portals-components";

@PortalsUiComponent({
    type: "ui-type-text",
    key: "smintio-ui-example-hello-world-1",
    displayName: {
        [DefaultCulture]: "Hello world",
        de: "Hallo Welt",
    },
    description: {
        [DefaultCulture]: "This component displays a message with an optional color.",
        de: "Diese Komponente dient zur Darstellung eines Banners mit optionalem Titel und einer Suchleiste.",
    },
})
@FormGroupDeclaration("hw-text")
@FormGroupDisplayName("hw-text", "en", "Text", true)
@FormGroupDeclaration("hw-color")
@FormGroupDisplayName("hw-color", "en", "Color", true)
@FormGroupDisplayName("hw-color", "de", "Farbe")
export default class PortalsUiComponentImplementation extends Mixins<SCssProps>(SCssProps) {
    @PortalsInject(PortalsGlobalServices.PortalsContext)
    public readonly portalsContext!: IPortalsContext;

    @DisplayName("en", "Component text", true)
    @DisplayName("de", "Komponententext")
    @Description("en", "The component text.", true)
    @Description("de", "Der Komponententext.")
    @Implements("ILocalizedStringsModel")
    @ComponentProperty({ name: "componentText" })
    @DynamicAllowedValuesProvider(
        PortalsGlobalServices.PortalsContext.toString(),
        "StringResourceAllowedValuesProvider"
    )
    @FormGroup("hw-text")
    public readonly componentText!: ILocalizedStringsModel;

    @DisplayName("en", "Color", true)
    @DisplayName("de", "Farbe")
    @Description("en", "The component text color.", true)
    @Description("de", "Die Textfarbe der Komponente.")
    @ComponentProperty({ name: "componentColor" })
    @IsColor()
    @FormGroup("hw-color")
    public readonly componentColor!: string;
}
</script>

<style lang="scss" scoped>
    .hw-heading {
        text-align: center;
    }
</style>