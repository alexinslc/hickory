# Accessibility Guidelines for Hickory Help Desk

## Overview

This document outlines the accessibility features implemented in the Hickory Help Desk web application and provides guidelines for maintaining and improving accessibility.

## Current Accessibility Features

### 1. Skip Navigation

A "Skip to main content" link is provided at the top of every page, allowing keyboard users to bypass repetitive navigation elements and jump directly to the main content.

- **Location**: Root layout (`apps/web/src/app/layout.tsx`)
- **Usage**: Press `Tab` on page load to reveal the skip link, then press `Enter` to skip to main content
- **Target**: `#main-content` ID on the `<main>` element

### 2. Keyboard Navigation

All interactive elements are keyboard accessible and include visible focus indicators:

- **Focus indicators**: 3px solid outline using the primary color
- **Keyboard-only focus**: Uses `:focus-visible` to show focus only for keyboard navigation
- **Tab order**: Follows logical document flow
- **Interactive elements**: All buttons, links, form inputs, and dropdowns are keyboard accessible

### 3. ARIA Labels and Semantic HTML

#### Navigation
- Main navigation includes `aria-label="Main navigation"`
- Search form includes `role="search"`
- Navigation links include descriptive `aria-label` attributes
- Mobile navigation includes `aria-label="Mobile navigation"`

#### Forms
- All form inputs have associated `<label>` elements with `htmlFor` attributes
- Required fields are marked with asterisks and `required` attribute
- Error messages use `aria-invalid` and `aria-describedby` to associate with inputs
- Form sections use `<fieldset>` with `<legend>` for grouping

#### Interactive Elements
- Buttons include descriptive `aria-label` attributes when text alone is insufficient
- Loading states include `role="status"` and `aria-label` for screen readers
- Error messages include `role="alert"` for immediate announcement
- Dialogs include `role="dialog"` and descriptive `aria-label`

#### Tables
- Table headers use `<th scope="col">` for proper screen reader announcement
- Interactive table rows include `role="button"`, `tabIndex`, and keyboard event handlers
- Complex data includes `aria-label` for screen reader context

#### Lists
- Semantic lists use `<ul>` and `<li>` elements
- List containers include `aria-label` for context

### 4. Screen Reader Support

#### Decorative vs Informative Content
- **Decorative images/icons**: Include `aria-hidden="true"` to hide from screen readers
- **Informative icons**: Include descriptive text (visible or screen-reader only)
- **Status indicators**: Include `aria-label` for screen reader announcement

#### Screen Reader Only Content
- Uses `.sr-only` utility class for content visible only to screen readers
- Applied to section headings, loading status, and supplementary information
- Defined in `apps/web/src/app/global.css`

#### Time Elements
- Dates use `<time dateTime="...">` for semantic date representation
- Screen readers announce properly formatted dates

### 5. Heading Hierarchy

Proper heading structure is maintained throughout the application:

- **h1**: Page title (one per page)
- **h2**: Major sections
- **h3**: Subsections within h2 sections
- Screen-reader-only headings (`.sr-only`) for sections without visible headings

### 6. Color and Contrast

- Primary color contrast meets WCAG AA standards (minimum 4.5:1 for normal text)
- Status colors (success, warning, error) include text labels in addition to color
- Focus indicators are highly visible (3px solid outline)
- Hover states provide additional visual feedback beyond color

### 7. Form Validation and Error Handling

- **Inline validation**: Errors displayed next to form fields
- **ARIA attributes**: `aria-invalid` and `aria-describedby` link errors to inputs
- **Error messages**: Use `role="alert"` for immediate screen reader announcement
- **Character counters**: Provide real-time feedback on input length
- **Required fields**: Clearly marked with asterisks and `required` attribute

### 8. Loading States and Progress

- Loading spinners include `role="status"` and descriptive `aria-label`
- Button states change during async operations (e.g., "Creating..." instead of "Create")
- Disabled state is indicated both visually and via `disabled` attribute

## Testing Recommendations

### Manual Testing

1. **Keyboard Navigation**
   - Test all pages using only keyboard (Tab, Shift+Tab, Enter, Space, Arrow keys)
   - Verify focus indicators are visible on all interactive elements
   - Ensure skip-to-content link works correctly

2. **Screen Reader Testing**
   - Test with NVDA (Windows), JAWS (Windows), or VoiceOver (macOS)
   - Verify all content is announced correctly
   - Check that form errors are announced when they occur
   - Ensure ARIA labels provide sufficient context

3. **Color Contrast**
   - Use browser DevTools or online contrast checkers
   - Verify all text meets WCAG AA standards (4.5:1 for normal text, 3:1 for large text)
   - Test with color blindness simulators

