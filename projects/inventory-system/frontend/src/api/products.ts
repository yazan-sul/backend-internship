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
