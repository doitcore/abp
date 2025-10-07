import { Rule, SchematicsException, Tree, apply, url, mergeWith, MergeStrategy, filter, chain } from '@angular-devkit/schematics';
import { join, normalize } from '@angular-devkit/core';
import { AiConfigSchema, AiTool } from './model';
import { getWorkspace } from '../../utils';

/**
 * Generates AI configuration files for Angular projects based on selected tools.
 * This schematic creates configuration files that help AI tools follow Angular best practices.
 */
export default function (options: AiConfigSchema): Rule {
  return async (tree: Tree) => {
    // Validate options
    if (!options.tool || options.tool.length === 0) {
      console.log('ℹ️  No AI tools selected. Skipping configuration generation.');
      return tree;
    }

    // Get workspace configuration
    const workspace = await getWorkspace(tree);
    let targetPath = '/';

    // If targetProject is specified, generate in project directory
    if (options.targetProject) {
      const project = workspace.projects.get(options.targetProject);
      if (!project) {
        throw new SchematicsException(
          `Project "${options.targetProject}" not found in workspace.`
        );
      }
      targetPath = normalize(project.root);
    }

    console.log('🚀 Generating AI configuration files...');
    console.log(`📁 Target path: ${targetPath}`);
    console.log(`🤖 Selected tools: ${options.tool.join(', ')}`);

    // Generate rules for each selected tool
    const rules: Rule[] = options.tool
      .map(tool => generateConfigForTool(tool, targetPath, options.overwrite || false));

    // Apply all rules and log results
    return chain([
      ...rules,
      (tree: Tree) => {
        console.log('✅ AI configuration files generated successfully!');
        console.log('\n📝 Generated files:');
        
        options.tool.forEach(tool => {
          const configPath = getConfigPath(tool, targetPath);
          console.log(`   - ${configPath}`);
        });

        console.log('\n💡 Tip: Restart your IDE or AI tool to apply the new configurations.');
        
        return tree;
      }
    ]);
  };
}

/**
 * Generates configuration for a specific AI tool
 */
function generateConfigForTool(tool: AiTool, targetPath: string, overwrite: boolean): Rule {
  return (tree: Tree) => {
    const configPath = getConfigPath(tool, targetPath);
    
    // Check if file already exists
    if (tree.exists(configPath) && !overwrite) {
      console.log(`⚠️  Configuration file already exists: ${configPath}`);
      console.log(`   Use --overwrite flag to replace existing files.`);
      return tree;
    }

    // Get template source
    const sourceDir = `./files/${tool}`;
    const source = apply(url(sourceDir), [
      filter(path => {
        // Filter out any unnecessary files
        return !path.endsWith('.DS_Store');
      })
    ]);

    // Merge with existing tree
    return mergeWith(source, overwrite ? MergeStrategy.Overwrite : MergeStrategy.Default);
  };
}

/**
 * Gets the configuration file path for a specific tool
 */
function getConfigPath(tool: AiTool, basePath: string): string {
  const configFiles: Record<AiTool, string> = {
    claude: '.claude/CLAUDE.md',
    copilot: '.github/copilot-instructions.md',
    cursor: '.cursor/rules/cursor.mdc',
    gemini: '.gemini/GEMINI.md',
    junie: '.junie/guidelines.md',
    windsurf: '.windsurf/rules/guidelines.md'
  };

  const configFile = configFiles[tool];
  return join(normalize(basePath), configFile);
}
