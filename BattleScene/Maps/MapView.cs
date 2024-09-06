using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    public class MapView : MonoBehaviour
    {
        public enum MapOrientation
        {
            BottomToTop,
            TopToBottom,
            RightToLeft,
            LeftToRight
        }

        public MapManager mapManager;
        public MapOrientation orientation;
        public float mapSize;

        [Tooltip("Assets 폴더의 모든 MapConfig 스크립터블 오브젝트 목록, 반드시 전부 포함해야 함")]
        public List<MapConfig> allMapConfigs;
        public GameObject nodePrefab;
        [Tooltip("맵의 시작/끝 노드가 화면 가장자리로부터의 오프셋")]
        public float orientationOffset;
        [Header("Background Settings")]
        public GameObject PannelPrefab;
        [Tooltip("배경 스프라이트가 null이면 배경이 표시되지 않음")]
        public Sprite background;
        public Color32 backgroundColor = Color.white;
        public float xSize;
        [Tooltip("최하단 노드와 최상단 노드에서 추가 상하 간격")]
        public float MapVerticalMargin;
        [Tooltip("미니맵 배경 Y축 Offset값")]
        public float BackgroundYOffset;
        [Tooltip("미니맵 전체 Y축 Offset값")]
        public float MapYOffset;

        [Header("Line Settings")]
        public GameObject linePrefab;
        [Range(3, 10)]
        public int linePointsCount = 10;
        [Tooltip("노드에서 라인의 시작 지점까지의 거리")]
        public float offsetFromNodes = 0.5f;
        [Header("Colors")]
        [Tooltip("방문된 노드 또는 도달 가능한 노드 색상")]
        public Color32 visitedColor = Color.white;
        [Tooltip("잠긴 노드 색상")]
        public Color32 lockedColor = Color.gray;
        [Tooltip("방문한 경로 또는 사용 가능한 경로 색상")]
        public Color32 lineVisitedColor = Color.white;
        [Tooltip("사용할 수 없는 경로 색상")]
        public Color32 lineLockedColor = Color.gray;

        protected GameObject firstParent;
        protected GameObject mapParent;
        private List<List<Point>> paths;
        private Camera cam;
        //모든 노드들
        public readonly List<MapNode> MapNodes = new List<MapNode>();
        protected readonly List<LineConnection> lineConnections = new List<LineConnection>();

        public static MapView Inst;
        private bool isHide;
        public float NodeIconSize;

        public Map Map { get; protected set; }

        private void Awake()
        {
            Inst = this;        //인스턴스 초기화
            cam = Camera.main;  //메인 카메라 참조
        }

        /// <summary>
        /// 맵을 초기화하고 부모 오브젝트를 삭제
        /// </summary>
        protected virtual void ClearMap()
        {
            if (firstParent != null)
                Destroy(firstParent);

            MapNodes.Clear();
            lineConnections.Clear();
        }

        public virtual void ShowMap(Map m)
        {
            if (m == null)
            {
                Debug.LogWarning("Map was null in MapView.ShowMap()");
                return;
            }

            Map = m;

            ClearMap();

            CreateMapParent();

            CreateNodes(m.nodes);

            DrawLines();

            SetOrientation();

            ResetNodesRotation();

            SetAttainableNodes();

            SetLineColors();

            CreateMapBackground(m);

            SetLayerForParentAndChildren(firstParent);

            MoveToCenter();

            SetSortingLayer();

            HideMap();
        }

        /// <summary>
        /// 맵 배경을 생성합니다.
        /// </summary>
        protected virtual void CreateMapBackground(Map m)
        {
            if (background == null) return;

            var backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(mapParent.transform);
            var bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
            var span = m.DistanceBetweenFirstAndLastLayers();
            backgroundObject.transform.localPosition = new Vector3(bossNode.transform.localPosition.x, span / 2f + BackgroundYOffset, 0f);
            backgroundObject.transform.localRotation = Quaternion.identity;
            var sr = backgroundObject.AddComponent<SpriteRenderer>();
            sr.color = backgroundColor;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sprite = background;
            sr.size = new Vector2(xSize, span + MapVerticalMargin * 2f);
        }

        /// <summary>
        /// 맵의 부모 오브젝트를 생성합니다.
        /// </summary>
        protected virtual void CreateMapParent()
        {
            firstParent = new GameObject("OuterMapParent");
            mapParent = new GameObject("MapParentWithAScroll");
            mapParent.transform.SetParent(firstParent.transform);
            var scrollNonUi = mapParent.AddComponent<ScrollNonUI>();
            scrollNonUi.freezeX = orientation == MapOrientation.BottomToTop || orientation == MapOrientation.TopToBottom;
            scrollNonUi.freezeY = orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft;
            var boxCollider = mapParent.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(100, 100, 1);

            Instantiate(PannelPrefab, firstParent.transform);
        }

        /// <summary>
        /// 노드들을 생성합니다.
        /// </summary>
        protected void CreateNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var mapNode = CreateMapNode(node);
                MapNodes.Add(mapNode);
            }
        }

        /// <summary>
        /// 단일 노드를 생성합니다.
        /// </summary>
        protected virtual MapNode CreateMapNode(Node node)
        {
            var mapNodeObject = Instantiate(nodePrefab, mapParent.transform);
            mapNodeObject.transform.localScale = mapNodeObject.transform.localScale * NodeIconSize;
            var mapNode = mapNodeObject.GetComponent<MapNode>();
            var blueprint = GetBlueprint(node.blueprintName);
            mapNode.SetUp(node, blueprint);
            mapNode.transform.localPosition = node.position;
            return mapNode;
        }

        /// <summary>
        /// 어떤 노드가 도달 가능한지 설정
        /// </summary>
        public void SetAttainableNodes()
        {
            // 모든 노드를 처음에는 잠김 상태로 설정
            foreach (var node in MapNodes)
                node.SetState(NodeStates.Locked);

            if (mapManager.CurrentMap.path.Count == 0)
            {
                // 맵에서 아직 이동을 시작하지 않았을 경우, 첫 번째 레이어의 모든 노드를 도달 가능 상태로 설정
                foreach (var node in MapNodes.Where(n => n.Node.point.y == 0))
                    node.SetState(NodeStates.Attainable);
            }
            else
            {
                // 맵에서 이미 이동을 시작한 경우, 경로를 먼저 방문된 상태로 설정
                foreach (var point in mapManager.CurrentMap.path)
                {
                    var mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Visited);
                }

                var currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                var currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                // 이동할 수 있는 모든 노드를 도달 가능 상태로 설정
                foreach (var point in currentNode.outgoing)
                {
                    var mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Attainable);
                }
            }
        }

        public virtual void SetLineColors()
        {
            // 모든 라인을 먼저 잠긴 색상으로 설정
            foreach (var connection in lineConnections)
                connection.SetColor(lineLockedColor);

            // 탐색이 시작되지 않았다면 라인 색상을 그대로 유지
            if (mapManager.CurrentMap.path.Count == 0)
                return;

            // 마지막 노드의 outgoing 연결을 도달 가능 색상으로 설정
            var currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
            var currentNode = mapManager.CurrentMap.GetNode(currentPoint);

            foreach (var point in currentNode.outgoing)
            {
                var lineConnection = lineConnections.FirstOrDefault(conn => conn.from.Node == currentNode &&
                                                                            conn.to.Node.point.Equals(point));
                lineConnection?.SetColor(lineVisitedColor);
            }

            if (mapManager.CurrentMap.path.Count <= 1) return;

            for (var i = 0; i < mapManager.CurrentMap.path.Count - 1; i++)
            {
                var current = mapManager.CurrentMap.path[i];
                var next = mapManager.CurrentMap.path[i + 1];
                var lineConnection = lineConnections.FirstOrDefault(conn => conn.@from.Node.point.Equals(current) &&
                                                                            conn.to.Node.point.Equals(next));
                lineConnection?.SetColor(lineVisitedColor);
            }
        }

        /// <summary>
        /// 맵의 방향(세로, 가로)에 맞춰 미니맵을 생성
        /// </summary>
        protected virtual void SetOrientation()
        {
            var scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
            var span = mapManager.CurrentMap.DistanceBetweenFirstAndLastLayers();
            var bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);

            firstParent.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
            var offset = orientationOffset;
            switch (orientation)
            {
                case MapOrientation.BottomToTop:
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.yConstraints.max = 0;
                        scrollNonUi.yConstraints.min = -(span + 2f * offset);
                    }
                    firstParent.transform.localPosition += new Vector3(0, offset, 0);
                    break;
                case MapOrientation.TopToBottom:
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 180);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.yConstraints.min = 0;
                        scrollNonUi.yConstraints.max = span + 2f * offset;
                    }
                    firstParent.transform.localPosition += new Vector3(0, -offset, 0);
                    break;
                case MapOrientation.RightToLeft:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 90);
                    firstParent.transform.localPosition -= new Vector3(offset, bossNode.transform.position.y, 0);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.xConstraints.max = span + 2f * offset;
                        scrollNonUi.xConstraints.min = 0;
                    }
                    break;
                case MapOrientation.LeftToRight:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, -90);
                    firstParent.transform.localPosition += new Vector3(offset, -bossNode.transform.position.y, 0);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.xConstraints.max = 0;
                        scrollNonUi.xConstraints.min = -(span + 2f * offset);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 노드들 사이의 라인을 그립니다.
        /// </summary>
        private void DrawLines()
        {
            foreach (var node in MapNodes)
            {
                foreach (var connection in node.Node.outgoing)
                    AddLineConnection(node, GetNode(connection));
            }
        }

        /// <summary>
        /// 모든 노드의 회전을 초기화합니다.
        /// </summary>
        private void ResetNodesRotation()
        {
            foreach (var node in MapNodes)
                node.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 두 노드 간에 라인 연결을 추가합니다.
        /// </summary>
        /// <param name="from">시작 노드</param>
        /// <param name="to">끝 노드</param>
        protected virtual void AddLineConnection(MapNode from, MapNode to)
        {
            if (linePrefab == null) return;

            var lineObject = Instantiate(linePrefab, mapParent.transform);
            var lineRenderer = lineObject.GetComponent<LineRenderer>();
            var fromPoint = from.transform.position +
                            (to.transform.position - from.transform.position).normalized * offsetFromNodes;

            var toPoint = to.transform.position +
                          (from.transform.position - to.transform.position).normalized * offsetFromNodes;

            lineObject.transform.position = fromPoint;
            lineRenderer.useWorldSpace = false;

            lineRenderer.positionCount = linePointsCount;
            for (var i = 0; i < linePointsCount; i++)
            {
                lineRenderer.SetPosition(i,
                    Vector3.Lerp(Vector3.zero, toPoint - fromPoint, (float)i / (linePointsCount - 1)));
            }

            var dottedLine = lineObject.GetComponent<DottedLineRenderer>();
            if (dottedLine != null) dottedLine.ScaleMaterial();

            lineConnections.Add(new LineConnection(lineRenderer, null, from, to));
        }

        protected MapNode GetNode(Point p)
        {
            return MapNodes.FirstOrDefault(n => n.Node.point.Equals(p));
        }

        protected MapConfig GetConfig(string configName)
        {
            return allMapConfigs.FirstOrDefault(c => c.name == configName);
        }

        protected NodeBlueprint GetBlueprint(NodeType type)
        {
            var config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.nodeType == type);
        }

        protected NodeBlueprint GetBlueprint(string blueprintName)
        {
            var config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.name == blueprintName);
        }

        /// <summary>
        /// 맵을 중앙으로 이동시킵니다. 보스 노드의 X 위치를 기준으로 중앙으로 이동하고, Y축으로 추가 이동합니다.
        /// </summary>
        public void MoveToCenter()
        {
            var bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
            float bossNodePozX = bossNode.gameObject.transform.position.x;
            mapParent.transform.position += new Vector3(-bossNodePozX, 0, 0);
            firstParent.transform.position += Vector3.up * MapVerticalMargin * MapYOffset;
        }


        /// <summary>
        /// 정렬 레이어를 설정합니다. 렌더러와 캔버스의 정렬 레이어를 "Map"으로 설정합니다.
        /// </summary>
        public void SetSortingLayer()
        {
            var parenttr = mapParent.transform;
            Renderer[] SRs = parenttr.GetComponentsInChildren<Renderer>();
            foreach (Renderer SR in SRs)
            {
                SR.sortingLayerName = "Map";
            }
            Canvas[] canvases = parenttr.GetComponentsInChildren<Canvas>();
            foreach (Canvas SR in canvases)
            {
                SR.sortingLayerName = "Map";
            }
            firstParent.transform.localScale = Vector3.one * mapSize;

        }


        /// <summary>
        /// 맵을 표시합니다. 첫 번째 부모를 활성화하고 모든 자식에 대해 페이드 인 효과를 적용합니다.
        /// </summary>
        public void ShowMap()
        {
            firstParent.SetActive(true);
            var parenttr = mapParent.transform;

            // 모든 자식을 순회하는 재귀 함수
            void FadeInChildren(Transform parent)
            {
                foreach (Transform child in parent)
                {
                    SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.color.a != 0)
                    {
                        sr.DOFade(0.2f, 0f);
                        sr.DOFade(1f, 0.5f).SetEase(Ease.Linear);
                    }
                    // 자식의 자식들도 처리
                    FadeInChildren(child);
                }
            }

            // 첫 번째 부모의 모든 자식에 대해 재귀 함수 호출
            FadeInChildren(parenttr);

            isHide = false;
        }

        /// <summary>
        /// 맵을 숨깁니다. 첫 번째 부모를 비활성화합니다.
        /// </summary>
        public void HideMap()
        {
            firstParent.gameObject.SetActive(false);
            isHide = true;
        }

        /// <summary>
        /// 현재 숨김 상태에 따라 맵을 표시하거나 숨깁니다.
        /// </summary>
        public void MapBTN()
        {
            if (isHide) ShowMap();
            else HideMap();
        }

        void SetLayerRecursively(GameObject parent, int layer)
        {
            // 부모 오브젝트의 레이어를 설정
            parent.layer = layer;

            // 모든 자식 오브젝트들을 순회하며 레이어를 설정
            foreach (Transform child in parent.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// 레이어를 설정합니다.
        /// </summary>
        /// <param name="firstParent">첫 번째 부모 오브젝트</param>
        void SetLayerForParentAndChildren(GameObject firstParent)
        {
            int layer = LayerMask.NameToLayer("Mystery&ETC");

            if (layer != -1)
            {
                // 부모 오브젝트와 모든 자식 오브젝트의 레이어를 설정
                SetLayerRecursively(firstParent, layer);
            }
            else
            {
                Debug.LogError("Layer 'Mystery&ETC' not found.");
            }
        }
    }
}
