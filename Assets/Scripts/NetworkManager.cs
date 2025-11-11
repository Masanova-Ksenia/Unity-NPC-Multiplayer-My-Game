using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Network Settings")]
    public string gameVersion = "1.0";
    public byte maxPlayersPerRoom = 4;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Подключение к Photon...");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Подключено к Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Вошли в Lobby");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Случайная комната не найдена, создаём новую");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayersPerRoom;
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Вошли в комнату: " + PhotonNetwork.CurrentRoom.Name);
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        PhotonNetwork.Instantiate("NetworkPlayer", spawnPosition, Quaternion.identity);
    }
}