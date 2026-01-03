export * from "./models/auth";
import { pgTable, text, serial, boolean, timestamp, integer, decimal, jsonb } from "drizzle-orm/pg-core";
import { createInsertSchema } from "drizzle-zod";
import { z } from "zod";

export const games = pgTable("games", {
  id: serial("id").primaryKey(),
  name: text("name").notNull(),
  type: text("type").notNull(), // 'football' | 'basketball'
  status: text("status").notNull().default("open"), // 'open' | 'active' | 'started'
  pricePerSquare: decimal("price_per_square", { precision: 10, scale: 2 }).default("1.00"),
  topNumbers: integer("top_numbers").array(),
  leftNumbers: integer("left_numbers").array(),
  scoreData: jsonb("score_data").$type<{
    team1: { name: string; score: number; quarters: number[] };
    team2: { name: string; score: number; quarters: number[] };
    currentQuarter: number;
    statusText: string;
  }>(),
  createdAt: timestamp("created_at").defaultNow(),
});

export const squareSelections = pgTable("square_selections", {
  id: serial("id").primaryKey(),
  gameId: integer("game_id").notNull(),
  userId: text("user_id").notNull(),
  row: integer("row").notNull(),
  col: integer("col").notNull(),
  playerName: text("player_name").notNull(),
  createdAt: timestamp("created_at").defaultNow(),
});

export const insertGameSchema = createInsertSchema(games).omit({ id: true, createdAt: true });
export const insertSquareSelectionSchema = createInsertSchema(squareSelections).omit({ id: true, createdAt: true });

export type Game = typeof games.$inferSelect;
export type InsertGame = z.infer<typeof insertGameSchema>;
export type SquareSelection = typeof squareSelections.$inferSelect;
export type InsertSquareSelection = z.infer<typeof insertSquareSelectionSchema>;

// Explicit API types
export type GameResponse = Game;
export type SquareSelectionResponse = SquareSelection;
export type UpdateGameRequest = Partial<InsertGame>;
