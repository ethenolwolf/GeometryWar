using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public Transform enemyLegionPrefab;

	public float spawnRadius = 28f;
	public float spawnInterval = 3f;

	PlayerLegionControl playerLegionControl;

	void Start () {
		GameObject playerLegionControlObj = GameObject.FindGameObjectWithTag("Player");
		playerLegionControl = playerLegionControlObj.GetComponent<PlayerLegionControl>();
		InvokeRepeating("SpawnEnemyLegion", 2f, spawnInterval);
	}

	void SpawnEnemyLegion() {
		Vector2 spawnPos = Random.insideUnitCircle.normalized * spawnRadius;
		Transform enemyLegionControlObj = Instantiate(enemyLegionPrefab, new Vector3(spawnPos.x, 0f, spawnPos.y), Quaternion.identity) as Transform;
		EnemyLegionControl enemyLegionControl = enemyLegionControlObj.GetComponent<EnemyLegionControl>();
		enemyLegionControl.OnDefeated += playerLegionControl.OnEnemyLegionDefeated;
	}
}
