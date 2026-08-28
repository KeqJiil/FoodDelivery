import eslint from "@eslint/js";
import angular from "angular-eslint";
import tseslint from "typescript-eslint";

export default tseslint.config(
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

      "@typescript-eslint/no-unsafe-any": "error",

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
      // Boolean strictness
      // ====================================================

      "@typescript-eslint/strict-boolean-expressions": [
        "error",
        {
          allowString: false,
          allowNumber: false,
          allowNullableObject: false,
          allowNullableBoolean: false,
          allowNullableString: false,
          allowNullableNumber: false,
          allowNullableEnum: false,
          allowAny: false,
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

      "@typescript-eslint/prefer-readonly-parameter-types": [
        "error",
        {
          checkParameterProperties: true,
          ignoreInferredTypes: false,
        },
      ],

      // ====================================================
      // Naming conventions
      // ====================================================

      "@typescript-eslint/naming-convention": [
        "error",

        // Classes / interfaces / types / enums
        {
          selector: "typeLike",
          format: ["PascalCase"],
        },

        // Functions
        {
          selector: "function",
          format: ["camelCase"],
        },

        // Variables
        {
          selector: "variable",
          format: ["camelCase", "UPPER_CASE"],
        },

        // Parameters
        {
          selector: "parameter",
          format: ["camelCase"],
          leadingUnderscore: "allow",
        },

        // Class properties
        {
          selector: "classProperty",
          format: ["camelCase"],
        },

        // Class methods
        {
          selector: "classMethod",
          format: ["camelCase"],
        },

        // Enum members
        {
          selector: "enumMember",
          format: ["PascalCase"],
        },

        // Destructured variables
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
