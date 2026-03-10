using UnityEngine;

public class GridPosition : MonoBehaviour {

    [SerializeField] private int col;
    [SerializeField] private int row;

    private void OnMouseDown() {
        //this is just "hey server, i clicked this square"
        Debug.Log($"Click: ({col}, {row})");
        GameManager.Instance.TryPlaceRpc(col, row, GameManager.Instance.GetMyMark());
    }
}