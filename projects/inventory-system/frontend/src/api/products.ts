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
