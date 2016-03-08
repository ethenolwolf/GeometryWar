using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	[System.Serializable]
	public struct Coord {
		public int x;
		public int y;

		public Coord(int x, int y) {
			this.x = x;
			this.y = y;
		}

		public static bool operator == (Coord c1, Coord c2) {
			return c1.x == c2.x && c1.y == c2.y;
		}

		public static bool operator != (Coord c1, Coord c2) {
			return !(c1 == c2);
		}
	}

	public Coord mapSize;
	[Range(0f, 1f)]
	public float obstacleCoverage = 0.1f;
	public float obstacleMaxHeight = 1.6f;
	public float obstacleMinHeight = 0.5f;
	public Color frontObstacleColor;
	public Color backObstacleColor;
	public int seed;
	public float tileMargin = 0.1f;
	public Color tileBackColor;
	public Transform tileBack;
	public Transform tilePrefab;
	public Transform obstaclePrefab;
	public Vector2 maxMapSize;
	public Transform ground;
	public Transform groundCollider;

	bool[,] map;
	List<Coord> mapCoords;
	Queue<Coord> shuffledMapCoordQueue;

	Queue<Coord> shuffledWalkableCoordQueue;

	Coord mapCenter {
		get {
			return new Coord(mapSize.x / 2, mapSize.y / 2);
		}
	}

	void Start () {
		GenerateMap();
	}

	public void GenerateMap() {
		System.Random random = new System.Random(seed);

		ground.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 1f);
		groundCollider.localScale = new Vector3(maxMapSize.x, 0.1f, maxMapSize.y);

		map = new bool[mapSize.x, mapSize.y];
		tileBack.localScale = new Vector3(mapSize.x, mapSize.y, 1f);
		tileBack.GetComponent<Renderer>().sharedMaterial.color = tileBackColor;

		mapCoords = new List<Coord>();
		for (int i = 0; i < mapSize.x; i++) {
			for (int j = 0; j < mapSize.y; j++) {
				mapCoords.Add(new Coord(i, j));
			}
		}
		List<Coord> walkableCoordList = new List<Coord>(mapCoords);

		shuffledMapCoordQueue = ShuffleListToQueue(mapCoords);

		// clean original tiles
		string tileHolderName = "Map Holder";
		if (transform.FindChild(tileHolderName)) {
			DestroyImmediate(transform.FindChild(tileHolderName).gameObject);
		}

		Transform tileHolder = new GameObject(tileHolderName).transform;
		tileHolder.parent = transform;

		// generate tiles
		for (int i = 0; i < mapSize.x; i++) {
			for (int j = 0; j < mapSize.y; j++) {
				Transform tile = Instantiate(tilePrefab, new Vector3(i - mapSize.x / 2f + 0.5f, 0f, j - mapSize.y / 2f + 0.5f), Quaternion.Euler(90f, 0f, 0f)) as Transform;
				tile.localScale = Vector3.one * (1 - tileMargin);
				tile.parent = tileHolder;
			}
		}

		// generate obstacles
		int totalObstacles = (int) (mapSize.x * mapSize.y * obstacleCoverage);
		int placedObstacles = 0;
		for (int i = 0; i < totalObstacles; i++) {
			Coord obstacleCoord = GetNextFromQueue(shuffledMapCoordQueue);
			if (obstacleCoord == mapCenter) {
				continue;
			}

			map[obstacleCoord.x, obstacleCoord.y] = true;
			placedObstacles++;
			if (IsPathFullyAccessible(map, placedObstacles)) {
				float obstacleHeight = Mathf.Lerp(obstacleMinHeight, obstacleMaxHeight, (float)random.NextDouble());
				Transform obstacle = Instantiate(obstaclePrefab, CoordToPosition(obstacleCoord) + new Vector3(0, obstacleHeight / 2f, 0), Quaternion.identity) as Transform;
				obstacle.parent = tileHolder;
				Renderer renderer = obstacle.GetComponent<Renderer>();
				Material material = new Material(renderer.sharedMaterial);
				material.color = Color.Lerp(frontObstacleColor, backObstacleColor, obstacleCoord.y / (float)mapSize.y);
				renderer.sharedMaterial = material;
				obstacle.localScale = new Vector3(1f, obstacleHeight, 1f);
				walkableCoordList.Remove(obstacleCoord);
			} else {
				map[obstacleCoord.x, obstacleCoord.y] = false;
				placedObstacles--;
			}
		}

		shuffledWalkableCoordQueue = ShuffleListToQueue(walkableCoordList);
	}

	public Vector3 GetRandomWalkablePos() {
		return CoordToPosition(GetNextFromQueue(shuffledWalkableCoordQueue));
	}

	Queue<T> ShuffleListToQueue<T>(List<T> list) {
		T[] toShuffle = list.ToArray();
		System.Random random = new System.Random(seed);

		for (int i = 0; i < toShuffle.Length; i++) {
			int toSwap = random.Next(i, toShuffle.Length);
			T tmp = toShuffle[i];
			toShuffle[i] = toShuffle[toSwap];
			toShuffle[toSwap] = tmp;
		}

		return new Queue<T>(toShuffle);
	}

	T GetNextFromQueue<T>(Queue<T> queue) {
		T next = queue.Dequeue();
		queue.Enqueue(next);
		return next;
	}

	bool IsPathFullyAccessible(bool[,] map, int obstacleNumber) {
		int width = map.GetLength(0);
		int height = map.GetLength(1);
		Queue<Coord> processingQueue = new Queue<Coord>();
		bool[,] visited = new bool[width, height];

		processingQueue.Enqueue(mapCenter);
		int accessibleTiles = 1;

		while (processingQueue.Count > 0) {
			Coord currentCoord = processingQueue.Dequeue();
			visited[currentCoord.x, currentCoord.y] = true;

			for (int i = -1; i <= 1; i++) {
				for (int j = -1; j <= 1; j++) {
					if ((Mathf.Abs(i) + Mathf.Abs(j)) == 1) {
						int neighborX = currentCoord.x + i;
						int neighborY = currentCoord.y + j;
						if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height) {
							if (map[neighborX, neighborY] != true && visited[neighborX, neighborY] != true) {
								processingQueue.Enqueue(new Coord(neighborX, neighborY));
								visited[neighborX, neighborY] = true;
								accessibleTiles++;
							}
						}
					}
				}
			}
		}
		return accessibleTiles == (width * height - obstacleNumber);
	}

	public Vector3 CoordToPosition(Coord coord) {
		return new Vector3(coord.x - mapSize.x / 2f + 0.5f, 0f, coord.y - mapSize.y / 2f + 0.5f);
	}

}
