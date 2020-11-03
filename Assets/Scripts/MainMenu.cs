using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public void Play ()
	{
		LoadLevel (2);
	}
	public void Quit ()
	{
		Application.Quit ();
	}
	public void ToMenu ()
	{
		LoadLevel (0);
	}
	public void ToIntro ()
	{
		LoadLevel (1);
	}
	public static void LoadLevel (int index)
	{
		AsyncOperation op = SceneManager.LoadSceneAsync (index);
		op.priority = 15;
	}
}

