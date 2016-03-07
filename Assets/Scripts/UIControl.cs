using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIControl : MonoBehaviour {

	public RectTransform gameOverBanner;
	public Text infoText;

	float startTime;

	void Start () {
		FindObjectOfType<PlayerLegionControl>().OnDefeated += OnGameOver;
		startTime = Time.time;
	}

	void OnGameOver() {
		float totalSurvivalTime = Time.time - startTime;
		infoText.text = string.Format("You survived for {0:F1} seconds", totalSurvivalTime);

		StartCoroutine(gameOverBannerAnim());
	}

	IEnumerator gameOverBannerAnim() {
		float percent = 0;
		float bannerSpeed = 2.5f;
		
		while (percent <= 1.0f) {
			gameOverBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-240, 0, percent);
			percent += bannerSpeed * Time.deltaTime;
			yield return null;
		}
	}
	
}
