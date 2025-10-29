import React from 'react';
import { render } from '@testing-library/react';
import Page from '../src/app/page';

// Mock Next.js navigation
jest.mock('next/navigation', () => ({
  redirect: jest.fn(() => {
    throw new Error('NEXT_REDIRECT');
  }),
}));

describe('Page', () => {
  it('should render successfully', () => {
    expect(() => render(<Page />)).toThrow('NEXT_REDIRECT');
  });
});
