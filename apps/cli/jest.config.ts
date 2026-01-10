export default {
  displayName: 'cli',
  preset: '../../jest.preset.js',
  testEnvironment: '<rootDir>/jest-environment-node-no-storage.js',
  setupFilesAfterEnv: ['<rootDir>/jest.env.setup.js'],
  transform: {
    '^.+\\.[tj]s$': ['ts-jest', { tsconfig: '<rootDir>/tsconfig.spec.json' }],
  },
  moduleFileExtensions: ['ts', 'js', 'html'],
  coverageDirectory: '../../coverage/apps/cli',
};
