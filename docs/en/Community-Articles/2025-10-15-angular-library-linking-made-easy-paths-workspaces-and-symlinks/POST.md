# Angular Library Linking Made Easy: Paths, Workspaces, and Symlinks

### Understanding TypeScript Path Mapping

Path aliases is a powerful feature in TypeScript that helps developers simplify and organize their import statements. Instead of dealing with long and error-prone relative paths like `../../../components/button`, you can define a clear and descriptive alias that points directly to a specific directory or module.

This configuration is managed through the `paths` property in the TypeScript configuration file (`tsconfig.json`), allowing you to map custom names to local folders or compiled outputs. For example:

```tsx
// tsconfig.json
{
	"compilerOptions": {
		"paths": {
			"@my-package": ["./dist/my-package"],
			"@my-second-package": ["./projects/my-second-package/src/public-api.ts"]

		}
	}
}
```

In this setup, `@my-package` serves as a shorthand reference to your locally built library. Once configured, you can import modules using `@my-package` instead of long relative paths, which greatly improves readability and maintainability across large projects.

When working with multiple subdirectories or a more complex folder structure, you can also use wildcards to create flexible and dynamic mappings. This pattern is especially useful for modular libraries or mono-repos that contain multiple sub-packages:

```tsx
// tsconfig.json
{
	"compilerOptions": {
		"paths": {
			"@my-package/*": ["./dist/my-package/*"]
		}
	}
}
```

With this approach, imports like `@my-package/utils` or `@my-package/components/button` will automatically resolve to the corresponding directories in your build output. This makes your codebase more maintainable, portable, and consistent. This is useful especially when collaborating across teams or working with multiple libraries in the same workspace.

---

### Step-by-Step Examples of Path Configuration

As this example provides a glimpse for the path mapping, this is not the only way for aliases. Here are the other ways to utilize this feature.

1. **Using `package.json` Exports for Library Mapping**

   When developing internal libraries within a mono-repo, another option is to use the `exports` field in each library’s `package.json`

   This allows Node and modern bundlers to resolve imports cleanly when consuming the library, without depending solely on TypeScript configuration.

   ```tsx
   // dist/my-lib/package.json
   {
     "name": "@my-org/my-lib",
     "version": "1.0.0",
     "exports": {
       ".": "./index.js",
       "./utils": "./utils/index.ts"
     }
   }
   ```

   ```tsx
   import { formatDate } from "@my-org/my-lib/utils";
   ```

   This approach becomes especially powerful when publishing your libraries or integrating them into larger Angular mono-repos. Because, it aligns both runtime (Node) and compile-time (TypeScript) resolution.

2. **Linking Local Libraries via Symlinks**

   If you want to use a local library that is not yet published to npm, you can create a symbolic link between your library’s `dist` output and your consuming app.

   This is useful when testing or developing multiple packages in parallel.

   You can create a symlink using npm or yarn:

   ```tsx
   # Inside your library folder
   npm link

   # Inside your consuming app
   npm link @my-org/my-lib
   ```

   This effectively tells Node to resolve `@my-org/my-lib` from your local file system instead of the npm registry.

   However, note that symlinks can sometimes lead to path resolution issues with certain Angular build configurations, especially before the new application builder. With the latest builder improvements, this approach is becoming more stable and predictable.

3. **Combining Path Mapping with Workspace Configuration**

   In a structured Angular workspace, especially one created with **Nx** or **Angular CLI** using multiple projects, you can combine the approaches above.

   For instance, your `tsconfig.base.json` can define local references for in-repo libraries, while each library’s `package.json` provides external mappings for reuse outside the workspace.

   This hybrid setup ensures that:

   - The workspace remains easy to navigate and refactor locally.
   - External consumers (or CI builds) can still resolve imports correctly once libraries are built.

   For larger Angular projects or mono-repos, **Workspaces** (supported by both **Yarn** and **npm**) offer a clean way to manage multiple local packages within the same repository. Workspaces automatically link internal libraries together, so you can reference them by name instead of using manual `file:` paths or complex TypeScript aliases. This approach keeps dependencies consistent, simplifies cross-project development, and scales well for enterprise or multi-package setups.

Each of these methods has its strengths:

- **TypeScript paths:** This is great for local development and quick imports.
- **`package.json` exports:** This is ideal for libraries meant to be distributed.
- **Symlinks:** These are convenient for local testing between projects.

Choosing the right one, or even combining them depends on the scale of your project and whether you are building internal libraries, or a full mono-repo setup.

---

### How Path References Worked Before the New Angular Application Builder

