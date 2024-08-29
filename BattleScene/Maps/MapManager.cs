using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Map
{
    public class MapManager : MonoBehaviour
    {
        public MapConfig[] configs;
        public MapView view;
        public static MapManager Inst;

        public Map CurrentMap { get; private set; }

        private void Awake()
        {
            Inst = this;
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey("Map"))
            {
                var mapJson = PlayerPrefs.GetString("Map");
                var map = JsonConvert.DeserializeObject<Map>(mapJson);
                // using this instead of .Contains()
                if (map.path.Any(p => p.Equals(map.GetBossNode().point)))
                {
                    // payer has already reached the boss, generate a new map
                    GenerateNewMap();
                }
                else
                {
                    CurrentMap = map;
                    // player has not reached the boss yet, load the current map
                    view.ShowMap(map);
                }
            }
            else
            {
                GenerateNewMap();
            }
            //PlayerPrefs.DeleteAll();
            GenerateNewMap();
        }

        public void GenerateNewMap()
        {
            var map = MapGenerator.GetMap(configs[ActManager.Inst.nowAct-1]);
            CurrentMap = map;
            MapView.Inst.ShowMap(map);
        }

        public void SaveMap()
        {
            if (CurrentMap == null) return;

            var json = JsonConvert.SerializeObject(CurrentMap, Formatting.Indented,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            PlayerPrefs.SetString("Map", json);
            PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            SaveMap();
        }
    }
}
