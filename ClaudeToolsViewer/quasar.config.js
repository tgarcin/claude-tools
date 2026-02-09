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
      vitePlugins: [],
      extendViteConf(viteConf) {
        viteConf.server = viteConf.server || {}
        viteConf.server.fs = viteConf.server.fs || {}
        viteConf.server.fs.allow = ['.', '/home/tgarcin/.yarn']
      },
    },

    devServer: {
      port: 9000,
      open: false,
      proxy: {
        '/api': {
          target: 'http://localhost:5044',
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
