using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerManager
{
    public class Manager
    {
        private static Manager _instance;
        public Dictionary<string, GameObject> ConnectedPlayers { get; set; }

        public int NumberConnectedPlayers { get; private set; }

        public string PlayerID { get; private set; }

        private Manager()
        {
            if (_instance != null)
            {
                return;
            }

            ConnectedPlayers = new Dictionary<string, GameObject>();
            NumberConnectedPlayers = 0;

            _instance = this;
        }

        public static Manager Instance
        {
            get
            {
                if (_instance == null)
                {
                    new Manager();
                }

                return _instance;
            }
        }

        public void AddPlayerToConnectedPlayers(string _playerID, GameObject _playerObject)
        {
            if (!ConnectedPlayers.ContainsKey(_playerID))
            {
                ConnectedPlayers.Add(_playerID, _playerObject);
                NumberConnectedPlayers++;
            }
        }

        public void RemovePlayerFromConnectedPlayers(string _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                ConnectedPlayers.Remove(_playerID);
                NumberConnectedPlayers--;
            }
        }

        public GameObject[] GetConnectedPlayers()
        {
            return ConnectedPlayers.Values.ToArray();
        }

        public void SetLocalPlayerID(string _playerID)
        {
            PlayerID = _playerID;
        }

        public GameObject GetPlayerFromConnectedPlayers(string _playerID)
        {
            if (ConnectedPlayers.ContainsKey(_playerID))
            {
                return ConnectedPlayers[_playerID];
            }

            return null;
        }
    }
}
