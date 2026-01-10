/**
 * Jest environment setup to disable localStorage in Node.js
 * 
 * This prevents the "Cannot initialize local storage without a `--localstorage-file` path" error
 * that occurs in Node.js v20+ with jest-environment-node.
 * 
 * See: https://github.com/nodejs/node/issues/40876
 */

// Prevent Node.js from initializing localStorage
delete global.localStorage;
delete global.sessionStorage;
