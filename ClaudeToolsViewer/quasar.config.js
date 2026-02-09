/* eslint-env node */
const { configure } = require('quasar/wrappers')

module.exports = configure(function () {
  return {
    eslint: {
      warnings: true,
      errors: true,
    },

    boot: ['pinia'],

    css: ['app.scss'],

    extras: ['roboto-font', 'material-icons'],

    build: {
      target: {
        browser: ['es2019', 'edge88', 'firefox78', 'chrome87', 'safari13.1'],
        node: 'node16',
      },
      vueRouterMode: 'history',
    },

    devServer: {
      port: 9000,
      open: false,
      proxy: {
        '/api': {
          target: 'http://localhost:5000',
          changeOrigin: true,
        },
      },
    },

    framework: {
      config: {
        dark: true,
      },
      plugins: ['Notify', 'Loading'],
    },

    animations: [],
  }
})