### Automated Testing

1. **Lighthouse Accessibility Audit**
   ```bash
   # Run Lighthouse in Chrome DevTools
   # Or use CLI:
   npx lighthouse https://your-app-url --only-categories=accessibility --view
   ```
   **Target Score**: > 95

2. **axe DevTools**
   - Install axe DevTools browser extension
   - Run automated scan on each page
   - Address all issues flagged by axe

3. **ESLint JSX Accessibility**
   - Already configured in the project
   - Run: `npx nx lint web`

## Best Practices for Developers

### When Adding New Components

1. **Use semantic HTML**
   - Choose the most appropriate HTML element (`<button>` vs `<div>`, `<nav>` vs `<div>`)
   - Use `<header>`, `<main>`, `<footer>`, `<section>`, `<article>`, etc.

2. **Add ARIA labels**
   - Add `aria-label` to buttons and links when text alone is insufficient
   - Use `aria-describedby` to associate help text with form inputs
   - Add `aria-invalid` and error states to form validation

3. **Include keyboard support**
   - All interactive elements must be keyboard accessible
   - Add `tabIndex={0}` for custom interactive elements
   - Implement keyboard event handlers (Enter, Space) for custom buttons

4. **Mark decorative content**
   - Add `aria-hidden="true"` to purely decorative icons and images
   - Ensure informative images have alt text or ARIA labels

5. **Test with keyboard**
   - Tab through the component
   - Verify focus is visible and logical
   - Ensure all functionality works without a mouse

### Form Accessibility Checklist

- [ ] All inputs have associated `<label>` elements
- [ ] Required fields are marked with `required` attribute
- [ ] Error messages use `aria-invalid` and `aria-describedby`
- [ ] Fieldsets group related inputs with descriptive legends
- [ ] Placeholder text is not used as the only label
- [ ] Form submission provides clear feedback (success/error)
- [ ] Disabled inputs are clearly indicated

### Component Accessibility Checklist

- [ ] Semantic HTML elements used appropriately
- [ ] Keyboard navigation works correctly
- [ ] Focus indicators are visible
- [ ] ARIA labels provide sufficient context
- [ ] Decorative elements are hidden from screen readers
- [ ] Color is not the only means of conveying information
- [ ] Loading and error states are announced to screen readers

## Resources

### WCAG Guidelines
- [WCAG 2.1 Quick Reference](https://www.w3.org/WAI/WCAG21/quickref/)
- [WebAIM Checklist](https://webaim.org/standards/wcag/checklist)

### Testing Tools
- [axe DevTools](https://www.deque.com/axe/devtools/) - Browser extension for accessibility testing
- [Lighthouse](https://developers.google.com/web/tools/lighthouse) - Built into Chrome DevTools
- [WAVE](https://wave.webaim.org/) - Web accessibility evaluation tool
- [Color Contrast Checker](https://webaim.org/resources/contrastchecker/)

### Screen Readers
- [NVDA](https://www.nvaccess.org/) - Free screen reader for Windows
- [JAWS](https://www.freedomscientific.com/products/software/jaws/) - Popular screen reader for Windows
- [VoiceOver](https://www.apple.com/accessibility/voiceover/) - Built into macOS and iOS

### Learning Resources
- [A11y Project](https://www.a11yproject.com/) - Community-driven resource for web accessibility
- [MDN Web Accessibility](https://developer.mozilla.org/en-US/docs/Web/Accessibility) - Mozilla's accessibility documentation
- [WebAIM](https://webaim.org/) - Web accessibility in mind

## Reporting Accessibility Issues

If you discover an accessibility issue:

1. Check if it's already documented in the GitHub Issues
2. Create a new issue with the `accessibility` label
3. Include:
   - Description of the issue
   - Steps to reproduce
   - Browser and assistive technology used
   - Expected vs actual behavior
   - Screenshots or video if helpful

## Future Improvements

Planned accessibility enhancements:

- [ ] Add more comprehensive keyboard shortcuts
- [ ] Implement reduced motion preferences support
- [ ] Add high contrast mode support
- [ ] Create accessibility statement page
- [ ] Conduct professional accessibility audit
- [ ] Add automated accessibility testing to CI/CD pipeline
- [ ] Implement ARIA live regions for dynamic content updates
- [ ] Add more screen-reader-only context throughout the app

## Compliance

This application aims to meet:
- **WCAG 2.1 Level AA** - Web Content Accessibility Guidelines
- **Section 508** - U.S. federal accessibility standards
- **ADA** - Americans with Disabilities Act web accessibility requirements

---

Last updated: 2025-10-29
