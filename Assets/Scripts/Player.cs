using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Transformable : MonoBehaviour
{
	public Transform trans { get; protected set; }

	protected virtual void Start ()
	{
		trans = transform;
	}
}

public class Player : Transformable {
	
	[SerializeField]
	Sprite[] playerStates;
	[SerializeField]
	SpriteRenderer body;

	void Update ()
	{
		Car near = Car.cars.Where ((Car c) => {
			Transform t = c.trans;
			Vector3 v = t.position - trans.position;
			return v.x > 0 && t.right.x < 0;
		}).FirstOrDefault();
		body.sprite = near ? playerStates [0] : playerStates [1];
	}
}
