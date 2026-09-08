import { z } from "zod";

const weatherSchema = z
  .object({
    location: z.string(),
    temperature: z.number().finite(),
    humidity: z.number().finite(),
  })
  .strict();

const botActivationSchema = z
  .object({
    bot: z.string(),
    message: z.string(),
  })
  .strict();

export const weatherUpdateResponseSchema = z
  .object({
    weather: weatherSchema,
    activations: z.array(botActivationSchema),
  })
  .strict();

export const apiErrorResponseSchema = z.object({ error: z.string() }).strict();

export type WeatherUpdateResponse = z.infer<typeof weatherUpdateResponseSchema>;
