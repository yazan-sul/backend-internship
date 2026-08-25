import { FormEvent, useEffect, useRef, useState } from "react";
import { Icon } from "./components/Icon";
import { createProduct, deleteProduct, getProducts, Product, ProductSortField, SortDirection, updateProduct } from "./api/products";
import { Pagination } from "./components/Pagination";
import { ProductTable } from "./components/ProductTable";
import "./index.css";

type FormState = { name: string; price: string; quantity: string };
type ModalState = { mode: "add" | "edit"; product?: Product };

const blankForm: FormState = { name: "", price: "", quantity: "" };
const currency = (value: number) => new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(value);

export function App() {
  const [products, setProducts] = useState<Product[]>([]);
  const [totalProducts, setTotalProducts] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [query, setQuery] = useState("");
  const [page, setPage] = useState(1);
  const [sortBy, setSortBy] = useState<ProductSortField>("name");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [reloadKey, setReloadKey] = useState(0);
  const [showStats, setShowStats] = useState(false);
  const [modal, setModal] = useState<ModalState | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null);
  const [form, setForm] = useState(blankForm);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState("");
  const nameInput = useRef<HTMLInputElement>(null);

  const totalValue = products.reduce((sum, p) => sum + p.price * p.quantity, 0);
  const totalUnits = products.reduce((sum, p) => sum + p.quantity, 0);
  const lowStock = products.filter((p) => p.quantity > 0 && p.quantity < 10).length;

  useEffect(() => {
    const controller = new AbortController();
    const timer = window.setTimeout(() => void loadProducts(), query.trim() ? 250 : 0);

    async function loadProducts() {
      try {
        setIsLoading(true);
        setLoadError("");
        const result = await getProducts(query, { page, pageSize: 10, sortBy, sortDirection, signal: controller.signal });
        setProducts(result.items);
        setTotalProducts(result.totalCount);
      } catch (cause) {
        if (cause instanceof DOMException && cause.name === "AbortError") return;
        setLoadError(cause instanceof Error ? cause.message : "The inventory could not be loaded.");
      } finally {
        if (!controller.signal.aborted) setIsLoading(false);
      }
    }

    return () => { window.clearTimeout(timer); controller.abort(); };
  }, [query, page, sortBy, sortDirection, reloadKey]);

  useEffect(() => {
    if (!modal && !deleteTarget) return;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") { setModal(null); setDeleteTarget(null); setError(""); setDeleteError(""); }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [modal, deleteTarget]);

  function showNotice(message: string) { setNotice(message); window.setTimeout(() => setNotice(""), 2800); }
  function openAdd() { setForm(blankForm); setError(""); setModal({ mode: "add" }); window.setTimeout(() => nameInput.current?.focus(), 0); }
  function openEdit(product: Product) { setForm({ name: product.name, price: String(product.price), quantity: String(product.quantity) }); setError(""); setModal({ mode: "edit", product }); window.setTimeout(() => nameInput.current?.focus(), 0); }
  function closeModal() { setModal(null); setError(""); }
  function setField(field: keyof FormState, value: string) { setForm((current) => ({ ...current, [field]: value })); if (error) setError(""); }
  function handleSort(field: ProductSortField) {
    setSortDirection((current) => sortBy === field ? (current === "asc" ? "desc" : "asc") : "asc");
    setSortBy(field);
    setPage(1);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const name = form.name.trim(); const price = Number(form.price); const quantity = Number(form.quantity);
    if (!name) return setError("Enter a product name.");
    if (!Number.isFinite(price) || price < 0) return setError("Enter a valid, non-negative price.");
    if (!Number.isInteger(quantity) || quantity < 0) return setError("Enter a valid, non-negative quantity.");
    if (products.some((p) => p.name.toLowerCase() === name.toLowerCase() && p.id !== modal?.product?.id)) return setError("Another product already uses this name.");
    if (modal?.mode === "edit" && modal.product) {
      try {
        setIsSaving(true);
        const updated = await updateProduct(modal.product.id, { name, price, quantity });
        setProducts((current) => current.map((product) => product.id === updated.id ? updated : product));
        showNotice(`${name} was updated`);
        closeModal();
      } catch (cause) {
        setError(cause instanceof Error ? cause.message : "The product could not be updated.");
      } finally {
        setIsSaving(false);
      }
      return;
    }

    try {
      setIsSaving(true);
      const createdProduct = await createProduct({ name, price, quantity });
      setProducts((current) => [...current, createdProduct]);
      setReloadKey((current) => current + 1);
      showNotice(`${name} was added to inventory`);
      closeModal();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "The product could not be added.");
    } finally {
      setIsSaving(false);
    }
  }
  async function confirmDelete() {
    if (!deleteTarget) return;
    try {
      setIsDeleting(true);
      setDeleteError("");
      await deleteProduct(deleteTarget.id);
      const name = deleteTarget.name;
      setProducts((current) => current.filter((p) => p.id !== deleteTarget.id));
      setReloadKey((current) => current + 1);
      setDeleteTarget(null);
      showNotice(`${name} was removed`);
    } catch (cause) {
      setDeleteError(cause instanceof Error ? cause.message : "The product could not be removed.");
    } finally {
      setIsDeleting(false);
    }
  }

  return <main className="app-shell"><div className="ambient-glow" /><div className="page-wrap">
    <header className="topbar"><div className="brand-lockup"><div className="brand-mark"><Icon name="box" size={20} /></div><div><p className="eyebrow">Operations</p><h1>Inventory</h1></div></div></header>
    <section className="intro-row"><div><h2>Everything in one place.</h2></div><div className="intro-actions"><button className="secondary-button stats-toggle" onClick={() => setShowStats((current) => !current)} aria-expanded={showStats}><span>{showStats ? "Hide summary" : "Show summary"}</span><span aria-hidden="true">{showStats ? "↑" : "↓"}</span></button><button className="primary-button" onClick={openAdd}><Icon name="plus" size={17} /> Add product</button></div></section>
    {showStats && <section className="stats-grid" aria-label="Inventory summary">
      <div className="stat-card"><span className="stat-label">Total value</span><strong>{currency(totalValue)}</strong><span className="stat-note"><Icon name="spark" size={13} /> Current inventory</span></div>
      <div className="stat-card"><span className="stat-label">Products</span><strong>{totalProducts}</strong><span className="stat-note">Across your catalog</span></div>
      <div className="stat-card"><span className="stat-label">Units in stock</span><strong>{totalUnits.toLocaleString()}</strong><span className="stat-note">Total available units</span></div>
      <div className="stat-card"><span className="stat-label">Low stock</span><strong className={lowStock ? "warning-number" : ""}>{lowStock}</strong><span className="stat-note">Needs your attention</span></div>
    </section>}
    <section className="inventory-panel"><div className="panel-toolbar"><div><h3>All products</h3><p>{query.trim() ? `${totalProducts} results for “${query.trim()}”` : "Manage your current inventory"}</p></div><label className="search-box"><Icon name="search" size={17} /><span className="sr-only">Search products</span><input value={query} onChange={(e) => { setQuery(e.target.value); setPage(1); }} placeholder="Search products..." /></label></div>
      {isLoading ? <div className="empty-state" role="status"><div className="empty-icon"><Icon name="spark" size={22} /></div><h3>Loading inventory</h3><p>Retrieving your products from the database.</p></div> : loadError ? <div className="empty-state" role="alert"><div className="empty-icon"><Icon name="close" size={22} /></div><h3>Couldn’t load inventory</h3><p>{loadError}</p></div> : products.length === 0 ? <div className="empty-state"><div className="empty-icon"><Icon name={query ? "search" : "box"} size={22} /></div><h3>{query ? "No products found" : "Your inventory is empty"}</h3><p>{query ? "Try a different search term." : "Add your first product to start tracking stock."}</p>{!query && <button className="secondary-button" onClick={openAdd}><Icon name="plus" size={16} /> Add your first product</button>}</div> : <><ProductTable products={products} sortBy={sortBy} sortDirection={sortDirection} onSort={handleSort} onEdit={openEdit} onDelete={(product) => { setDeleteError(""); setDeleteTarget(product); }} /><Pagination page={page} pageSize={10} totalItems={totalProducts} onPageChange={setPage} /></>}
    </section>
  </div>
  {(modal || deleteTarget) && <div className="modal-backdrop" onMouseDown={(e) => { if (e.target === e.currentTarget) { closeModal(); setDeleteTarget(null); } }}>
    {modal && <div className="modal-card" role="dialog" aria-modal="true" aria-labelledby="modal-title"><div className="modal-heading"><div><p className="eyebrow accent">{modal.mode === "add" ? "New item" : "Update item"}</p><h2 id="modal-title">{modal.mode === "add" ? "Add product" : "Edit product"}</h2></div><button className="close-button" onClick={closeModal} aria-label="Close dialog"><Icon name="close" size={18} /></button></div><form onSubmit={submit}><div className="field"><label htmlFor="product-name">Product name</label><input id="product-name" ref={nameInput} value={form.name} onChange={(e) => setField("name", e.target.value)} placeholder="e.g. Desk lamp" /></div><div className="form-row"><div className="field"><label htmlFor="product-price">Price</label><div className="input-prefix"><span>$</span><input id="product-price" type="number" step="0.01" min="0" value={form.price} onChange={(e) => setField("price", e.target.value)} placeholder="0.00" /></div></div><div className="field"><label htmlFor="product-quantity">Quantity</label><input id="product-quantity" type="number" min="0" step="1" value={form.quantity} onChange={(e) => setField("quantity", e.target.value)} placeholder="0" /></div></div>{error && <p className="form-error" role="alert">{error}</p>}<div className="modal-actions"><button type="button" className="secondary-button" onClick={closeModal}>Cancel</button><button type="submit" className="primary-button" disabled={isSaving}>{isSaving ? "Saving..." : modal.mode === "add" ? "Add product" : "Save changes"}</button></div></form></div>}
    {deleteTarget && <div className="modal-card confirm-card" role="dialog" aria-modal="true" aria-labelledby="delete-title"><div className="confirm-icon"><Icon name="trash" size={21} /></div><h2 id="delete-title">Remove {deleteTarget.name}?</h2><p>This product will be removed from your inventory. This action can’t be undone.</p>{deleteError && <p className="form-error" role="alert">{deleteError}</p>}<div className="modal-actions"><button className="secondary-button" onClick={() => setDeleteTarget(null)} disabled={isDeleting}>Keep product</button><button className="delete-button" onClick={() => void confirmDelete()} disabled={isDeleting}>{isDeleting ? "Removing..." : "Remove product"}</button></div></div>}
  </div>}{notice && <div className="toast" role="status"><span className="toast-check"><Icon name="check" size={14} /></span>{notice}</div>}</main>;
}
