using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneGenerator : MonoBehaviour {

	GameObject car;
	[SerializeField]
	Transform rightDirPoint;
	[SerializeField]
	Transform leftDirPoint;
	[SerializeField]
	Transform cloudPont;
	[SerializeField]
	Camera cam;
	Transform trans;

	void Start ()
	{
		trans = transform;
		cam = Camera.main;
		car = Resources.Load<GameObject> ("Prefabs/Car");
		Invoke ("CarSpawn", Random.Range(0.25f, 2f));
	}
	void Update ()
	{
		cam.backgroundColor = InGameMenu.inGame.progress.skyColor;
	}
	void CarSpawn ()
	{
		bool right = Random.value < 0.5f;
		Vector3 pos = right ? rightDirPoint.position : leftDirPoint.position;
		pos += Vector3.up * Random.Range (-0.5f, 0.5f) - Vector3.forward * 2;
		Instantiate (car, pos, Quaternion.identity, trans);
		Invoke ("CarSpawn", Random.Range(0.25f, 2f));
	}
}
