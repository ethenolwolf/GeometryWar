using UnityEngine;
using System.Collections;

public class EnemyLegionControl : LegionControl {

	public float destRadius = 20;
	public float newDestTime = 10f;
	public Color legionColorVariance;

	MapGenerator mapGenerator;

	void Start () {
		legionColor = Color.Lerp(legionColor, legionColorVariance, Random.Range(0f, 1f));
		mapGenerator = FindObjectOfType<MapGenerator>();

		InitializeSoldiers("Enemy", transform.position);
		InvokeRepeating("SetRandomDestination", 1f, newDestTime);

		GameObject playerLegionControlObj = GameObject.FindGameObjectWithTag("Player");
		PlayerLegionControl playerLegionControl = playerLegionControlObj.GetComponent<PlayerLegionControl>();

		for(int i = 0; i < legion.childCount; i++) {
			Transform soldierTransform = legion.GetChild(i);
			SoldierEntity soldierEntity = soldierTransform.GetComponent<SoldierEntity>();
			soldierEntity.OnSoldierDead += playerLegionControl.OnEnemySoldierDead;
		}
	}

	void Update() {
		CheckLegionDefeated();
	}

	void SetRandomDestination() {
		SetDestination(mapGenerator.GetRandomWalkablePos());
	}

}
