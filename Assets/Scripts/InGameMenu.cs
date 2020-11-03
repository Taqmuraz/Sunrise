using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public struct PathParameters
{
	public float hoursCar { get { return dist / carSpeed; } }
	public float hoursHuman { get { return dist / humanSpeed; } }
	public string destinationName;
	public float dist;

	public const float carSpeed = 30;
	public const float humanSpeed = 7;

	public PathParameters (string _destName, float _dist)
	{
		dist = _dist;
		destinationName = _destName;
	}

	public static PathParameters[] pathParametersAll
	{
		get {
			PathParameters[] pps =
			{
				new PathParameters("Москва", 0),
				new PathParameters("Нижний новгород", 420),
				new PathParameters("Екатеринбург", 1000),
				new PathParameters("Омск", 840),
				new PathParameters("Новосибирск", 600),
				new PathParameters("Красноярск", 540)
			};
			return pps;
		}
	}

	public static PathParameters[] FromTo (string fr, string to)
	{
		List<PathParameters> pps = PathParameters.pathParametersAll.ToList ();

		PathParameters start = pps.FirstOrDefault ((PathParameters pp) => pp.destinationName == fr);
		PathParameters end = pps.FirstOrDefault ((PathParameters pp) => pp.destinationName == to);

		int s = pps.IndexOf (start);
		int e = pps.IndexOf (end);

		return pps.Where ((PathParameters pp) => {

			int i = pps.IndexOf (pp);

			return i > s && !(i > e);
		}).ToArray ();
	}
}

public class ProgressBlock
{
	int _day;
	float _time;
	public float hunger { get; private set; }
	public float weakness { get; private set; }
	public int money { get; private set; }
	public string currentPointName { get; private set; }

	public event Action OnDayLeft;

	public ProgressBlock ()
	{
		hunger = 25;
		weakness = 50;
		money = 500;
	}

	public void Init ()
	{
		currentPointName = PathParameters.pathParametersAll [0].destinationName;
	}

	public void AddMoney (int val)
	{
		int sum = money + val;
		money = sum > 0 ? sum : 0;
	}

	public void AddTime (float hours)
	{
		float t = hours / 24f;
		time += t * dayLength;
	}

	public void AddWeakness (float val)
	{
		float resoult = weakness + val;
		weakness = ClampParam (resoult);
	}

	public void DriveTo (string pointName)
	{
		float dist = FullDist (pointName);
		AddMoney ((int)(-dist * costPerKm));
		MoveTo (pointName, PathParameters.carSpeed);
	}
	public void WalkTo (string pointName)
	{
		MoveTo (pointName, PathParameters.humanSpeed);
	}
	public void MoveTo (string pointName, float speed)
	{
		float dist = FullDist (pointName);
		PathParameters pp = new PathParameters (pointName, dist);
		currentPointName = pp.destinationName;
		AddTime (pp.dist / speed);
	}

	public PathParameters[] future
	{
		get
		{
			PathParameters[] pps = PathParameters.FromTo (currentPointName, PathParameters.pathParametersAll.LastOrDefault().destinationName);
			return pps;
		}
	}

	public void WorkInMac ()
	{
		AddWeakness (15);
		AddTime (8);
		AddMoney (macWorkCost);
	}

	public float FullDist (string pointName)
	{
		float dist = 0;
		PathParameters[] all = PathParameters.FromTo (currentPointName, pointName);
		foreach (PathParameters f in all) {
			dist += f.dist;
		}
		return dist;
	}

	public void AddHunger (float val)
	{
		float resoult = hunger + val;
		hunger = ClampParam (resoult);
		if (!(hunger * (100 - weakness) > 0)) {
			InGameMenu.inGame.HungerDeath ();
		}
	}
	public void Eat ()
	{
		int mon = money;
		AddMoney (foodCost);
		AddHunger (mon / 5);
	}
	float ClampParam (float val)
	{
		return Mathf.Clamp (val, 0f, 100f);
	}

	public float localTime
	{
		get {
			return time % dayLength;
		}
	}

	void TimeLeftControl (float delta)
	{
		float wq = InGameMenu.inGame.activeState.name == "GuestHouse" ? weaknessByRest : weaknessByRoad;
		AddWeakness (wq / dayLength * delta);
		AddHunger (hungerByDay / dayLength * delta);
	}

	void DaysLeft (int count)
	{
		for (int i = 0; i < count; i++) {
			OnDayLeft ();
		}
	}

	public int daysLeft
	{
		get {
			return (int)(time / dayLength);
		}
	}

