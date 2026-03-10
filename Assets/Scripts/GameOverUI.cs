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

    private void Start(){
        GameManager.Instance.GameWon += HandleGameWon;
        GameManager.Instance.RematchStarted += HandleRematchStarted;
        Hide();
    }

    private void Awake(){
        //when you mash play again we just ask the server for a fresh board
        playAgainButton.onClick.AddListener(() => {
            GameManager.Instance.StartRematchRpc();
        });
    }

    private void HandleRematchStarted(object sender, EventArgs e){
        Hide();
    }


    private void HandleGameWon(object sender, GameManager.GameWonArgs e){
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
