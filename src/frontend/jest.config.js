const path = require('node:path');
const { createCjsPreset } = require('jest-preset-angular/presets');

const cjsPreset = createCjsPreset({
  tsconfig: '<rootDir>/tsconfig.spec.json',
});

module.exports = {
  ...cjsPreset,
  setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],
  testPathIgnorePatterns: ['/node_modules/', '/dist/'],
  moduleDirectories: ['node_modules', path.resolve(__dirname, 'node_modules')],
  moduleNameMapper: {
    '^@accionadev/ui-kit$': path.resolve(
      __dirname,
      '../../../ui-design-system/projects/ui-kit/src/public-api.ts'
    ),
    '^@angular/core$': require.resolve('@angular/core'),
    '^@angular/core/testing$': require.resolve('@angular/core/testing'),
    '^@angular/common$': require.resolve('@angular/common'),
    '^@angular/common/http$': require.resolve('@angular/common/http'),
    '^@angular/common/http/testing$': require.resolve('@angular/common/http/testing'),
    '^@angular/forms$': require.resolve('@angular/forms'),
    '^@angular/router$': require.resolve('@angular/router'),
    '^@angular/platform-browser$': require.resolve('@angular/platform-browser'),
    '^lucide$': require.resolve('lucide'),
    '^rxjs$': require.resolve('rxjs'),
    '^tslib$': require.resolve('tslib'),
  },
};