	public float time
	{
		get {
			return _time;
		}
		private set {
			float delta = value - _time;

			TimeLeftControl (delta);

			_time = value;

			int dpt = (int)(_time / dayLength);
			if (dpt != _day) {
				int d = dpt - _day;
				DaysLeft (d);
				_day = dpt;
			}
		}
	}

	public const float hungerByDay = -10f;
	public const float weaknessByRoad = 10f;
	public const float weaknessByRest = -50f;
	public const int guestHouseCost = 600;
	public const int foodCost = -450;
	public const float costPerKm = 5.5f;
	public const int macWorkCost = 1200;

	public bool isDay
	{
		get {
			return localTime > dayLength / 4f && localTime < dayLength / 4f * 3f; 
		}
	}

	public const float dayLength = 100f;

	static Color nightColor = Color.black;
	static Color dayColor = new Color (156 / 255f, 220 / 255f, 229 / 255f);
	static Color sunsetColor = new Color (255f / 255f, 186 / 255f, 4 / 255f);
	static Color sunriseColor = new Color (141 / 255f, 55 / 255f, 178f / 255f);

	public Color skyColor
	{
		get {
			float cur = time % dayLength;
			Color[] clrs = {nightColor, sunriseColor, dayColor, sunsetColor};
			int part = (int)(dayLength / clrs.Length);
			int curPart = (int)cur / part;
			float delta = cur - (curPart * part);
			Color a = clrs[curPart % clrs.Length];
			Color b = clrs[(curPart + 1) % clrs.Length];
			Color clr = Color.Lerp (a, b, delta / (dayLength / clrs.Length));
			return clr;
		}
	}

	public void TimeUpdate ()
	{
		time += Time.deltaTime;
	}
}
public abstract class ADynamic
{
	public virtual void Update () {}
	public ADynamic ()
	{
		InGameMenu.inGame.AddDynamic (this);
	}
}
public class ValueDynamic : ADynamic
{
	Scrollbar scroll;
	ReadValue readValue;
	public delegate float ReadValue ();

	public ValueDynamic (Scrollbar _scroll, ReadValue _readValue) : base ()
	{
		scroll = _scroll;
		readValue = _readValue;
	}

	public override void Update ()
	{
		base.Update ();
		scroll.size = readValue () / 100f;
	}
}
public class TextDynamic : ADynamic
{
	Text text;
	ReadText readStream;
	public delegate string ReadText ();

	public TextDynamic (Text _text, ReadText _readStream) : base ()
	{
		text = _text;
		readStream = _readStream;
	}
	public override void Update ()
	{
		base.Update ();
		text.text = readStream ();
	}
}
public class InGameMenu : MonoBehaviour {

	List <ADynamic> dynamics { get; set; }

	public void AddDynamic (ADynamic d)
	{
		if (dynamics == null) {
			dynamics = new List<ADynamic> ();
		}
		dynamics.Add (d);
	}

	public event Action OnHungerDeath;

	public void HungerDeath ()
	{
		InGameMenu.inGame.OnHungerDeath ();
	}

	[SerializeField]
	RectTransform buttonsParent;
	public ProgressBlock progress { get; private set; }

	public static InGameMenu inGame { get; private set; }

	public List<InGameState> states { get; private set; }

	public int buttonsAll { get; set; }

	[SerializeField]
	GameObject roadScene;
	[SerializeField]
	GameObject guestHouseScene;
	[SerializeField]
	GameObject hungerDeathScene;
	[SerializeField]
	GameObject inPathScene;
	[SerializeField]
	GameObject workingScene;

	[SerializeField]
	Scrollbar hungerBar;
	[SerializeField]
	Scrollbar weaknessBar;
	[SerializeField]
	Text moneyText;
	[SerializeField]
	Text pointText;

	public InGameState activeState { get; private set; }

	void ChangeState (string name)
	{
		InGameState state = states.FirstOrDefault (((InGameState st) => st.name == name));
		if (activeState && name == activeState.name) {
			return;
		}
		if (state && activeState) {
			activeState.Disable ();
		}
		activeState = state;
		activeState.Enable ();
	}

	void InitGame ()
	{
		progress.Init ();
	}

	void GameOver ()
	{
		MainMenu.LoadLevel (0);
	}

