using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Map
{
    public class MapPlayerTracker : MonoBehaviour
    {
        public bool lockAfterSelecting = false;
        public float enterNodeDelay = 1f;
        public MapManager mapManager;
        public MapView view;

        public static MapPlayerTracker Instance;

        private bool _locked;

        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;
                PlayerPrefs.SetInt("Locked", _locked ? 1 : 0); // bool 값을 int로 변환하여 저장
                PlayerPrefs.Save(); // PlayerPrefs를 즉시 저장
            }
        }

        private void Awake()
        {
            Instance = this;
            // 초기화 시 PlayerPrefs에서 값을 불러옴
            if (PlayerPrefs.HasKey("Locked"))
            {
                _locked = PlayerPrefs.GetInt("Locked") == 1;
            }
            else
            {
                _locked = false; // 기본값 설정
            }
        }
      
        public void SelectNode(MapNode mapNode)
        {
            if (Locked) return;

            // Debug.Log("Selected node: " + mapNode.Node.point);

            if (mapManager.CurrentMap.path.Count == 0)
            {
                // player has not selected the node yet, he can select any of the nodes with y = 0
                if (mapNode.Node.point.y == 0)
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
            else
            {
                var currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                var currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                if (currentNode != null && currentNode.outgoing.Any(point => point.Equals(mapNode.Node.point)))
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
        }

        private void SendPlayerToNode(MapNode mapNode)
        {
            Locked = lockAfterSelecting;
            mapManager.CurrentMap.path.Add(mapNode.Node.point);
            mapManager.SaveMap();
            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();

            DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(mapNode));
        }

   
        private static void EnterNode(MapNode mapNode)
        {
            //입장시 지도를 닫고
            MapView.Inst.HideMap();

            Instance.Locked = true;


            //필요한 작업을 수행
            switch (mapNode.Node.nodeType)
            {
                case NodeType.MinorEnemy:
                    BattleManager.Inst.StartBattle();
                    break;

                case NodeType.EliteEnemy:
                    break;

                case NodeType.RestSite:
                    BattleScene.Inst.StartContent(E_ContentType.Rest);
                    break;
            
                case NodeType.Store:
                    BattleScene.Inst.StartContent(E_ContentType.Store);
                    break;

                case NodeType.Boss:
                    MapManager.Inst.GenerateNewMap();
                    break;

                case NodeType.Mystery:
                    BattleScene.Inst.StartContent(E_ContentType.Mystery);
                    break;
            }
        }

        private void PlayWarningThatNodeCannotBeAccessed()
        {
            Debug.Log("Selected node cannot be accessed");
        }
    }
}