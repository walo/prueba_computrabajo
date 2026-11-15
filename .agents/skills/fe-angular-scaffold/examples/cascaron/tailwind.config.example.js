/** @type {import('tailwindcss').Config} */
module.exports = {
  presets: [require('../../../ui-design-system/tailwind.preset.js')],
  content: [
    './src/**/*.{html,ts}',
    '../../../ui-design-system/projects/ui-kit/src/**/*.{html,ts}',
  ],
};
