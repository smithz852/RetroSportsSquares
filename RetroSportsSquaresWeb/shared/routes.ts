import { z } from "zod";
import { insertGameSchema, games, squareSelections, insertSquareSelectionSchema } from "./schema";

export const api = {
  games: {
    list: {
      method: "GET" as const,
      path: "/api/games",
      responses: {
        200: z.array(z.custom<typeof games.$inferSelect>()),
      },
    },
    get: {
      method: "GET" as const,
      path: "/api/games/:id",
      responses: {
        200: z.custom<typeof games.$inferSelect>(),
        404: z.object({ message: z.string() }),
      },
    },
    create: {
      method: "POST" as const,
      path: "/api/games",
      input: insertGameSchema,
      responses: {
        201: z.custom<typeof games.$inferSelect>(),
      },
    },
    update: {
      method: "PATCH" as const,
      path: "/api/games/:id",
      input: insertGameSchema.partial(),
      responses: {
        200: z.custom<typeof games.$inferSelect>(),
        404: z.object({ message: z.string() }),
      },
    },
  },
  selections: {
    list: {
      method: "GET" as const,
      path: "/api/games/:gameId/selections",
      responses: {
        200: z.array(z.custom<typeof squareSelections.$inferSelect>()),
      },
    },
    create: {
      method: "POST" as const,
      path: "/api/games/:gameId/selections",
      input: insertSquareSelectionSchema.omit({ gameId: true }),
      responses: {
        201: z.custom<typeof squareSelections.$inferSelect>(),
        400: z.object({ message: z.string() }),
      },
    },
    clear: {
      method: "DELETE" as const,
      path: "/api/games/:gameId/selections",
      responses: {
        204: z.void(),
      },
    },
  },
};

export function buildUrl(path: string, params?: Record<string, string | number>): string {
  let url = path;
  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (url.includes(`:${key}`)) {
        url = url.replace(`:${key}`, String(value));
      }
    });
  }
  return url;
}
