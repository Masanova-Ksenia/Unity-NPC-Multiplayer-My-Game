using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MultiplayerLobbyUI : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public GameObject lobbyPanel;
    public TMP_InputField playerNameInput;
    public TMP_InputField roomNameInput;
    public TMP_Text statusText;
    public TMP_Text playersInRoomText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button startGameButton;
    public Button exitToMenuButton; // НОВАЯ кнопка

    private bool isMultiplayerMode = false;
    private float connectionTimeout = 10f;
    private float connectionTimer = 0f;
    private bool isConnecting = false;
    private bool isInLobby = false; // НОВЫЙ флаг

    void Update()
    {
        // БЛОКИРОВКА ESC в лобби
        if (isInLobby && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC заблокирован в лобби");
            return;
        }

        // Показывать состояние Photon каждую секунду при подключении
        if (isConnecting && Time.frameCount % 60 == 0)
        {
            Debug.Log(">>> Photon State: " + PhotonNetwork.NetworkClientState);
            Debug.Log(">>> IsConnected: " + PhotonNetwork.IsConnected);
            Debug.Log(">>> IsConnectedAndReady: " + PhotonNetwork.IsConnectedAndReady);
        }

        // Таймаут подключения
        if (isConnecting)
        {
            connectionTimer += Time.unscaledDeltaTime;

            if (connectionTimer > connectionTimeout)
            {
                Debug.LogError("Таймаут подключения!");
                UpdateStatus("Не удалось подключиться. Проверьте интернет.");
                isConnecting = false;

                if (createRoomButton != null) createRoomButton.interactable = false;
                if (joinRoomButton != null) joinRoomButton.interactable = false;
            }
        }
    }

    void Start()
    {
        int gameMode = PlayerPrefs.GetInt("GameMode", 0);
        isMultiplayerMode = (gameMode == 1);

        Debug.Log("=== MultiplayerLobbyUI Start ===");
        Debug.Log("GameMode: " + gameMode + " (0=Single, 1=Multi)");

        if (isMultiplayerMode)
        {
            Debug.Log("РЕЖИМ МУЛЬТИПЛЕЕРА");

            ShowLobby();

            if (createRoomButton != null) createRoomButton.interactable = false;
            if (joinRoomButton != null) joinRoomButton.interactable = false;

            // Отключиться если уже подключены
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Уже подключен, отключаюсь...");
                PhotonNetwork.Disconnect();
            }

            Invoke("ConnectToPhoton", 0.5f);
        }
        else
        {
            Debug.Log("РЕЖИМ ОДИНОЧНОЙ ИГРЫ");
            HideLobby();
            StartSinglePlayerGame();
        }
    }

    void ShowLobby()
    {
        Debug.Log("ShowLobby() вызван");

        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(true);
        }

        isInLobby = true; // Флаг что мы в лобби
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);

        if (exitToMenuButton != null)
            exitToMenuButton.gameObject.SetActive(true);

        UpdateStatus("Введите имя и название комнаты");
    }

    void HideLobby()
    {
        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(false);
        }

        isInLobby = false; // Больше не в лобби
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void StartSinglePlayerGame()
    {
        Debug.Log("Запуск одиночной игры");

        // ЗАПУСТИТЬ музыку в одиночной игре
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RestartMusic();
            Debug.Log("Музыка запущена (одиночная игра)");
        }

        CoinSpawnerDynamic coinSpawner = FindObjectOfType<CoinSpawnerDynamic>();
        if (coinSpawner != null)
        {
            coinSpawner.numberOfPlayers = 1;
            coinSpawner.GenerateCoins();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(true);
        }
    }

    void ConnectToPhoton()
    {
        UpdateStatus("Подключение к серверу...");

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";

        isConnecting = true;
        connectionTimer = 0f;

        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Уже подключен");
            isConnecting = false;
            OnConnectedToMaster();
        }
        else
        {
            Debug.Log("Подключение к Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Подключено к Master Server");
        UpdateStatus("одключено! Создайте или присоединитесь");

        isConnecting = false;

        if (createRoomButton != null) createRoomButton.interactable = true;
        if (joinRoomButton != null) joinRoomButton.interactable = true;

        // УБРАЛИ: Invoke("ConnectToPhoton", 2f); - это было лишнее!
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Отключен: " + cause);
        UpdateStatus("Отключен. Переподключение...");

        if (createRoomButton != null) createRoomButton.interactable = false;
        if (joinRoomButton != null) joinRoomButton.interactable = false;

        // Переподключиться только если всё ещё в лобби
        if (isInLobby)
        {
            Invoke("ConnectToPhoton", 2f);
        }
    }

    public void OnCreateRoomClicked()
    {
        string playerName = playerNameInput != null && playerNameInput.text.Trim().Length > 0
            ? playerNameInput.text.Trim()
            : "Player_" + Random.Range(1000, 9999);

        string roomName = roomNameInput != null && roomNameInput.text.Trim().Length > 0
            ? roomNameInput.text.Trim()
            : "Room_" + Random.Range(1000, 9999);

        PhotonNetwork.NickName = playerName;

        Debug.Log("=== Создание комнаты ===");
        Debug.Log("Игрок: " + playerName + ", Комната: " + roomName);

        UpdateStatus("Создание комнаты: " + roomName + "...");

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.CreateRoom(roomName, roomOptions);

        if (createRoomButton != null) createRoomButton.interactable = false;
        if (joinRoomButton != null) joinRoomButton.interactable = false;
    }

    public void OnJoinRoomClicked()
    {
        string playerName = playerNameInput != null && playerNameInput.text.Trim().Length > 0
            ? playerNameInput.text.Trim()
            : "Player_" + Random.Range(1000, 9999);

        string roomName = roomNameInput != null ? roomNameInput.text.Trim() : "";

        if (roomName.Length == 0)
        {
            UpdateStatus("Введите название комнаты!");
            return;
        }

        PhotonNetwork.NickName = playerName;

        UpdateStatus("Подключение: " + roomName + "...");
        PhotonNetwork.JoinRoom(roomName);

        if (createRoomButton != null) createRoomButton.interactable = false;
        if (joinRoomButton != null) joinRoomButton.interactable = false;
    }

    // НОВЫЙ МЕТОД: Выход в меню
    public void OnExitToMenuClicked()
    {
        Debug.Log("Выход в главное меню из лобби");

        // Отключиться от Photon
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        // Снять паузу
        Time.timeScale = 1f;

        // Остановить музыку
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        // Загрузить меню
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("В комнате: " + PhotonNetwork.CurrentRoom.Name);
        UpdateStatus("В комнате: " + PhotonNetwork.CurrentRoom.Name);

        UpdatePlayersList();

        GameObject singlePlayer = GameObject.FindGameObjectWithTag("Player");
        if (singlePlayer != null)
        {
            singlePlayer.SetActive(false);
        }

        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }

        // Скрыть кнопку выхода в меню
        if (exitToMenuButton != null)
        {
            exitToMenuButton.gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            CoinSpawnerDynamic coinSpawner = FindObjectOfType<CoinSpawnerDynamic>();
            if (coinSpawner != null)
            {
                coinSpawner.numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                coinSpawner.GenerateCoins();
            }
        }
    }

    public void OnStartGameClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            UpdateStatus("Только хост может начать!");
            return;
        }

        Debug.Log("Хост запускает игру!");
        photonView.RPC("RPC_StartGame", RpcTarget.All);
    }

    [PunRPC]
    void RPC_StartGame()
    {
        Debug.Log("Игра начинается!");

        HideLobby();

        // ЗАПУСТИТЬ музыку после входа в мультиплеер
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RestartMusic();
            Debug.Log("Музыка запущена (мультиплеер)");
        }

        SpawnNetworkPlayer();
    }

    void SpawnNetworkPlayer()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        Vector3 spawnPos;
        if (spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            spawnPos = spawnPoints[randomIndex].transform.position;
        }
        else
        {
            spawnPos = new Vector3(
                Random.Range(-10f, 10f),
                2f,
                Random.Range(-10f, 10f)
            );
        }

        PhotonNetwork.Instantiate("NetworkPlayer", spawnPos, Quaternion.identity);
        Debug.Log("Игрок создан: " + PhotonNetwork.NickName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(" " + newPlayer.NickName + " присоединился");
        UpdateStatus(" " + newPlayer.NickName + " присоединился!");
        UpdatePlayersList();

        if (PhotonNetwork.IsMasterClient)
        {
            CoinSpawnerDynamic coinSpawner = FindObjectOfType<CoinSpawnerDynamic>();
            if (coinSpawner != null)
            {
                coinSpawner.numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                coinSpawner.GenerateCoins();
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(" " + otherPlayer.NickName + " вышел");
        UpdatePlayersList();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Ошибка входа: " + message);
        UpdateStatus("Ошибка: " + message);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (createRoomButton != null) createRoomButton.interactable = true;
        if (joinRoomButton != null) joinRoomButton.interactable = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Ошибка создания: " + message);
        UpdateStatus("Ошибка: " + message);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (createRoomButton != null) createRoomButton.interactable = true;
        if (joinRoomButton != null) joinRoomButton.interactable = true;
    }

    void UpdatePlayersList()
    {
        if (playersInRoomText != null && PhotonNetwork.InRoom)
        {
            string playersList = "Игроки (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "):\n\n";

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                playersList += "• " + player.NickName;
                if (player.IsMasterClient)
                    playersList += " ";
                playersList += "\n";
            }

            playersInRoomText.text = playersList;
        }
    }

    void UpdateStatus(string message)
    {
        Debug.Log("Status: " + message);
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}