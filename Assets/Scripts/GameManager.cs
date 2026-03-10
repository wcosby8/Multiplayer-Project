using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {

    public static GameManager Instance { get; private set; }

    private const int BOARD_SIZE = 3;

    private Mark myMark;
    private readonly NetworkVariable<Mark> turnMark = new NetworkVariable<Mark>();
    private Mark[,] board;
    private List<WinLine> winLines;

    public event EventHandler<SquarePickArgs> SquarePicked;
    public class SquarePickArgs : EventArgs {
        public int x;
        public int y;
        public Mark mark;
    }

    public event EventHandler GameStarted;
    public event EventHandler<GameWonArgs> GameWon;
    public class GameWonArgs : EventArgs {
        public WinLine line;
        public Mark winPlayerType;
    }
    public event EventHandler TurnChanged;
    public event EventHandler RematchStarted;

    public enum Mark {
        None,
        X,
        O
    }


    public enum WinLineDir{
        Horizontal,
        Vertical,
        DiagonalMain,
        DiagonalAnti,
    }

    public struct WinLine{
        public Vector2Int a;
        public Vector2Int b;
        public Vector2Int c;
        public Vector2Int center;
        public WinLineDir dir;
    }

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one GameManager! " + transform + " - " + Instance);
        }
        Instance = this;

        //setup stuff thats the same for everyone
        board = new Mark[BOARD_SIZE, BOARD_SIZE];
        winLines = BuildWinLines();
    }
    

    public override void OnNetworkSpawn() {
        Debug.Log("OnNetworkSpawn: " + NetworkManager.Singleton.LocalClientId);
        //client 0 is always host in this setup, so they get x
        if (NetworkManager.Singleton.LocalClientId == 0) {
            myMark = Mark.X;
        } else {
            myMark = Mark.O;
        }

        //server waits until both people are here before starting
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        turnMark.OnValueChanged += OnTurnMarkChanged;
    }

    public Mark GetMyMark() {
        return myMark;
    }

    public Mark GetTurnMark() {
        return turnMark.Value;
    }

    private void OnTurnMarkChanged(Mark oldMark, Mark newMark) {
        TurnChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClientConnected(ulong clientId) {
        if (NetworkManager.Singleton.ConnectedClients.Count == 2) {
            turnMark.Value = Mark.X;
            BroadcastStartRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BroadcastStartRpc() {
        //everyone gets the "go time" signal at once
        GameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void TryPlaceRpc(int x, int y, Mark mark) {
        Debug.Log("Clicked Grid Position: " + x + ", " + y);

        if (!IsValidMove(x, y, mark)) {
            Debug.Log("It's not your turn!");
            return;
        }

        PlaceMark(x, y, mark);

        SquarePicked?.Invoke(this, new SquarePickArgs { x = x, y = y, mark = mark });

        FlipTurn();
        CheckForWin();

    }

    private bool IsValidMove(int x, int y, Mark mark) {
        //quick sanity check so people cant place twice in a row
        if (mark != turnMark.Value) return false;

        //no overwriting squares
        if (board[x, y] != Mark.None) return false;

        return true;
    }

    private void PlaceMark(int x, int y, Mark mark) {
        board[x, y] = mark;
    }

    private void FlipTurn() {
        //this is the whole "ok your turn now" part
        turnMark.Value = turnMark.Value == Mark.X ? Mark.O : Mark.X;
    }

    private void CheckForWin() {
        //after every move, just see if somebody won
        for (int i = 0; i < winLines.Count; i++) {
            WinLine line = winLines[i];
            if (IsLineWinner(line)) {
                EndGame(line);
                //grab the mark sitting in the middle of this line and send it to everyone
                Mark winner = board[line.center.x, line.center.y];
                TriggerOnGameWinRpc(i, winner);
                break;
            }
        }
    }

    private bool IsLineWinner(WinLine line) {
        Mark a = board[line.a.x, line.a.y];
        if (a == Mark.None) return false;
        return a == board[line.b.x, line.b.y] && a == board[line.c.x, line.c.y];
    }

    private void EndGame(WinLine winningLine) {
        Debug.Log("We have a winner!");
        //game is over so nobody should be able to place anything else
        turnMark.Value = Mark.None;
        
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, Mark winPlayerType){
        WinLine winningLine = winLines[lineIndex];
        GameWon?.Invoke(this, new GameWonArgs {
            line = winningLine,
            //winner is always the mark in the middle of the line
            winPlayerType = winPlayerType,
        });
    }

    [Rpc(SendTo.Server)]
    public void StartRematchRpc() {
        //wipe out the old board state
        for (int x = 0; x < BOARD_SIZE; x++) {
            for (int y = 0; y < BOARD_SIZE; y++) {
                board[x, y] = Mark.None;
            }
        }
        //x always goes first again after a rematch
        turnMark.Value = Mark.X;
        //tell everyone the new round is live
        BroadcastStartRpc();
        NotifyRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyRematchRpc() {
        RematchStarted?.Invoke(this, EventArgs.Empty);
    }

    private List<WinLine> BuildWinLines() {
        var lines = new List<WinLine>(8);

        //rows
        for (int y = 0; y < BOARD_SIZE; y++) {
            lines.Add(new WinLine {
                a = new Vector2Int(0, y),
                b = new Vector2Int(1, y),
                c = new Vector2Int(2, y),
                center = new Vector2Int(1, y),
                dir = WinLineDir.Horizontal
            });
        }

        //cols
        for (int x = 0; x < BOARD_SIZE; x++) {
            lines.Add(new WinLine {
                a = new Vector2Int(x, 0),
                b = new Vector2Int(x, 1),
                c = new Vector2Int(x, 2),
                center = new Vector2Int(x, 1),
                dir = WinLineDir.Vertical
            });
        }

        //diagonals
        lines.Add(new WinLine {
            a = new Vector2Int(0, 0),
            b = new Vector2Int(1, 1),
            c = new Vector2Int(2, 2),
            center = new Vector2Int(1, 1),
            dir = WinLineDir.DiagonalMain
        });
        lines.Add(new WinLine {
            a = new Vector2Int(0, 2),
            b = new Vector2Int(1, 1),
            c = new Vector2Int(2, 0),
            center = new Vector2Int(1, 1),
            dir = WinLineDir.DiagonalAnti
        });

        return lines;
    }
}