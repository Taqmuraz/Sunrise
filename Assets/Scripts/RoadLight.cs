using UnityEngine;
using System.Collections;

public class RoadLight : MonoBehaviour
{
	bool isLight;
	SpriteRenderer rend;

	void Start ()
	{
		rend = transform.GetChild(0).GetComponent<SpriteRenderer> ();
	}

	void Update ()
	{
		bool d = !InGameMenu.inGame.progress.isDay;
		if (isLight != d) {
			isLight = d;
			SetLight ();
		}
	}
	void SetLight () {
		rend.enabled = isLight;
	}
}

