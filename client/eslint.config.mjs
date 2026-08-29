import eslint from "@eslint/js";
import angular from "angular-eslint";
import tseslint from "typescript-eslint";

export default tseslint.config(
  {
    ignores: ["dist/**", ".angular/**", "node_modules/**"],
  },

  // ============================================================
  // JavaScript / TypeScript base
  // ============================================================

  eslint.configs.recommended,

  // ============================================================
  // TypeScript — MAXIMUM STRICTNESS
  // ============================================================

  {
    files: ["**/*.ts"],

    extends: [
      tseslint.configs.strictTypeChecked,
      tseslint.configs.stylisticTypeChecked,
    ],

    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },

    rules: {
      // ====================================================
      // Explicit typing — C# style
      // ====================================================

      // Every function/method must have return type
      "@typescript-eslint/explicit-function-return-type": [
        "error",
        {
          allowExpressions: false,
          allowTypedFunctionExpressions: false,
          allowHigherOrderFunctions: false,
          allowDirectConstAssertionInArrowFunctions: false,
          allowConciseArrowFunctionExpressionsStartingWithVoid: false,
        },
      ],

      // Public API / exported functions must have types
      "@typescript-eslint/explicit-module-boundary-types": "error",

      // Every class member must explicitly say
      // public / protected / private
      "@typescript-eslint/explicit-member-accessibility": [
        "error",
        {
          accessibility: "explicit",
        },
      ],

      // ====================================================
      // NO ANY
      // ====================================================

      "@typescript-eslint/no-explicit-any": "error",

      "@typescript-eslint/no-unsafe-assignment": "error",

      "@typescript-eslint/no-unsafe-argument": "error",

      "@typescript-eslint/no-unsafe-call": "error",

      "@typescript-eslint/no-unsafe-member-access": "error",

      "@typescript-eslint/no-unsafe-return": "error",

      "@typescript-eslint/no-unsafe-enum-comparison": "error",

      // ====================================================
      // Null safety
      // ====================================================

      // user!.name -> forbidden
      "@typescript-eslint/no-non-null-assertion": "error",

      // ====================================================
      // Type assertions
      // ====================================================

      // const user = value as User -> forbidden
      "@typescript-eslint/consistent-type-assertions": [
        "error",
        {
          assertionStyle: "never",
        },
      ],

      // ====================================================
      // Unnecessary code
      // ====================================================

      "@typescript-eslint/no-unnecessary-condition": "error",

      "@typescript-eslint/no-unnecessary-type-arguments": "error",

      "@typescript-eslint/no-unnecessary-type-assertion": "error",

      "@typescript-eslint/no-unnecessary-type-parameters": "error",

      // ====================================================
      // Type imports
      // ====================================================

      "@typescript-eslint/consistent-type-imports": [
        "error",
        {
          prefer: "type-imports",
          fixStyle: "separate-type-imports",
        },
      ],

      // ====================================================
      // Interfaces / types
      // ====================================================

      "@typescript-eslint/consistent-type-definitions": [
        "error",
        "interface",
      ],

      // ====================================================
      // readonly
      // ====================================================

      "@typescript-eslint/prefer-readonly": "error",

      // Observable<void>/HttpClient's own generic methods (get<void>, post<void>, etc.)
      // conflict with this rule's generic-position check — void here is the
      // idiomatic Angular way to type an empty HTTP response body.
      "@typescript-eslint/no-invalid-void-type": "off",

      // ====================================================
      // Naming conventions
      // ====================================================

      "@typescript-eslint/naming-convention": [
        "error",

        {
          selector: "interface",
          format: ["PascalCase"],
          custom: {
            regex: "^I[A-Z]",
            match: true,
          },
        },

        {
          selector: ["class", "enum", "typeAlias"],
          format: ["PascalCase"],
        },

        {
          selector: "variable",
          modifiers: ["const", "global"],
          format: ["PascalCase", "UPPER_CASE"],
          leadingUnderscore: "allow",
        },

        {
          selector: "variable",
          format: ["camelCase"],
          leadingUnderscore: "allow",
        },

        {
          selector: "classProperty",
          modifiers: ["private"],
          format: ["camelCase"],
          leadingUnderscore: "require",
        },
        {
          selector: "classProperty",
          modifiers: ["public"],
          format: ["PascalCase", "camelCase"],
        },
        {
          selector: "classProperty",
          modifiers: ["protected"],
          format: ["PascalCase", "camelCase"],
          leadingUnderscore: "allow",
        },

        {
          selector: "function",
          format: ["PascalCase", "camelCase"],
        },

        {
          selector: "parameter",
          format: ["camelCase"],
          leadingUnderscore: "allow",
        },

        {
          selector: "enumMember",
          format: ["PascalCase"],
        },

        {
          selector: "objectLiteralProperty",
          format: null,
        },

        {
          selector: "variable",
          modifiers: ["destructured"],
          format: null,
        },
      ],

      // ====================================================
      // Class member ordering — C# style
      // ====================================================

      "@typescript-eslint/member-ordering": [
        "error",
        {
          default: [
            // Static fields
            "public-static-field",
            "protected-static-field",
            "private-static-field",

            // Instance fields
            "public-instance-field",
            "protected-instance-field",
            "private-instance-field",

            // Constructor
            "public-constructor",
            "protected-constructor",
            "private-constructor",

            // Static methods
            "public-static-method",
            "protected-static-method",
            "private-static-method",

            // Instance methods
            "public-instance-method",
            "protected-instance-method",
            "private-instance-method",
          ],
        },
      ],

      // ====================================================
      // Unused
      // ====================================================

      "@typescript-eslint/no-unused-vars": [
        "error",
        {
          args: "all",
          argsIgnorePattern: "^_",
          vars: "all",
          varsIgnorePattern: "^_",
          caughtErrors: "all",
          caughtErrorsIgnorePattern: "^_",
        },
      ],

      // ====================================================
      // General JS safety
      // ====================================================

      "eqeqeq": ["error", "always"],

      "no-var": "error",

      "prefer-const": "error",

      "no-eval": "error",

      "no-implied-eval": "error",

      "no-with": "error",

      "no-new-wrappers": "error",

      "no-throw-literal": "error",
    },
  },

  // ============================================================
  // Angular TypeScript
  // ============================================================

  {
    files: ["**/*.ts"],

    extends: [
      angular.configs.tsRecommended,
    ],
  },

  // ============================================================
  // Angular HTML templates
  // ============================================================

  {
    files: ["**/*.html"],

    extends: [
      angular.configs.templateRecommended,
    ],
  },
);
