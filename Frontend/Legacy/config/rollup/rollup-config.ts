/* eslint-disable @typescript-eslint/ban-ts-comment */
/**
 * The only working export seems to be roolup
 *
 * Weitere Infos:
 *      https://rollupjs.org/guide/en/
 *
 * Cheat-Sheet:
 *      https://devhints.io/rollup
 */
// NR: The name differs from origin rollup.config.js, to avoid accidental transpilation
// with tsc, which would overwrite the file rollup.config.js

import "tslib";
import path from "path";

import resolve from "@rollup/plugin-node-resolve";
import babel from "@rollup/plugin-babel";
import commonjs from "@rollup/plugin-commonjs";
import { terser } from "rollup-plugin-terser";
import typescript from "rollup-plugin-typescript2";
import vue from "rollup-plugin-vue";

import { DEFAULT_EXTENSIONS } from "@babel/core";

// @ts-ignore
export default (pkg, packagePath) => {
    return {
        input: "src/PortalsUiComponent.vue", // our source file
        output: [
            {
                name: ("" + pkg.name).replace(/^@smintio\//, "smintio-"),
                exports: "default",
                file: path.join(packagePath, pkg.main),
                format: "umd", // the preferred format
                sourcemap: process.env.BUILD !== "development",
                globals: {
                    moment: "moment",
                    Vue: "Vue",
                    vue: "Vue",
                    "@smintio/portals-component-sdk": "smintio-portals-component-sdk",
                    "@smintio/portals-core": "smintio-portals-core",
                    "@smintio/portals-components": "smintio-portals-components",
                    tslib: "tslib",
                    "vue-class-component": "VueClassComponent",
                    "vue-property-decorator": "VuePropertyDecorator",
                    VueClassComponent: "VueClassComponent",
                    VuePropertyDecorator: "VuePropertyDecorator",
                },
                intro: `
var exports = exports || {};
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = (function() {

        `,
                outro: `
})();
return exports;
        `,
            },
        ],
        external: [
            "@smintio/portals-component-sdk",
            "@smintio/portals-components",
            "@smintio/portals-core",
            "tslib",
            "vue",
            "vue-class-component",
            "vue-property-decorator",
            "vuex",
            "vuex-class",
            "vuetify",
            "vuetify/lib",
            "video.js",
            /^vuetify\/.*/,
        ],
        plugins: [
            resolve({ browser: true }),
            typescript({
                typescript: require("typescript"),
                tsconfig: path.join(packagePath, "tsconfig.json"),
                tsconfigOverride: {
                    compilerOptions: {
                        target: "es5",
                    },
                    exclude: ["node_modules", "tests", "src/main.ts"],
                },
                verbosity: 2,
            }),

            commonjs(),

            babel({
                babelHelpers: "runtime",
                exclude: "node_modules/**",
                extensions: [...DEFAULT_EXTENSIONS, ".ts", ".tsx"],

                ignore: ["./node_modules/@smintio/", "../../ui-components/", "./node_modules/@babel/"],
                plugins: [
                    [
                        "@babel/plugin-transform-runtime",
                        {
                            absoluteRuntime: true,
                            helpers: true,
                            regenerator: true,
                            useESModules: true,
                        },
                    ],
                ],
                presets: [
                    [
                        "@babel/preset-env",
                        {
                            useBuiltIns: "entry",
                            corejs: 3,
                            forceAllTransforms: true,
                        },
                    ],
                ],
            }),

            // [Rollup Plugin Vue](https://rollup-plugin-vue.vuejs.org/)
            vue({
                css: true, // Dynamically inject css as a <style> tag
                // @ts-ignore
                template: { isProduction: true },
            }),
        ].concat(
            (() => {
                if (process.env.BUILD !== "development" && process.env.BUILD !== "test") {
                    return [
                        terser({
                            toplevel: true,
                            module: true,
                        }), // minifies generated bundles
                    ];
                } else {
                    return [];
                }
            })()
        ),

        watch: {
            skipWrite: false,
            clearScreen: false,
            include: "src/**",
        },
    };
};
