using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestHouseManager : MonoBehaviour {
	[SerializeField]
	SpriteRenderer sky;

	void Update ()
	{
		sky.color = InGameMenu.inGame.progress.skyColor;
	}
}
