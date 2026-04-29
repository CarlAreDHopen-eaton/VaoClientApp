# Copilot Instructions

When writing or modifying C# code in this repository:

- Private fields (including `readonly`): prefix with `m` and use PascalCase after the prefix.
- Private field examples: `mVariableName`, `mApiClient`, `mCurrentUser`.
- Method and function parameters: use `camelCase`, for example `variableName`.
- Local variables: use `camelCase`.
- Public, internal, and protected fields: use PascalCase.
- Constants (`const`): use `C_VARIABLE_NAME` (uppercase snake case with `C_` prefix), for example `C_MAX_RETRIES`.
- Static readonly fields: use PascalCase.
- Classes, structs, records, enums, delegates: use PascalCase.
- Interfaces: use PascalCase with an `I` prefix, for example `IRequestHandler`.
- Methods and local functions: use PascalCase.
- Properties: use PascalCase.
- Events: use PascalCase.
- Enum members: use PascalCase.
- Namespaces: use PascalCase.
- Type parameters: use `T` prefix and PascalCase, for example `TItem`, `TResult`.
- Boolean members, properties, and variables should use clear prefixes when appropriate: `Is`, `Has`, `Can`, `Should`.
- Async methods should end with `Async`.
- Avoid abbreviations unless they are well-known and clear (for example `Http`, `Uri`).

Do not use underscore-prefixed private fields (for example `_variableName`) unless existing code in the same file already requires it for compatibility.
