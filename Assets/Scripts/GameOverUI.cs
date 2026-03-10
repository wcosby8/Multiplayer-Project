using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI xWinText;
    [SerializeField] private Color xWinColor;
    [SerializeField] private Color xLoseColor;
    [SerializeField] private Button playAgainButton;

    private void Awake(){
        playAgainButton.onClick.AddListener(() => {
            GameManager.Instance.RematchRpc();
        });
    }


    private void Start(){
        GameManager.Instance.GameWon += GameManager_OnGameWon;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        Hide();
    }
    private void GameManager_OnRematch(object sender, EventArgs e){
        Hide();
    }


    private void GameManager_OnGameWon(object sender, GameManager.GameWonArgs e){
        //this is just checking if the winner mark matches the local player's mark
        if(e.winPlayerType == GameManager.Instance.GetMyMark()){
            xWinText.text = "you win!";
            xWinText.color = xWinColor;
        }else{
            xWinText.text = "you lose!";
            xWinText.color = xLoseColor;
        }
        Show();
    }

    private void Show(){
        gameObject.SetActive(true);
    }
    private void Hide(){
        gameObject.SetActive(false);
    }
}
