using UnityEngine;
using System.Collections;

public class Soldier : SoldierEntity {

	protected override void Update () {
		base.Update ();

		if (nextAttackTime < Time.time) {
			nextAttackTime = Time.time + attackInterval;
			StartCoroutine(Attack());
		}
	}

	protected override IEnumerator Attack () {

		Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.GetMask(LayerMask.LayerToName(rivalLayer)));
		if (colliders.Length > 0) {
			currentStatus = Status.Attack;
			nav.enabled = false;
			SoldierEntity soldierEntity = colliders[0].transform.GetComponent<SoldierEntity>();

			Vector3 originalPos = transform.position;
			Vector3 targetPos = colliders[0].transform.position + (transform.position - colliders[0].transform.position).normalized * attackRadius / 2f;

			float animationSpeed = 2f;
			// attack animation
			float percent = 0;
			bool hasAttacked = false;
			while(percent <= 1) {
				if (percent > 0.5f && !hasAttacked) {
					hasAttacked = true;
					if (soldierEntity != null) {
						soldierEntity.TakeDamage(damage, transform);
					}
					else {
						break;
					}
				}
				transform.position =  Vector3.Lerp(originalPos, targetPos, (-(percent * percent) + percent) * 4);
				percent += animationSpeed * Time.deltaTime;
				yield return null;
			}
			nav.enabled = true;
			currentStatus = Status.Idle;
		}
	}
}
