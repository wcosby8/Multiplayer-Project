using UnityEngine;

public class PlayerUI : MonoBehaviour {
    [SerializeField] private GameObject xArrow;
    [SerializeField] private GameObject oArrow;
    [SerializeField] private GameObject xYouText;
    [SerializeField] private GameObject oYouText;


    private void Awake() {
        //starting clean so nothing flashes on for a frame
        xArrow.SetActive(false);
        oArrow.SetActive(false);
        xYouText.SetActive(false);
        oYouText.SetActive(false);
    }

    private void Start() {
        GameManager.Instance.GameStarted += GameManager_OnGameStarted;
        GameManager.Instance.TurnChanged += GameManager_OnTurnChanged;
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e) {
        if (GameManager.Instance.GetMyMark() == GameManager.Mark.X) {
            xYouText.SetActive(true);
        } else {
            oYouText.SetActive(true);
        }
        RefreshTurnArrow();
    }

    private void GameManager_OnTurnChanged(object sender, System.EventArgs e) {
        RefreshTurnArrow();
    }

    private void RefreshTurnArrow() { 
        //just swaps the little arrow based on whose turn it is
        if(GameManager.Instance.GetTurnMark() == GameManager.Mark.X) {
            xArrow.SetActive(true);
            oArrow.SetActive(false);
        } else {
            xArrow.SetActive(false);
            oArrow.SetActive(true);
        }
    }
}
