using UnityEngine;
using System.Collections;

public class PlayerLegionControl : LegionControl {

	public float cameraSpeed = 1f;
	public float cameraRigBoundary = 10f;
	public Transform cameraRig;
    public Transform destMarker;

	Material destMarkerMaterial;
	Color destMarkerColor;
	float destMarkerFadeOutTime = 0.5f;
	float destMarkerFadeOutTimeCount = 0;

    Plane plane;

	int enemySoldiersKilled = 0;
	int newSoldierPerEnemyKilled = 3;

	bool legionDefeated = false;

    void Start() {
		InitializeSoldiers("Player", new Vector3(0.5f, 0f, 0.5f));
        plane = new Plane(Vector3.up, Vector3.zero);

		destMarkerMaterial = destMarker.GetComponent<Renderer>().material;
		destMarkerColor = destMarkerMaterial.color;
    }

    void Update () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float dst;

		if (Input.GetButton("Fire1")) {
			if (plane.Raycast(ray, out dst)) {
				dest = ray.GetPoint(dst);
			}
			destMarkerMaterial.color = destMarkerColor;
			destMarker.transform.position = dest;
			destMarkerFadeOutTimeCount = 0;
			SetDestination(dest);
		}

		if (destMarkerFadeOutTimeCount <= destMarkerFadeOutTime) {
			destMarkerMaterial.color = Color.Lerp(destMarkerMaterial.color, Color.clear, destMarkerFadeOutTimeCount / destMarkerFadeOutTime);
			destMarkerFadeOutTimeCount += Time.deltaTime;
		}


		CheckLegionDefeated();

		// move cameraRig
		if (!legionDefeated) {
			Vector3 centerPos = GetCenterPos();
			Vector2 cameraDir = new Vector2(centerPos.x - cameraRig.position.x, centerPos.z - cameraRig.position.z).normalized;
			Vector3 cameraRigPos = cameraRig.position + new Vector3(cameraDir.x, 0f, cameraDir.y) * cameraSpeed * Time.deltaTime;
			cameraRig.position = new Vector3(Mathf.Clamp(cameraRigPos.x, -cameraRigBoundary, cameraRigBoundary), 0f, Mathf.Clamp(cameraRigPos.z, -cameraRigBoundary, cameraRigBoundary));
		}

    }

	protected override void CheckLegionDefeated () {
		if (!legionDefeated && legion.childCount == 0) {
			legionDefeated = true;

			OnLegionDefeated();
		}
	}

	Vector3 GetCenterPos() {
		Vector3 centerPos = new Vector3();

		for (int i = 0; i < legion.childCount; i++) {
			centerPos += legion.GetChild(i).position;
		}

		return centerPos / (float)legion.childCount;
	}

	public void OnEnemySoldierDead(SoldierEntity soldierEntity) {
		Vector3 spawnPos = GetCenterPos();
		enemySoldiersKilled++;
		if (enemySoldiersKilled % newSoldierPerEnemyKilled == 0) {
			CreateNewSoldier(spawnPos);		
		}
	}

	void CreateNewSoldier(Vector3 spawnPos) {
		Transform newSoldier = Instantiate(soldierPrefab, spawnPos, Quaternion.identity) as Transform;
		newSoldier.parent = legion;
		SoldierEntity soldierEntity = newSoldier.GetComponent<SoldierEntity>();
		soldierEntity.SetColor(legionColor);
		soldierEntity.SetLayer("Player");
		soldierEntity.SetRivalLayer("Enemy");
	}

	public void OnEnemyLegionDefeated() {
		Vector3 spawnPos = GetCenterPos();
		CreateNewSoldier(spawnPos);
	}
}
