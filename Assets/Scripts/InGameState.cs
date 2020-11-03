using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public delegate void Action ();

public class ActionButton
{
	public string text { get; private set; }
	public Action action { get; private set; }
	public Button buttonComponent { get; private set; }
	public Text textComponent { get; private set; }
	public Image imageComponent { get; private set; }
	public Transform parentTrans { get; private set; }
	public Vector3 position { get; private set; }
	public Vector3 localSize { get; private set; }

	public ActionButton (string _text, Action _action, Transform parent, Vector3 pos, Vector3 size)
	{
		parentTrans = parent;
		localSize = size;
		position = pos;
		text = _text;
		action = _action;
	}

	public void Enable ()
	{
		position = new Vector3 (position.x, -InGameMenu.inGame.buttonsAll * localSize.y);
		GameObject buttonObj = new GameObject ("Button : " + text);
		RectTransform rectTrans = buttonObj.AddComponent<RectTransform> ();
		rectTrans.SetParent (parentTrans);
		rectTrans.anchorMax = Vector2.up;
		rectTrans.anchorMin = Vector2.up;
		rectTrans.pivot = Vector2.up;
		rectTrans.anchoredPosition = position;
		rectTrans.localScale = Vector3.one;
		rectTrans.localPosition = new Vector3 (rectTrans.localPosition.x, rectTrans.localPosition.y, 0);
		rectTrans.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, localSize.x);
		rectTrans.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, localSize.y);
		Image img = buttonObj.AddComponent<Image> ();
		img.sprite = Car.LoadSprite ("Sprites/UI/BoxWhite");
		img.color = Color.gray;

		GameObject textObj = new GameObject ("Text");
		RectTransform textRect = textObj.AddComponent<RectTransform> ();
		textRect.SetParent (rectTrans);
		textRect.pivot = Vector2.up;
		textRect.localScale = Vector3.one;
		textRect.anchorMax = Vector2.up;
		textRect.anchorMin = Vector2.up;

		textRect.localPosition = new Vector3 (rectTrans.localPosition.x, rectTrans.localPosition.y, 0);
		textRect.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, rectTrans.rect.width);
		textRect.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, rectTrans.rect.height);

		textRect.anchoredPosition = Vector2.zero;

		Text txt = textObj.AddComponent<Text> ();
		txt.color = Color.black;
		txt.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault();
		txt.alignment = TextAnchor.MiddleCenter;
		txt.text = text;
		Button bt = buttonObj.AddComponent<Button> ();
		bt.image = img;
		bt.targetGraphic = img;

		bt.onClick.AddListener (() => action ());

		buttonComponent = bt;
		textComponent = txt;
		imageComponent = img;
		InGameMenu.inGame.buttonsAll++;
	}

	public void Destroy ()
	{
		if (buttonComponent) {
			MonoBehaviour.Destroy(buttonComponent.gameObject);
			InGameMenu.inGame.buttonsAll--;
		}
	}
}

public class InGameState : NullBool
{
	public string name { get; private set; }
	public ActionButton[] actions { get; private set; }
	public GameObject stateScenePrefab { get; private set; }
	GameObject stateScene;
	Action init = () => {
	};

	public InGameState (string _name, GameObject _stateScene, params ActionButton[] _actions)
	{
		name = _name;
		actions = _actions;
		stateScenePrefab = _stateScene;
		InGameMenu.inGame.states.Add (this);
	}

	public void SetInit (Action _init)
	{
		init = _init;
	}
	public void SetActions (ActionButton[] acts)
	{
		actions = acts;
	}

	public void Enable ()
	{

		init ();

		stateScene = MonoBehaviour.Instantiate (stateScenePrefab);

		ActionButton[] bts = new ActionButton[actions.Length];
		for (int i = 0; i < actions.Length; i++) {
			bts [i] = new ActionButton (actions[i].text, actions[i].action, actions[i].parentTrans, actions[i].position, actions[i].localSize);
			bts [i].Enable ();
		}
		actions = bts;

		if (actions.Length > 0) {
			RectTransform p = actions.FirstOrDefault ().parentTrans.GetComponent<RectTransform> ();
			Vector3 size = actions.FirstOrDefault ().localSize;
			p.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, size.y * (InGameMenu.inGame.buttonsAll));
		}
	}

	public void Disable ()
	{
		MonoBehaviour.Destroy (stateScene);
		foreach (var item in actions) {
			item.Destroy ();
		}
	}
}

