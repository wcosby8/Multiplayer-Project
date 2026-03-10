using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {

    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private UnityTransport transport;

    private const string HOST_IP = "192.168.1.71";
    private const ushort PORT = 7777;

    private void Awake() {
        //two buttons, two paths, pretty simple
        if (transport == null) {
            var nm = NetworkManager.Singleton;
            if (nm == null) {
                Debug.LogError("networkmanagerui couldnt find a networkmanager in the scene");
                return;
            }
            transport = nm.GetComponent<UnityTransport>();
            if (transport == null) {
                Debug.LogError("networkmanagerui couldnt find a unitytransport on the networkmanager");
                return;
            }
        }
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