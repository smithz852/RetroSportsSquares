import { useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useLocation } from "wouter";
import { API_BASE_URL } from "@shared/routes";

export function useGameHub(gameId: string) {
  const queryClient = useQueryClient();
  const [, setLocation] = useLocation();
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!gameId) return;
    const token = localStorage.getItem("token");

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/game`, {
        accessTokenFactory: () => token ?? "",
      })
      .withAutomaticReconnect()
      .build();

    // Turn advanced or player skipped — refresh turn queue and board
    connection.on("TurnAdvanced", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
      queryClient.invalidateQueries({ queryKey: ["boardSquares", gameId] });
    });

    // A new player joined — refresh turn queue
    connection.on("PlayerJoined", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
    });

    // Selection phase kicked off by host
    connection.on("SelectionsStarted", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
    });

    // A square was claimed in open mode — refresh the board
    connection.on("SquareSelected", () => {
      queryClient.invalidateQueries({ queryKey: ["boardSquares", gameId] });
    });

    // Host cancelled the game before it started
    connection.on("GameDeleted", () => {
      setLocation("/");
    });

    // A player left — refresh the player list for remaining players
    connection.on("PlayerLeft", () => {
      queryClient.invalidateQueries({ queryKey: ["turnStatus", gameId] });
    });

    connection
      .start()
      .then(() => connection.invoke("JoinGame", gameId))
      .catch(console.error);

    connectionRef.current = connection;

    return () => {
      connection.invoke("LeaveGame", gameId).finally(() => connection.stop());
    };
  }, [gameId]);
}
