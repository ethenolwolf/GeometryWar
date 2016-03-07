using UnityEngine;
using System.Collections;

[RequireComponent (typeof(NavMeshAgent))]
public class SoldierEntity : MonoBehaviour {
	public enum Status {
		Idle, Marching, Attack, Faint
	}

	public int hp = 6;
	public float speed = 2f;
	public int damage = 1;
	public float attackInterval = 2f;
	public float attackRadius = 2f;

	public float idleDistance = 2f;
	public float idleTime = 2f;

	protected LayerMask rivalLayer;
	protected NavMeshAgent nav;
	public Color damageColor = Color.red;

	public ParticleSystem deathParticleSystem;

	protected Status currentStatus;

	protected bool isLegionDest;
	protected Vector3 legionDest;
	protected Vector3 currentDest;

	protected float nextAttackTime;
	protected float idleTimeCount = 0;

	protected Rigidbody rb;

	protected Color legionColor;
	protected Material material;

	public event System.Action<SoldierEntity> OnSoldierDead;

	protected virtual void Awake () {
		rb = GetComponent<Rigidbody>();
		nav = GetComponent<NavMeshAgent>();
		material = GetComponent<Renderer>().material;
		nav.speed = speed;
		currentStatus = Status.Marching;
	}

	protected virtual void Update() {
		if (currentStatus == Status.Marching) {
			if ((transform.position - currentDest).sqrMagnitude <= Mathf.Pow(idleDistance, 2)) {
				idleTimeCount += Time.deltaTime;

				if (idleTimeCount >= idleTime) {
					currentStatus = Status.Idle;
				}

			} else {
				idleTimeCount = 0f;
			}
		} 
		else if (currentStatus == Status.Idle) {
			nav.enabled = false;

			if ((transform.position - currentDest).sqrMagnitude > Mathf.Pow(idleDistance, 2)) {
				currentStatus = Status.Marching;
				nav.enabled = true;
			}
		}
	}

	public void SetColor (Color color) {
		legionColor = color;
		material.color = color;
	}

	protected virtual IEnumerator Attack() {
		yield return null;
	}

	public void TakeDamage(int damage, Transform attacker) {
		hp -= damage;
		StartCoroutine(DamageAnim());
		SetDestination(attacker.position);
		if (hp < 0) {
			Die();
		}
	}

	protected virtual IEnumerator DamageAnim() {
		float animSpeed = 1 / 0.1f;
		float percent = 0;

		while (percent <= 1) {
			material.color = Color.Lerp(legionColor, damageColor, (-(percent * percent) + percent) * 4);
			percent += Time.deltaTime * animSpeed;
			yield return null;
		}

		material.color = legionColor;
	}

	protected virtual void Die() {
		ParticleSystem deathEffect = Instantiate(deathParticleSystem, transform.position, Quaternion.Euler(-90f, 0f, 0f)) as ParticleSystem;
		deathEffect.GetComponent<Renderer>().material.color = legionColor;
		Destroy(deathEffect.gameObject, deathEffect.duration + deathEffect.startLifetime);

		if (OnSoldierDead != null) {
			OnSoldierDead(this);
		}
		Destroy(gameObject);
	}

	public void SetLayer(string layerName) {
		gameObject.layer = LayerMask.NameToLayer(layerName);
	}

	public void SetRivalLayer(string layerName) {
		rivalLayer = LayerMask.NameToLayer(layerName);
	}

	public void SetLegionDestination(Vector3 dest) {
		legionDest = dest;
		if (currentStatus == Status.Marching || currentStatus == Status.Idle) {
			SetDestination(legionDest);
		}
	}

	public virtual void SetDestination(Vector3 dest) {
		currentDest = dest;
		nav.enabled = true;
		nav.SetDestination(dest);
	}

	public void ResumeMarching() {
		currentStatus = Status.Marching;
		nav.enabled = true;
		nav.SetDestination(legionDest);
	}

	public void SetStatus(Status status) {
		currentStatus = status;
	}
}
