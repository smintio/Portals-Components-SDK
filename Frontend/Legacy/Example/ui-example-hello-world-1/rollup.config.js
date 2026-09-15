// A stub that lets rollup read the shared TypeScript config
import tsNode from "ts-node";
import pkg from "./package.json";
import rollupConfig from "../../config/rollup/rollup-config.ts";

tsNode.register({
    compilerOptions: {
        module: "CommonJS",
    },
    project: "../../config/rollup/tsconfig.json",
});

module.exports = rollupConfig(pkg, __dirname);
