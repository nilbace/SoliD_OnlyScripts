using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 주어진 MapConfig에 따라 맵을 생성하는 클래스
    /// Layer = 가로 한 층(여러 노드 배치 가능)
    /// Layer들이 모이고 사이에 여러 노드들에 길이 생기며 미니맵이 생성되는 형식
    /// </summary>
    public static class MapGenerator
    {
        private static MapConfig config;

        // 무작위로 생성 가능한 노드 타입들
        private static readonly List<NodeType> RandomNodes = new List<NodeType>
        {NodeType.Mystery, NodeType.Store, NodeType.Treasure, NodeType.MinorEnemy, NodeType.RestSite};

        private static List<float> layerDistances;
        private static List<List<Point>> paths;
        //전체 노드
        private static readonly List<List<Node>> nodes = new List<List<Node>>();

        /// <summary>
        /// 주어진 설정에 따라 지도를 생성합니다.
        /// </summary>
        /// <param name="conf">지도 생성 설정</param>
        /// <returns>생성된 지도 객체</returns>
        public static Map GetMap(MapConfig conf)
        {
            if (conf == null)
            {
                Debug.LogWarning("Config was null in MapGenerator.Generate()");
                return null;
            }

            config = conf;
            nodes.Clear();

            GenerateLayerDistances();

            for (var i = 0; i < conf.layers.Count; i++)
                PlaceLayer(i);

            GeneratePaths();

            RandomizeNodePositions();

            SetUpConnections();

            RemoveCrossConnections();

            // 연결이 있는 모든 노드 선택
            var nodesList = nodes.SelectMany(n => n).Where(n => n.incoming.Count > 0 || n.outgoing.Count > 0).ToList();

            // 보스 레벨의 랜덤 이름 선택
            var bossNodeName = config.nodeBlueprints.Where(b => b.nodeType == NodeType.Boss).ToList().Random().name;
            return new Map(conf.name, bossNodeName, nodesList, new List<Point>());
        }

        /// <summary>
        /// 각 레이어 간의 거리 계산
        /// </summary>
        private static void GenerateLayerDistances()
        {
            layerDistances = new List<float>();
            foreach (var layer in config.layers)
                layerDistances.Add(layer.distanceFromPreviousLayer.GetValue());
        }

        /// <summary>
        /// 특정 레이어까지의 거리 반환
        /// </summary>
        /// <param name="layerIndex">레이어 인덱스</param>
        /// <returns>레이어까지의 거리</returns>
        private static float GetDistanceToLayer(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex > layerDistances.Count) return 0f;

            return layerDistances.Take(layerIndex + 1).Sum();
        }

        /// <summary>
        /// 지정된 레이어에 노드를 배치합니다.
        /// </summary>
        /// <param name="layerIndex">레이어 인덱스</param>
        private static void PlaceLayer(int layerIndex)
        {
            var layer = config.layers[layerIndex];
            var nodesOnThisLayer = new List<Node>();

            // offset of this layer to make all the nodes centered:
            var offset = layer.nodesApartDistance * config.GridWidth / 2f;

            for (var i = 0; i < config.GridWidth; i++)
            {
                var nodeType = Random.Range(0f, 1f) < layer.randomizeNodes ? GetRandomNode() : layer.nodeType;
                var blueprintName = config.nodeBlueprints.Where(b => b.nodeType == nodeType).ToList().Random().name;
                var node = new Node(nodeType, blueprintName, new Point(i, layerIndex))
                {
                    position = new Vector2(-offset + i * layer.nodesApartDistance, GetDistanceToLayer(layerIndex))
                };
                nodesOnThisLayer.Add(node);
            }

            nodes.Add(nodesOnThisLayer);
        }

        /// <summary>
        /// 노드의 위치를 무작위로 변경하여 변동성 추가
        /// </summary>
        private static void RandomizeNodePositions()
        {
            for (var index = 0; index < nodes.Count; index++)
            {
                var list = nodes[index];
                var layer = config.layers[index];
                var distToNextLayer = index + 1 >= layerDistances.Count
                    ? 0f
                    : layerDistances[index + 1];
                var distToPreviousLayer = layerDistances[index];

                foreach (var node in list)
                {
                    var xRnd = Random.Range(-1f, 1f);
                    var yRnd = Random.Range(-1f, 1f);

                    var x = xRnd * layer.nodesApartDistance / 2f;
                    var y = yRnd < 0 ? distToPreviousLayer * yRnd / 2f : distToNextLayer * yRnd / 2f;

                    node.position += new Vector2(x, y) * layer.randomizePosition;
                }
            }
        }

        /// <summary>
        /// 노드 간의 연결 설정
        /// </summary>
        private static void SetUpConnections()
        {
            foreach (var path in paths)
            {
                for (var i = 0; i < path.Count - 1; ++i)
                {
                    var node = GetNode(path[i]);
                    var nextNode = GetNode(path[i + 1]);
                    node.AddOutgoing(nextNode.point);
                    nextNode.AddIncoming(node.point);
                }
            }
        }

        /// <summary>
        /// 교차 연결을 제거하여 지도를 정리
        /// </summary>
        private static void RemoveCrossConnections()
        {
            for (var i = 0; i < config.GridWidth - 1; ++i)
                for (var j = 0; j < config.layers.Count - 1; ++j)
                {
                    var node = GetNode(new Point(i, j));
                    if (node == null || node.HasNoConnections()) continue;
                    var right = GetNode(new Point(i + 1, j));
                    if (right == null || right.HasNoConnections()) continue;
                    var top = GetNode(new Point(i, j + 1));
                    if (top == null || top.HasNoConnections()) continue;
                    var topRight = GetNode(new Point(i + 1, j + 1));
                    if (topRight == null || topRight.HasNoConnections()) continue;

                    // 교차 연결 노드를 검사합니다.
                    if (!node.outgoing.Any(element => element.Equals(topRight.point))) continue;
                    if (!right.outgoing.Any(element => element.Equals(top.point))) continue;

                    // 직접 연결을 추가
                    node.AddOutgoing(top.point);
                    top.AddIncoming(node.point);

                    right.AddOutgoing(topRight.point);
                    topRight.AddIncoming(right.point);

                    // 무작위로 교차 연결 제거
                    var rnd = Random.Range(0f, 1f);
                    if (rnd < 0.2f)
                    {
                        node.RemoveOutgoing(topRight.point);
                        topRight.RemoveIncoming(node.point);
                        right.RemoveOutgoing(top.point);
                        top.RemoveIncoming(right.point);
                    }
                    else if (rnd < 0.6f)
                    {
                        node.RemoveOutgoing(topRight.point);
                        topRight.RemoveIncoming(node.point);
                    }
                    else
                    {
                        right.RemoveOutgoing(top.point);
                        top.RemoveIncoming(right.point);
                    }
                }
        }

        /// <summary>
        /// 특정 위치에 있는 노드를 반환
        /// </summary>
        /// <param name="p">노드의 위치</param>
        /// <returns>해당 위치의 노드</returns>
        private static Node GetNode(Point p)
        {
            if (p.y >= nodes.Count) return null;
            if (p.x >= nodes[p.y].Count) return null;

            return nodes[p.y][p.x];
        }

        /// <summary>
        /// 마지막 보스 노드를 반환
        /// </summary>
        /// <returns>보스 노드의 위치</returns>
        private static Point GetFinalNode()
        {
            var y = config.layers.Count - 1;
            if (config.GridWidth % 2 == 1)
                return new Point(config.GridWidth / 2, y);

            return Random.Range(0, 2) == 0
                ? new Point(config.GridWidth / 2, y)
                : new Point(config.GridWidth / 2 - 1, y);
        }

        /// <summary>
        /// 지도 경로를 생성
        /// </summary>
        private static void GeneratePaths()
        {
            var finalNode = GetFinalNode();
            paths = new List<List<Point>>();
            var numOfStartingNodes = config.numOfStartingNodes.GetValue();
            var numOfPreBossNodes = config.numOfPreBossNodes.GetValue();

            var candidateXs = new List<int>();
            for (var i = 0; i < config.GridWidth; i++)
                candidateXs.Add(i);

            candidateXs.Shuffle();
            var startingXs = candidateXs.Take(numOfStartingNodes);
            var startingPoints = (from x in startingXs select new Point(x, 0)).ToList();

            candidateXs.Shuffle();
            var preBossXs = candidateXs.Take(numOfPreBossNodes);
            var preBossPoints = (from x in preBossXs select new Point(x, finalNode.y - 1)).ToList();

            int numOfPaths = Mathf.Max(numOfStartingNodes, numOfPreBossNodes) + Mathf.Max(0, config.extraPaths);
            for (int i = 0; i < numOfPaths; ++i)
            {
                Point startNode = startingPoints[i % numOfStartingNodes];
                Point endNode = preBossPoints[i % numOfPreBossNodes];
                var path = Path(startNode, endNode);
                path.Add(finalNode);
                paths.Add(path);
            }
        }

        /// <summary>
        /// 시작점과 끝점 사이의 무작위 경로를 생성합니다.
        /// </summary>
        /// <param name="fromPoint">시작점</param>
        /// <param name="toPoint">끝점</param>
        /// <returns>생성된 경로의 포인트 리스트</returns>
        private static List<Point> Path(Point fromPoint, Point toPoint)
        {
            int toRow = toPoint.y;
            int toCol = toPoint.x;

            int lastNodeCol = fromPoint.x;

            var path = new List<Point> { fromPoint };
            var candidateCols = new List<int>();
            for (int row = 1; row < toRow; ++row)
            {
                candidateCols.Clear();

                int verticalDistance = toRow - row;
                int horizontalDistance;

                int forwardCol = lastNodeCol;
                horizontalDistance = Mathf.Abs(toCol - forwardCol);
                if (horizontalDistance <= verticalDistance)
                    candidateCols.Add(lastNodeCol);

                int leftCol = lastNodeCol - 1;
                horizontalDistance = Mathf.Abs(toCol - leftCol);
                if (leftCol >= 0 && horizontalDistance <= verticalDistance)
                    candidateCols.Add(leftCol);

                int rightCol = lastNodeCol + 1;
                horizontalDistance = Mathf.Abs(toCol - rightCol);
                if (rightCol < config.GridWidth && horizontalDistance <= verticalDistance)
                    candidateCols.Add(rightCol);

                int RandomCandidateIndex = Random.Range(0, candidateCols.Count);
                int candidateCol = candidateCols[RandomCandidateIndex];
                var nextPoint = new Point(candidateCol, row);

                path.Add(nextPoint);

                lastNodeCol = candidateCol;
            }

            path.Add(toPoint);

            return path;
        }

        private static NodeType GetRandomNode()
        {
            return RandomNodes[Random.Range(0, RandomNodes.Count)];
        }
    }
}