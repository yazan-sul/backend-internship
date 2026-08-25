export type Product = {
  id: number;
  name: string;
  price: number;
  quantity: number;
  createdAt?: string;
  updatedAt?: string;
};

/**
 * Loads all products currently persisted by the inventory API.
 */
export async function getProducts(search = "", signal?: AbortSignal): Promise<Product[]> {
  const params = new URLSearchParams();
  if (search.trim()) params.set("search", search.trim());
  const response = await fetch(`/api/products${params.size ? `?${params}` : ""}`, { signal });

  if (!response.ok) {
    throw new Error("The inventory could not be loaded.");
  }

  return response.json() as Promise<Product[]>;
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
