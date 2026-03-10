using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{

    private const float CELL_SIZE = 3.1f;

    [SerializeField] private Transform xPrefab;
    [SerializeField] private Transform oPrefab;
    [SerializeField] private Transform winLinePrefab;

    private void Start() {
            //just listening for game events and spawning visuals
            GameManager.Instance.GridClicked += GameManager_OnGridClicked;
            GameManager.Instance.MatchWon += GameManager_OnMatchWon;
    }

    private void GameManager_OnMatchWon(object sender, GameManager.WinArgs e) {
        float eulerZ = 0;
        switch(e.winLine.dir) {
            default:
            case GameManager.WinLineDir.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.WinLineDir.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.WinLineDir.DiagonalMain:
                eulerZ = 45f;
                break;
            case GameManager.WinLineDir.DiagonalAnti:
                eulerZ = -45f;
                break;       
        }

        Transform winLineTransform = Instantiate(winLinePrefab, ToWorldPos(e.winLine.centerGridPosition.x, e.winLine.centerGridPosition.y), Quaternion.Euler(0,0,eulerZ));
        winLineTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private void GameManager_OnGridClicked(object sender, GameManager.GridClickArgs e) {
        SpawnMarkRpc(e.col, e.row, e.mark);
    }

    [Rpc(SendTo.Server)]
    private void SpawnMarkRpc(int col, int row, GameManager.Mark mark){
        Debug.Log("SpawnObject");
        //server spawns the actual network objects so both clients stay in sync
        Transform prefab;
        switch(mark){
            default:
            case GameManager.Mark.X:
                prefab = xPrefab;
                break;
            case GameManager.Mark.O:
                prefab = oPrefab;
                break;
        }
        Transform spawnedMarkTransform = Instantiate(prefab, ToWorldPos(col, row), Quaternion.identity);
        spawnedMarkTransform.GetComponent<NetworkObject>().Spawn(true);
        
    }

    private Vector2 ToWorldPos(int col, int row) {
        //tiny helper so the board doesnt have a bunch of repeated math everywhere
        return new Vector2(-CELL_SIZE + col * CELL_SIZE, -CELL_SIZE + row * CELL_SIZE);
    }
}
