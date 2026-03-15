import { render, screen, fireEvent } from '@testing-library/react';
import { Pagination, usePagination } from '@/components/ui/pagination';
import { renderHook, act } from '@testing-library/react';

describe('Pagination', () => {
  const defaultProps = {
    page: 1,
    pageSize: 10,
    totalCount: 100,
    totalPages: 10,
    onPageChange: jest.fn(),
    onPageSizeChange: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders nothing when totalCount is 0', () => {
    const { container } = render(
      <Pagination {...defaultProps} totalCount={0} totalPages={0} />
    );
    expect(container.firstChild).toBeNull();
  });

  it('renders results summary correctly', () => {
    render(<Pagination {...defaultProps} />);
    expect(screen.getByText(/Showing/)).toBeTruthy();
    expect(screen.getByRole('status')).toBeTruthy();
  });

  it('renders page size selector when enabled', () => {
    render(<Pagination {...defaultProps} showPageSizeSelector={true} />);
    expect(screen.getByLabelText('Select number of items per page')).toBeTruthy();
  });

  it('hides page size selector when disabled', () => {
    render(<Pagination {...defaultProps} showPageSizeSelector={false} />);
    expect(screen.queryByLabelText('Select number of items per page')).toBeNull();
  });

  it('calls onPageChange when clicking next page', () => {
    render(<Pagination {...defaultProps} />);
    // Use getAllByLabelText since there are mobile and desktop versions
    const nextButtons = screen.getAllByLabelText('Go to next page');
    fireEvent.click(nextButtons[0]);
    expect(defaultProps.onPageChange).toHaveBeenCalledWith(2);
  });

  it('calls onPageChange when clicking previous page', () => {
    render(<Pagination {...defaultProps} page={5} />);
    const prevButtons = screen.getAllByLabelText('Go to previous page');
    fireEvent.click(prevButtons[0]);
    expect(defaultProps.onPageChange).toHaveBeenCalledWith(4);
  });

  it('disables previous button on first page', () => {
    render(<Pagination {...defaultProps} page={1} />);
    const prevButtons = screen.getAllByLabelText('Go to previous page');
    prevButtons.forEach(button => {
      expect((button as HTMLButtonElement).disabled).toBe(true);
    });
  });

  it('disables next button on last page', () => {
    render(<Pagination {...defaultProps} page={10} />);
    const nextButtons = screen.getAllByLabelText('Go to next page');
    nextButtons.forEach(button => {
      expect((button as HTMLButtonElement).disabled).toBe(true);
    });
  });

  it('calls onPageChange when clicking first page button', () => {
    render(<Pagination {...defaultProps} page={5} showFirstLastButtons={true} />);
    const firstButton = screen.getByLabelText('Go to first page');
    fireEvent.click(firstButton);
    expect(defaultProps.onPageChange).toHaveBeenCalledWith(1);
  });

  it('calls onPageChange when clicking last page button', () => {
    render(<Pagination {...defaultProps} page={5} showFirstLastButtons={true} />);
    const lastButton = screen.getByLabelText('Go to last page');
    fireEvent.click(lastButton);
    expect(defaultProps.onPageChange).toHaveBeenCalledWith(10);
  });

  it('calls onPageSizeChange when selecting a new page size', () => {
    render(<Pagination {...defaultProps} />);
    const select = screen.getByLabelText('Select number of items per page');
    fireEvent.change(select, { target: { value: '25' } });
    expect(defaultProps.onPageSizeChange).toHaveBeenCalledWith(25);
  });

  it('disables all controls when disabled prop is true', () => {
    render(<Pagination {...defaultProps} page={5} disabled={true} />);
    const select = screen.getByLabelText('Select number of items per page') as HTMLSelectElement;
    expect(select.disabled).toBe(true);
  });

  it('handles keyboard navigation with Enter key', () => {
    render(<Pagination {...defaultProps} page={5} />);
    const nextButtons = screen.getAllByLabelText('Go to next page');
    fireEvent.keyDown(nextButtons[1], { key: 'Enter' }); // Desktop button
    expect(defaultProps.onPageChange).toHaveBeenCalledWith(6);
  });

  it('handles keyboard navigation with Space key', () => {
    render(<Pagination {...defaultProps} page={5} />);
    const prevButtons = screen.getAllByLabelText('Go to previous page');
    fireEvent.keyDown(prevButtons[1], { key: ' ' }); // Desktop button
    expect(defaultProps.onPageChange).toHaveBeenCalledWith(4);
  });

  it('shows correct page numbers for middle pages', () => {
    render(<Pagination {...defaultProps} page={5} />);
    // Should show: 1 ... 4 5 6 ... 10
    expect(screen.getByLabelText('Go to page 1')).toBeTruthy();
    expect(screen.getByLabelText('Go to page 4')).toBeTruthy();
    expect(screen.getByLabelText('Go to page 5')).toBeTruthy();
    expect(screen.getByLabelText('Go to page 6')).toBeTruthy();
    expect(screen.getByLabelText('Go to page 10')).toBeTruthy();
  });

  it('marks current page with aria-current', () => {
    render(<Pagination {...defaultProps} page={5} />);
    const currentPageButton = screen.getByLabelText('Go to page 5');
    expect(currentPageButton.getAttribute('aria-current')).toBe('page');
  });

  it('does not show pagination controls when only one page', () => {
    render(<Pagination {...defaultProps} totalCount={5} totalPages={1} />);
    expect(screen.queryByLabelText('Go to next page')).toBeNull();
    expect(screen.queryByLabelText('Go to previous page')).toBeNull();
  });

  it('renders singular "result" when totalCount is 1', () => {
    render(<Pagination {...defaultProps} totalCount={1} totalPages={1} pageSize={10} />);
    expect(screen.getByText(/result$/)).toBeTruthy();
    expect(screen.queryByText(/results$/)).toBeNull();
  });
});

describe('usePagination hook', () => {
  it('initializes with default values', () => {
    const { result } = renderHook(() => usePagination());
    expect(result.current.page).toBe(1);
    expect(result.current.pageSize).toBe(10);
  });

  it('initializes with custom default values', () => {
    const { result } = renderHook(() => 
      usePagination({ defaultPage: 3, defaultPageSize: 25 })
    );
    expect(result.current.page).toBe(3);
    expect(result.current.pageSize).toBe(25);
  });

  it('updates page correctly', () => {
    const { result } = renderHook(() => usePagination());
    act(() => {
      result.current.setPage(5);
    });
    expect(result.current.page).toBe(5);
  });

  it('updates pageSize and resets page to 1', () => {
    const { result } = renderHook(() => usePagination());
    act(() => {
      result.current.setPage(5);
    });
    expect(result.current.page).toBe(5);
    
    act(() => {
      result.current.setPageSize(25);
    });
    expect(result.current.pageSize).toBe(25);
    expect(result.current.page).toBe(1); // Should reset to page 1
  });

  it('resets to default values', () => {
    const { result } = renderHook(() => 
      usePagination({ defaultPage: 1, defaultPageSize: 10 })
    );
    
    act(() => {
      result.current.setPage(5);
      result.current.setPageSize(50);
    });
    expect(result.current.page).toBe(1); // setPageSize resets to 1
    expect(result.current.pageSize).toBe(50);
    
    act(() => {
      result.current.setPage(3);
    });
    
    act(() => {
      result.current.reset();
    });
    expect(result.current.page).toBe(1);
    expect(result.current.pageSize).toBe(10);
  });
});
