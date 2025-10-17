using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private Transform _levelElementHolder;
    [SerializeField] private Transform _explosionMaskHolder;
    [SerializeField] private Transform _brokenWindowHolder;
    [SerializeField] private GameObject[] _levelElements;
    [SerializeField] private int _minimumDistanceBetweenPlayers = 7;
    public int MinimumDistanceBetweenPlayers { get { return _minimumDistanceBetweenPlayers; } }
    [SerializeField] private int _maximumDistanceBetweenPlayers = 30;
    private int _distanceBetweenPlayers;
    [SerializeField] private float _minimumBuildingHeight = -13f;
    [SerializeField] private float _maximumBuildingHeight = -8f;
    private List<LevelElementDetails> _levelElementDetailsList = new();
    private int _numberOfLevelElements;
    private float _totalElementWidth = 0f;
    public List<Vector3> _playerSpawnPointList = new();
    public List<GameObject> _playerSpawnPointArrows = new();
    private List<GameObject> _levelElementGOs = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void BuildLevel()
    {
        if (!IsServer) return;

        ClearCurrentLevelRpc();

        // time to build the level
        ChooseElements();
        PlaceElements();

        GameManager.Instance.UpdateGameState(GameState.SetupPlayers);
    }

    private void PlaceElements()
    {
        float startingXPos = -_totalElementWidth / 2.0f;
        float xOffset = 0f;
        Vector3 newPos;
        GameObject newElement;
        Color randomBuildingColour;
        int ledIndex = 0;
        NetworkObject elementNO;

        foreach (LevelElementDetails led in _levelElementDetailsList)
        {
            newPos = new(startingXPos + xOffset + (led.ElementWidth / 2), led.ElementHeight, 0f);
            newElement = Instantiate(led.ElementPrefab, newPos, Quaternion.identity);
            elementNO = newElement.GetComponent<NetworkObject>();
            elementNO.Spawn(true);
            elementNO.TrySetParent(_levelElementHolder, true);
            randomBuildingColour = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
            // set the building colour on the server
            newElement.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color = randomBuildingColour;
            // save the colour to update on the client using an rpc
            newElement.GetComponent<Building>().SetBuildingSpriteColourRpc(randomBuildingColour);
            xOffset += led.ElementWidth;

            SetSpawnPointArrowListRpc(newElement.GetComponent<NetworkObject>());
            _levelElementGOs.Add(newElement);

            ledIndex++;
        }

        UpdateBuildingSpriteColours();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetSpawnPointArrowListRpc(NetworkObjectReference networkObject)
    {
        if (!networkObject.TryGet(out NetworkObject building))
        {
            Debug.Log("SetSpawnPointArrowListRpc Error: Could not retrieve NetworkObject");
            return;
        }

        // spawn points are the second child of the building (first is GFX)
        for (int i = 0; i < building.transform.GetChild(1).childCount; i++)
        {
            _playerSpawnPointList.Add(building.transform.GetChild(1).GetChild(i).transform.position);
            // first child of the spawn point is the arrow
            _playerSpawnPointArrows.Add(building.transform.GetChild(1).GetChild(i).GetChild(0).gameObject);
            _playerSpawnPointArrows[_playerSpawnPointArrows.Count - 1].GetComponentInChildren<MovePlayerArrow>().SetArrowIndex(_playerSpawnPointArrows.Count - 1);
        }
    }

    private void ChooseElements()
    {
        _distanceBetweenPlayers = Random.Range(_minimumDistanceBetweenPlayers, _maximumDistanceBetweenPlayers);
        _numberOfLevelElements = _distanceBetweenPlayers * 3;
        LevelElementDetails newLevelElementDetails;
        GameObject prefab;
        float prefabWidth;
        float prefabHeight;

        for (int i = 0; i <= _numberOfLevelElements; i++)
        {
            prefab = _levelElements[Random.Range(0, _levelElements.Length)];
            prefabWidth = prefab.transform.GetChild(0).transform.localScale.x;
            prefabHeight = Random.Range(_minimumBuildingHeight, _maximumBuildingHeight);

            newLevelElementDetails = new LevelElementDetails
            {
                ElementPrefab = prefab,
                ElementWidth = prefabWidth,
                ElementHeight = prefabHeight
            };

            _levelElementDetailsList.Add(newLevelElementDetails);
            _totalElementWidth += prefabWidth;
        }

        // if the total width is even, add an extra single building
        if (_totalElementWidth % 2 == 0)
        {
            prefab = _levelElements[0];
            prefabWidth = prefab.GetComponentInChildren<SpriteRenderer>().transform.localScale.x;
            prefabHeight = Random.Range(_minimumBuildingHeight, _maximumBuildingHeight);

            newLevelElementDetails = new LevelElementDetails
            {
                ElementPrefab = prefab,
                ElementWidth = prefabWidth,
                ElementHeight = prefabHeight
            };

            _levelElementDetailsList.Add(newLevelElementDetails);
            _totalElementWidth += prefabWidth;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ClearCurrentLevelRpc()
    {
        if (IsServer)
        {
            // loop through all of the level element game objects and destroy
            foreach (GameObject go in _levelElementGOs)
            {
                Destroy(go);
                //go.GetComponent<NetworkObject>().Despawn(true);
            }
        }
        _levelElementGOs.Clear();

        _levelElementDetailsList.Clear();
        _playerSpawnPointList.Clear();
        _playerSpawnPointArrows.Clear();
        _totalElementWidth = 0;

        if (!IsServer) return;

        // destroy the level elements
        for (int i = 0; i < _levelElementHolder.childCount; i++)
        {
            Destroy(_levelElementHolder.GetChild(i).gameObject);
            //_levelElementHolder.GetChild(i).gameObject.GetComponent<NetworkObject>().Despawn(true);
        }

        // destroy the explosion masks
        for (int i = 0; i < _explosionMaskHolder.childCount; i++)
        {
            Destroy(_explosionMaskHolder.GetChild(i).gameObject);
            //_explosionMaskHolder.GetChild(i).gameObject.GetComponent<NetworkObject>().Despawn(true);
        }

        // destroy the broken windows
        for (int i = 0; i < _brokenWindowHolder.childCount; i++)
        {
            Destroy(_brokenWindowHolder.GetChild(i).gameObject);
            //_brokenWindowHolder.GetChild(i).gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }

    public Vector3 GetSpawnPointAtIndex(int index)
    {
        return _playerSpawnPointList[index];
    }

    public void GetFirstAndLastSpawnPoints(out Vector3 firstSpawnPoint, out Vector3 lastSpawnPoint, out int firstSpawnPointIndex, out int lastSpawnPointIndex)
    {
        firstSpawnPointIndex = (_playerSpawnPointList.Count / 2) - Mathf.CeilToInt(_distanceBetweenPlayers / 2f);
        lastSpawnPointIndex = (_playerSpawnPointList.Count / 2) + Mathf.FloorToInt(_distanceBetweenPlayers / 2f);

        firstSpawnPoint = GetSpawnPointAtIndex(firstSpawnPointIndex);
        lastSpawnPoint = GetSpawnPointAtIndex(lastSpawnPointIndex);
    }

    private void UpdateBuildingSpriteColours()
    {
        foreach (GameObject go in _levelElementGOs)
        {
            go.GetComponent<Building>().UpdateBuildingSpriteColourRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowHideSpawnPointArrowsBetweenIndexesRpc(int firstIndex, int currentIndex, int lastIndex, bool show)
    {
        for (int i = firstIndex; i <= lastIndex; i++)
        {
            if (i == currentIndex) continue;

            _playerSpawnPointArrows[i].SetActive(show);
        }
    }
}

public struct LevelElementDetails
{
    public GameObject ElementPrefab;
    public float ElementWidth;
    public float ElementHeight;
}
