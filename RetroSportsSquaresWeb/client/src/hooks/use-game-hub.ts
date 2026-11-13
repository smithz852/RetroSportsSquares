import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useLocation } from "wouter";
import { API_BASE_URL } from "@shared/routes";
import { type ChatMessage } from "@shared/schema";
import { appendChatMessage } from "./use-chat";
import { handleSessionExpired } from "@/lib/api-client";
import { useLiveAnnouncer } from "./use-live-announcer";

const isAuthError = (err: unknown): boolean =>
  err instanceof signalR.HttpError
    ? err.statusCode === 401
    : /\b401\b|unauthorized/i.test(err instanceof Error ? err.message : String(err));

export function useGameHub(gameId: string) {
  const queryClient = useQueryClient();
  const [, setLocation] = useLocation();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const { announce, liveRegion } = useLiveAnnouncer();

  useEffect(() => {
    if (!gameId) return;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/game`, {
        accessTokenFactory: () => localStorage.getItem("token") ?? "",
      })
      .withAutomaticReconnect()
      .build();

    // Turn advanced or player skipped — refresh turn queue and board
    connection.on("TurnAdvanced", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
      queryClient.invalidateQueries({ queryKey: ["boardSquares", gameId] });
      announce("Turn advanced to the next player.");
    });

    // A new player joined — refresh turn queue
    connection.on("PlayerJoined", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
      announce("A new player joined the game.");
    });

    // Selection phase kicked off by host
    connection.on("SelectionsStarted", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
    });

    // A square was claimed in open mode — refresh the board
    connection.on("SquareSelected", () => {
      queryClient.invalidateQueries({ queryKey: ["boardSquares", gameId] });
      announce("A square was selected.");
    });

    // Host cancelled the game before it started
    connection.on("GameDeleted", () => {
      setLocation("/");
    });

    // A player left — refresh the player list for remaining players
    connection.on("PlayerLeft", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
      announce("A player left the game.");
    });

    // Host started the game — trigger outside squares fetch for all players
    connection.on("GameStarted", () => {
      queryClient.invalidateQueries({ queryKey: ["OutsideSquares", gameId] });
      announce("The game has started.");
    });

    // Score data updated by background service — refresh scoreboard for all players
    connection.on("ScoreUpdated", () => {
      queryClient.invalidateQueries({ queryKey: ["gameScoreData", gameId] });
      announce("The score was updated.");
    });

    // Chat is an append-only stream — push into the cache instead of refetching
    connection.on("ChatMessage", (msg: ChatMessage) => {
      queryClient.setQueryData<ChatMessage[]>(["chat", gameId], (old) =>
        appendChatMessage(old, msg),
      );
    });

    // Token expired before negotiate, or expired mid-session and automatic
    // reconnect's negotiate fails too — either way, treat it as a session expiry.
    connection.onclose((err) => {
      if (isAuthError(err)) handleSessionExpired();
    });

    connection
      .start()
      .then(() => connection.invoke("JoinGame", gameId))
      .catch((err) => {
        if (isAuthError(err)) handleSessionExpired();
        else console.error(err);
      });

    connectionRef.current = connection;

    return () => {
      connection.invoke("LeaveGame", gameId).finally(() => connection.stop());
    };
  }, [gameId]);

  return { liveRegion };
}
