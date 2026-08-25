import { Icon } from "./Icon";
import type { Product, ProductSortField, SortDirection } from "../api/products";

type ProductTableProps = {
  products: Product[];
  sortBy: ProductSortField;
  sortDirection: SortDirection;
  onSort: (field: ProductSortField) => void;
  onEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
};

function SortableHeader({ label, field, sortBy, sortDirection, onSort }: { label: string; field: ProductSortField; sortBy: ProductSortField; sortDirection: SortDirection; onSort: (field: ProductSortField) => void }) {
  const active = sortBy === field;
  return <th><button className={`sort-button ${active ? "active" : ""}`} onClick={() => onSort(field)} aria-label={`Sort by ${label}`}>
    {label}<span className="sort-indicator" aria-hidden="true">{active ? (sortDirection === "asc" ? "↑" : "↓") : "↕"}</span>
  </button></th>;
}

const currency = (value: number) => new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(value);

export function ProductTable({ products, sortBy, sortDirection, onSort, onEdit, onDelete }: ProductTableProps) {
  return <div className="table-wrap"><table><thead><tr>
    <SortableHeader label="Product" field="name" {...{ sortBy, sortDirection, onSort }} />
    <SortableHeader label="Price" field="price" {...{ sortBy, sortDirection, onSort }} />
    <SortableHeader label="In stock" field="quantity" {...{ sortBy, sortDirection, onSort }} />
    <SortableHeader label="Inventory value" field="inventoryValue" {...{ sortBy, sortDirection, onSort }} />
    <th><span className="sr-only">Actions</span></th>
  </tr></thead><tbody>{products.map((product) => <tr key={product.id}>
    <td><div className="product-cell"><div className="product-icon"><Icon name="box" size={17} /></div><div><strong>{product.name}</strong><span>SKU-{String(product.id).padStart(4, "0")}</span></div></div></td>
    <td>{currency(product.price)}</td>
    <td><span className={`stock-pill ${product.quantity === 0 ? "out" : product.quantity < 10 ? "low" : "good"}`}><span />{product.quantity === 0 ? "Out of stock" : product.quantity < 10 ? `${product.quantity} left` : `${product.quantity} units`}</span></td>
    <td>{currency(product.price * product.quantity)}</td>
    <td><div className="row-actions"><button className="icon-button" onClick={() => onEdit(product)} aria-label={`Edit ${product.name}`} title="Edit product"><Icon name="edit" size={16} /></button><button className="icon-button danger" onClick={() => onDelete(product)} aria-label={`Delete ${product.name}`} title="Delete product"><Icon name="trash" size={16} /></button></div></td>
  </tr>)}</tbody></table></div>;
}
