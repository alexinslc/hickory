/**
 * Custom Jest environment that extends node environment but disables localStorage.
 * 
 * This fixes the "Cannot initialize local storage without a `--localstorage-file` path" error
 * that occurs in Node.js v20+ with jest-environment-node.
 * 
 * See: https://github.com/nodejs/node/issues/40876
 */

// Patch Node.js global before the environment initializes
// This prevents localStorage from trying to initialize
Object.defineProperty(global, 'localStorage', {
  get() {
    return undefined;
  },
  configurable: true,
});

Object.defineProperty(global, 'sessionStorage', {
  get() {
    return undefined;
  },
  configurable: true,
});

const NodeEnvironment = require('jest-environment-node').TestEnvironment;

class NodeEnvironmentNoStorage extends NodeEnvironment {
  constructor(config, context) {
    super(config, context);
    
    // Ensure localStorage and sessionStorage are not available in test global
    // Our CLI doesn't use these APIs - it uses file-based storage
    delete this.global.localStorage;
    delete this.global.sessionStorage;
  }
}

module.exports = NodeEnvironmentNoStorage;
