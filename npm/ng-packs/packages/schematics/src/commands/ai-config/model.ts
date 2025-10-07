export type AiTool = 'claude' | 'copilot' | 'cursor' | 'gemini' | 'junie' | 'windsurf';

export interface AiConfigSchema {
  tool: AiTool[];
  targetProject?: string;
  overwrite?: boolean;
}

export interface AiConfigFile {
  path: string;
  content: string;
}
