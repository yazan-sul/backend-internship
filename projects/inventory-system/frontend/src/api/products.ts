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
export async function getProducts(signal?: AbortSignal): Promise<Product[]> {
  const response = await fetch("/api/products", { signal });

  if (!response.ok) {
    throw new Error("The inventory could not be loaded.");
  }

  return response.json() as Promise<Product[]>;
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
