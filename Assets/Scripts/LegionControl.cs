using UnityEngine;
using System.Collections;

public class LegionControl : MonoBehaviour {

	public Transform legion;
	public Transform soldierPrefab;
	public Color legionColor;
	public int soldierCount = 4;

	protected Vector3 dest = new Vector3();

	public event System.Action OnDefeated;

	public void InitializeSoldiers(string side, Vector3 pos) {
		string rivalSide = side.StartsWith("Player") ? "Enemy" : "Player";

		for (int i = 0; i < soldierCount; i++) {
			Transform soldierTransform = Instantiate(soldierPrefab, pos, Quaternion.identity) as Transform;
			soldierTransform.parent = legion;

			SoldierEntity soldierEntity = soldierTransform.GetComponent<SoldierEntity>();
			soldierEntity.SetLayer(side);
			soldierEntity.SetRivalLayer(rivalSide);
			soldierEntity.SetColor(legionColor);
		}
	}

	protected virtual void SetDestination(Vector3 dest) {
		this.dest = dest;

		SoldierEntity[] soldierEntities = legion.GetComponentsInChildren<SoldierEntity>();
		foreach (var soldierEntity in soldierEntities) {
			soldierEntity.SetLegionDestination(dest);
		}
	}


	protected virtual void CheckLegionDefeated() {
		if (legion.childCount == 0) {
			OnLegionDefeated();
			Destroy(gameObject);
		}
	}

	protected virtual void OnLegionDefeated() {
		if (OnDefeated != null) {
			OnDefeated();
		}
	}
}
