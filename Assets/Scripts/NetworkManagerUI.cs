using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {

    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private const string HOST_IP = "192.168.1.71";
    private const ushort PORT = 7777;

    private void Awake() {
        //two buttons, two paths, pretty simple
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        hostButton.onClick.AddListener(() => {
            //host listens on this machine, clients connect via lan ip
            transport.SetConnectionData("0.0.0.0", PORT);
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        clientButton.onClick.AddListener(() => {
            transport.SetConnectionData(HOST_IP, PORT);
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}