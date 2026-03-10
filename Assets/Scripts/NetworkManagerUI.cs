using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {

    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake() {
        //two buttons, two paths, pretty simple
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}