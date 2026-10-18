type PaginationProps = {
  page: number;
  pageSize: number;
  totalItems: number;
  onPageChange: (page: number) => void;
};

export function Pagination({ page, pageSize, totalItems, onPageChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
  if (totalItems === 0) return null;

  return <div className="pagination" aria-label="Product pagination">
    <span className="pagination-summary">Showing {Math.min((page - 1) * pageSize + 1, totalItems)}–{Math.min(page * pageSize, totalItems)} of {totalItems}</span>
    <div className="pagination-controls">
      <button className="page-button" onClick={() => onPageChange(page - 1)} disabled={page === 1}>Previous</button>
      {Array.from({ length: totalPages }, (_, index) => index + 1).map((pageNumber) => <button key={pageNumber} className={`page-button page-number ${pageNumber === page ? "active" : ""}`} onClick={() => onPageChange(pageNumber)} aria-current={pageNumber === page ? "page" : undefined}>{pageNumber}</button>)}
      <button className="page-button" onClick={() => onPageChange(page + 1)} disabled={page === totalPages}>Next</button>
    </div>
  </div>;
}
