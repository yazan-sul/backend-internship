export type Product = {
  id: number;
  name: string;
  price: number;
  quantity: number;
  createdAt?: string;
  updatedAt?: string;
};

export type ProductSortField = "name" | "price" | "quantity" | "inventoryValue";
export type SortDirection = "asc" | "desc";

export type ProductPage = {
  items: Product[];
  totalCount: number;
  page: number;
  pageSize: number;
};

/**
 * Loads all products currently persisted by the inventory API.
 */
export async function getProducts(
  search = "",
  options: { page?: number; pageSize?: number; sortBy?: ProductSortField; sortDirection?: SortDirection; signal?: AbortSignal } = {},
): Promise<ProductPage> {
  const params = new URLSearchParams();
  if (search.trim()) params.set("search", search.trim());
  params.set("page", String(options.page ?? 1));
  params.set("pageSize", String(options.pageSize ?? 10));
  params.set("sortBy", options.sortBy ?? "name");
  params.set("sortDirection", options.sortDirection ?? "asc");
  const response = await fetch(`/api/products${params.size ? `?${params}` : ""}`, { signal: options.signal });

  if (!response.ok) {
    throw new Error("The inventory could not be loaded.");
  }

  return response.json() as Promise<ProductPage>;
}

export async function createProduct(input: { name: string; price: number; quantity: number }): Promise<Product> {
  const response = await fetch("/api/products", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null) as { message?: string } | null;
    throw new Error(body?.message ?? "The product could not be added.");
  }

  return response.json() as Promise<Product>;
}

/**
 * Permanently removes a product from the inventory API.
 */
export async function deleteProduct(id: number): Promise<void> {
  const response = await fetch(`/api/products/${id}`, { method: "DELETE" });

  if (!response.ok) {
    const body = await response.json().catch(() => null) as { message?: string } | null;
    throw new Error(body?.message ?? "The product could not be removed.");
  }
}

/**
 * Persists changes to an existing product.
 */
export async function updateProduct(id: number, input: Pick<Product, "name" | "price" | "quantity">): Promise<Product> {
  const response = await fetch(`/api/products/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null) as { message?: string } | null;
    throw new Error(body?.message ?? "The product could not be updated.");
  }

  return response.json() as Promise<Product>;
}