Angular used to support path aliases to the locally installed packages by referencing to the `node_modules` folder like this:

```tsx
// tsconfig.json
{
	"compilerOptions": {
		"paths": {
			"@angular/*": ["./node_modules/@angular/*"]
		}
	}
}
```

However, this approach is not recommended, hence not supported, by the typescript. You can find detailed guidance on this topic in the TypeScript documentation, which notes that paths should not reference mono-repo packages or those inside **node_modules\***:\* [Paths should not point to monorepo packages or node_modules packages](https://www.typescriptlang.org/docs/handbook/modules/reference.html#paths-should-not-point-to-monorepo-packages-or-node_modules-packages).

Giving a real life example would explain the situation better. Suppose that you have such structure:

- You have a main angular app that consumes several npm dependencies and holds registered local paths that reference to another library locally like this:
  ```tsx
  // angular/tsconfig.json
  {
    "compileOnSave": false,
    "compilerOptions": {
      "paths": {
        "@abp/ng.identity": [
          "../modules/Volo.Abp.Identity/angular/projects/identity/src/public-api.ts"
        ],
        "@abp/ng.identity/config": [
          "../modules/Volo.Abp.Identity/angular/projects/identity/config/src/public-api.ts"
        ],
        "@abp/ng.identity/proxy": [
          "../modules/Volo.Abp.Identity/angular/projects/identity/proxy/src/public-api.ts"
        ]
      },
    },
  }
  ```
  This simply references to this package physically https://github.com/abpframework/abp/tree/dev/npm/ng-packs/packages/identity
- This library is also using these dependencies
  ```tsx
  // npm/ng-packs/packages/identity/package.json
  {
    "name": "@abp/ng.identity",
    "version": "10.0.0-rc.1",
    "homepage": "https://abp.io",
    "repository": {
      "type": "git",
      "url": "https://github.com/abpframework/abp.git"
    },
    "dependencies": {
      "@abp/ng.components": "~10.0.0-rc.1",
      "@abp/ng.permission-management": "~10.0.0-rc.1",
      "@abp/ng.theme.shared": "~10.0.0-rc.1",
      "tslib": "^2.0.0"
    },
    "publishConfig": {
      "access": "public"
    }
  }
  ```
  As these libraries also have their own dependencies, the identity package needs to consume them in itself. Before the [application builder migration](https://angular.dev/tools/cli/build-system-migration), you could register the path configuration like this
  ```tsx
  // angular/tsconfig.json
  {
    "compileOnSave": false,
    "compilerOptions": {
      "paths": {
        "@angular/*":["node_modules/@angular/*"],
        "@abp/*":["node_modules/@abp/*"],
        "@swimlane/*": ["node_modules/@swimlane/*"],
        "@ngx-validate/core":["node_modules/@ngx-validate/core"],
        "@ng-bootstrap/ng-bootstrap": ["node_modules/@ng-bootstrap/ng-bootstrap"],
        "@abp/ng.identity": [
          "../modules/Volo.Abp.Identity/angular/projects/identity/src/public-api.ts"
        ],
        "@abp/ng.identity/config": [
          "../modules/Volo.Abp.Identity/angular/projects/identity/config/src/public-api.ts"
        ],
        "@abp/ng.identity/proxy": [
          "../modules/Volo.Abp.Identity/angular/projects/identity/proxy/src/public-api.ts"
        ]
      },
    },
  }
  ```
  However, the latest builder forces more strict rules.So, it does not resolve the paths that reference to the `node_modules` causing a common DI errors as mentioned here:
  - https://github.com/angular/angular-cli/issues/31395
  - https://github.com/angular/angular-cli/issues/26901
  - https://github.com/angular/angular-cli/issues/27176

In this case, we recommend using a symlink script. You can reach them through: [INSERT GITHUB LINK]

These scripts help you share dependencies from the main Angular app to local library projects via symlinks:

- `symlink-config.ps1` centralizes which library directories to touch (e.g., ../../modules/Volo.Abp.Identity/angular/projects/identity) and which packages to link (e.g., @angular, @abp, rxjs)
- `setup-symlinks.ps1` reads that config and, for each library, creates a `node_modules` folder if needed and symlinks only the listed packages from the `node_modules` of the app to avoid duplicate installs
- `remove-symlinks.ps1` cleans up by deleting those library node_modules directories so they can use their own local deps again
- In `angular/package.json`, the `symlinks:setup` and `symlinks:remove` npm scripts simply run those two PowerShell scripts so you can execute them conveniently with your package manager.