	void CreateDynamics ()
	{
		TextDynamic m = new TextDynamic (moneyText, () => "Деньги :\n" + progress.money);
		TextDynamic p = new TextDynamic (pointText, () => "Текущий город : " + progress.currentPointName + "\nДней прошло : "  + progress.daysLeft + ")");
		ValueDynamic h = new ValueDynamic (hungerBar, () => progress.hunger);
		ValueDynamic w = new ValueDynamic (weaknessBar, () => progress.weakness);
	}

	string Cost (int val)
	{
		return "(" + val + " руб)";
	}
	string Cost (float val)
	{
		return Cost ((int)val);
	}

	ActionButton[] RoadButtons ()
	{
		PathParameters[] pps = progress.future;
		List<ActionButton> roadBts = new List<ActionButton> ();
		roadBts.AddRange (pps.Select ((PathParameters pp) => {

			float dist = progress.FullDist (pp.destinationName);

			return AddButton ("Ехать до : " + pp.destinationName + "\n(" + dist + ") km, " + Cost (dist * ProgressBlock.costPerKm), () => {
				progress.DriveTo (pp.destinationName);
				ChangeState ("InPath");
			});
		}));
		roadBts.AddRange (pps.Select ((PathParameters pp) => {

			float dist = progress.FullDist (pp.destinationName);

			return AddButton ("Идти до : " + pp.destinationName + "\n(" + dist + ") km", () => {
				progress.WalkTo (pp.destinationName);
				ChangeState ("InPath");
			});
		}));
		return roadBts.ToArray ();
	}

	void CreateStates ()
	{
		//ActionButton toRoad = AddButton ("Уйти на дорогу", () => ChangeState("Road"));
		ActionButton sleep = AddButton ("Спать до следующего дня", () => {
			progress.AddTime(24);
			progress.AddMoney(-ProgressBlock.guestHouseCost);
			ChangeState("Road");
		});
		ActionButton food = AddButton ("Купить еду " + Cost((progress.money > ProgressBlock.foodCost ? ProgressBlock.foodCost : progress.money)), () => progress.Eat());
		ActionButton guest = AddButton ("В гостинницу (1 день, " + Cost (ProgressBlock.guestHouseCost), () => {
			if (!(progress.money < ProgressBlock.guestHouseCost))
				ChangeState ("GuestHouse");
		});
		ActionButton macWork = AddButton ("Работать в Маке (+" + Cost(ProgressBlock.macWorkCost) + ")", () => ChangeState("Working"));

		InGameState road = new InGameState ("Road", roadScene);

		road.SetInit (() => {
			List<ActionButton> rBts = new List<ActionButton>();
			rBts.Add(guest);
			rBts.Add(food);
			rBts.Add(macWork);
			rBts.AddRange(RoadButtons());
			road.SetActions(rBts.ToArray());
		});

		InGameState guestHouse = new InGameState ("GuestHouse", guestHouseScene,
			                         food,
			                         sleep);
		InGameState hungerDeath = new InGameState ("HungerDeath", hungerDeathScene,
			                          AddButton ("Завершить игру", () => GameOver ()));

		InGameState inPath = new InGameState ("InPath", inPathScene);
		inPath.SetInit (() => Invoke("ToRoad", 2));

		InGameState working = new InGameState ("Working", workingScene);
		working.SetInit (() => {
			Invoke("Work", 2);
		});

		OnHungerDeath += () => ChangeState (hungerDeath.name);
		progress.OnDayLeft += () => {
			if (activeState.name == "GuestHouse") {
				if (progress.money < ProgressBlock.guestHouseCost) {
					ChangeState("Road");
				}
			}
		};
		ChangeState ("Road");
	}

	void Work ()
	{
		progress.WorkInMac ();
		ToRoad ();
	}

	void ToRoad ()
	{
		if (activeState.name != "HungerDeath") {
			if (progress.future.Length < 1) {
				MainMenu.LoadLevel (3);
			}
			ChangeState ("Road");
		}
	}

	void Awake ()
	{
		states = new List<InGameState> ();
		progress = new ProgressBlock ();
		inGame = this;
		InitGame ();
		CreateDynamics ();
		CreateStates ();
	}
	void Update ()
	{
		progress.TimeUpdate ();
		DynamicsUpdate ();

	}
	void DynamicsUpdate ()
	{
		foreach (var d in dynamics) {
			d.Update ();
		}
	}
	public ActionButton AddButton (string text, Action action)
	{
		Rect r = buttonsParent.parent.parent.GetComponent<RectTransform> ().rect;
		Vector3 size = new Vector3 (r.width, 70);
		ActionButton button = new ActionButton (text, action, buttonsParent, -Vector3.up * buttonsAll * size.y, size);
		return button;
	}
}
