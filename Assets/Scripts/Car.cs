using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Car : Transformable {

	public static List<Car> cars
	{
		get {
			return _cars;
		}
		private set {
			_cars = value;
		}
	}

	private static List<Car> _cars = new List<Car>();

	[SerializeField]
	SpriteRenderer body;
	SpriteRenderer[] chs;
	[SerializeField]
	Color[] colors;
	float speed;

	protected override void Start () {
		base.Start ();
		chs = GetComponentsInChildren<SpriteRenderer> ();
		cars = cars.Where ((Car c) => c).ToList();
		cars.Add (this);
		trans = transform;
		SetCar (trans.position.x < 0);
	}
	public static Sprite LoadSprite (string path)
	{
		return Resources.LoadAll<Sprite> (path).FirstOrDefault ();
	}
	void SetCar (bool right)
	{
		speed = Random.Range (7, 15f);
		trans.right = right ? Vector3.right : Vector3.left;
		body.color = colors[Random.Range(0, colors.Length)];
		body.sprite = LoadSprite ("Sprites/Cars/Body_" + Random.Range (0, 4));
		foreach (var r in chs) {
			int l = 1 - (int)trans.position.y * 10;
			r.sortingOrder = r == body ? l : l + 1;
		}
	}
	void FixedUpdate ()
	{
		CarMotion ();
	}
	void CarMotion ()
	{
		trans.position += trans.right * Time.fixedDeltaTime * speed;
		if (trans.position.magnitude > 40) {
			Destroy (gameObject);
		}
	}
	void OnDestroy ()
	{
		cars.Remove (this);
	}
}
