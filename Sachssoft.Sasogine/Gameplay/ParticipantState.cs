namespace Sachssoft.Sasogine.Gameplay
{
    /// <summary>
    /// Defines the possible states of a participant in a game lifecycle.
    /// Covers lobby, gameplay, post-game, network, and administrative scenarios.
    /// </summary>
    public enum ParticipantState
    {
        /// <summary>No defined state / empty slot.</summary>
        None,           // Kein Status gesetzt (Platzhalter)

        // --- Meta / Lobby ---

        /// <summary>Participant is establishing a connection to the server or host.</summary>
        Connecting,     // Verbindung wird hergestellt

        /// <summary>Participant is authenticating (e.g., login, account check).</summary>
        Authenticating, // Spieler authentifiziert sich

        /// <summary>Participant is in a lobby, waiting for the game to start.</summary>
        InLobby,        // In der Lobby, vor Spielstart

        /// <summary>Participant is waiting in matchmaking queue.</summary>
        Matchmaking,    // Warteschlange für Matchmaking

        /// <summary>Participant must confirm readiness (ready check).</summary>
        ReadyCheck,     // "Bereit?"-Abfrage

        /// <summary>Participant is loading assets or synchronizing before match start.</summary>
        Loading,        // Lädt Assets / Synchronisierung

        /// <summary>Participant is ready to start the game.</summary>
        Ready,          // Bereit zum Spielstart

        /// <summary>Participant is waiting for other players or round to begin.</summary>
        Waiting,        // Wartet auf andere Spieler / Rundenstart

        /// <summary>Participant is in a selection phase (character, items, tiles, etc.).</summary>
        Selection,      // Auswahlphase (Charakter, Items, Kacheln)

        // --- Gameplay ---

        /// <summary>Participant is spawning for the first time.</summary>
        Spawning,       // Erste Platzierung im Spiel

        /// <summary>Participant is alive and actively playing.</summary>
        Active,         // Lebendig und aktiv

        /// <summary>Participant is in the game but not giving input (AFK, timeout).</summary>
        Idle,           // Untätig / AFK

        /// <summary>Participant has paused the game.</summary>
        Paused,         // Spiel pausiert

        /// <summary>Participant is interacting with objects or UI.</summary>
        Interacting,    // Mit Objekten/UI beschäftigt

        /// <summary>Participant is performing an explicit action (skill, item use, etc.).</summary>
        PerformingAction, // Führt Aktion aus

        /// <summary>Participant is in a trade window or exchange state.</summary>
        Trading,        // Handel / Austausch

        /// <summary>Participant is hidden/stealthed from others.</summary>
        Hidden,         // Unsichtbar / getarnt

        /// <summary>Participant is temporarily invincible (e.g., after respawn, power-up).</summary>
        Invincible,     // Unverwundbar

        /// <summary>Participant is stunned and cannot act.</summary>
        Stunned,        // Benommen / bewegungsunfähig

        /// <summary>Participant is disabled due to game mechanics or restrictions.</summary>
        Disabled,       // Deaktiviert / gesperrt im Spiel

        // --- Death & End ---

        /// <summary>Participant is dead but may respawn.</summary>
        Killed,         // Gestorben (evtl. Respawn möglich)

        /// <summary>Participant is in respawn transition phase.</summary>
        Respawning,     // Respawn läuft

        /// <summary>Participant has been permanently eliminated from the match.</summary>
        Eliminated,     // Dauerhaft ausgeschieden

        /// <summary>Participant has finished the match/level successfully.</summary>
        Finished,       // Ziel erreicht

        /// <summary>Participant is spectating the game.</summary>
        Spectating,     // Zuschauer-Modus

        // --- Post Game ---

        /// <summary>Participant is in post-game phase (scoreboard, waiting room).</summary>
        PostGame,       // Nach Match-Ende, in der Session

        /// <summary>Participant is receiving rewards, XP, or loot.</summary>
        Rewarding,      // Belohnungen / XP erhalten

        /// <summary>Participant is reviewing a replay or statistics.</summary>
        Reviewing,      // Replay / Statistik anschauen

        /// <summary>Participant has completely exited the game.</summary>
        Exited,         // Spiel verlassen

        // --- Network ---

        /// <summary>Participant is synchronizing state/data with the server.</summary>
        Syncing,        // Synchronisierung läuft

        /// <summary>Participant is experiencing network lag.</summary>
        Lagging,        // Netzwerk-Lag

        /// <summary>Participant is out of sync with the server/host.</summary>
        OutOfSync,      // Spielzustand passt nicht mehr

        /// <summary>Participant has lost connection.</summary>
        Disconnected,   // Verbindung verloren

        /// <summary>Participant is attempting to reconnect.</summary>
        Reconnecting,   // Verbindungsversuch

        // --- Administration ---

        /// <summary>Participant left the game voluntarily.</summary>
        Left,           // Spieler hat freiwillig verlassen

        /// <summary>Participant is banned and cannot participate.</summary>
        Banned,         // Gesperrt

        /// <summary>Participant is suspended temporarily and cannot play.</summary>
        Suspended       // Vorübergehend ausgeschlossen
    }

}
