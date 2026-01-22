import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  // Input: schema file (relative to this config, which is in build/graphql-codegen/)
  schema: '../../docs/schema.graphql',

  // Output: TypeScript types to intermediate file (merged into api.generated.ts by NUKE)
  generates: {
    '../../src/Consilient.WebApp2/src/types/.graphql.temp.ts': {
      plugins: ['typescript'],
      config: {
        // Map .NET custom scalars to string
        scalars: {
          Date: 'string',
          DateOnly: 'string',
          DateTimeOffset: 'string',
          TimeOnly: 'string',
          Char: 'string',
        },
        skipTypename: true,
        enumsAsTypes: false,
      },
    },
  },
};

export default config;
