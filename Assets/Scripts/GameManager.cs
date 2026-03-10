using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {

    public static GameManager Instance { get; private set; }

    public event EventHandler<GridClickArgs> GridClicked;
    public class GridClickArgs : EventArgs {
        public int col;
        public int row;
        public Mark mark;
    }

    public event EventHandler MatchStarted;
    public event EventHandler<WinArgs> MatchWon;
    public class WinArgs : EventArgs {
        public WinLine winLine;
    }
    public event EventHandler TurnChanged;

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
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public WinLineDir dir;
    }

    private Mark myMark;
    private NetworkVariable<Mark> turnMark = new NetworkVariable<Mark>();
    private Mark[,] board;
    private List<WinLine> winLines;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one GameManager! " + transform + " - " + Instance);
        }
        Instance = this;
        board = new Mark[3, 3];

        winLines = new List<WinLine> {
            new WinLine { 
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) }, 
                centerGridPosition = new Vector2Int(0, 1),
                dir = WinLineDir.Vertical
            },
            new WinLine { 
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) }, 
                centerGridPosition = new Vector2Int(1, 1),
                dir = WinLineDir.Vertical
            },
            new WinLine { 
                gridVector2IntList = new List<Vector2Int> { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2) }, 
                centerGridPosition = new Vector2Int(2, 1),
                dir = WinLineDir.Vertical
            },
            new WinLine { gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) }, centerGridPosition = new Vector2Int(1, 0), dir = WinLineDir.Horizontal },
            new WinLine { gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) }, centerGridPosition = new Vector2Int(1, 1), dir = WinLineDir.Horizontal },
            new WinLine { gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) }, centerGridPosition = new Vector2Int(1, 2), dir = WinLineDir.Horizontal },
            new WinLine { gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) }, centerGridPosition = new Vector2Int(1, 1), dir = WinLineDir.DiagonalMain },
            new WinLine { gridVector2IntList = new List<Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) }, centerGridPosition = new Vector2Int(1, 1), dir = WinLineDir.DiagonalAnti },
        };
    }
    

    public override void OnNetworkSpawn() {
        Debug.Log("OnNetworkSpawn: " + NetworkManager.Singleton.LocalClientId);
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

    private void OnTurnMarkChanged(Mark oldMark, Mark newMark) {
        TurnChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClientConnected(ulong clientId) {
        if (NetworkManager.Singleton.ConnectedClients.Count == 2) {
            turnMark.Value = Mark.X;
            NotifyMatchStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyMatchStartedRpc() {
        //everyone gets the "go time" signal at once
        MatchStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void TryPlaceRpc(int col, int row, Mark mark) {
        Debug.Log("Clicked Grid Position: " + col + ", " + row);
        //quick sanity check so people cant place twice in a row
        if (mark != turnMark.Value) {
            Debug.Log("It's not your turn!");
            return;
        }

        //no overwriting squares
        if (board[col, row] != Mark.None) {
            Debug.Log("This grid position is already taken!");
            return;
        }
        board[col, row] = mark;

        GridClicked?.Invoke(this, new GridClickArgs {
            col = col,
            row = row,
            mark = mark,
        });

        switch (turnMark.Value) {
            default:
            case Mark.X:
                turnMark.Value = Mark.O;
                break;
            case Mark.O:
                turnMark.Value = Mark.X;
                break;
        }

        //after every move, just see if somebody won
        CheckForWin();

    }


    private bool IsWinLine(WinLine winLine) {
        return IsWinLine(
            board[winLine.gridVector2IntList[0].x, winLine.gridVector2IntList[0].y],
            board[winLine.gridVector2IntList[1].x, winLine.gridVector2IntList[1].y],
            board[winLine.gridVector2IntList[2].x, winLine.gridVector2IntList[2].y]
        );
    }

    private bool IsWinLine(Mark a, Mark b, Mark c) {
        return a != Mark.None && a == b && b == c;
    }

    private void CheckForWin() {
        foreach(WinLine winLine in winLines) {
            if (IsWinLine(winLine)) {
                Debug.Log("We have a winner!");
                //game is over so nobody should be able to place anything else
                turnMark.Value = Mark.None;
                MatchWon?.Invoke(this, new WinArgs {
                    winLine = winLine
                });
                break;
            }
        }
    } 




    public Mark GetMyMark() {
        return myMark;
    }

    public Mark GetTurnMark() {
        return turnMark.Value;
    }

}