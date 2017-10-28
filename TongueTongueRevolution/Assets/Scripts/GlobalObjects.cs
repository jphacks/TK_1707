using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalObjects : MonoBehaviour {

	private static GlobalObjects sharedInstance = null;
	private string mStringParam = null;
	private object mParam = null;
	private object[] mParams = null;

	public string StringParam {
		get {
			return mStringParam;
		}
	}

	public object Param {
		get {
			return mParam;
		}
	}

	public object[] Params {
		get {
			return mParams;
		}
	}

	public static GlobalObjects getInstance ()
	{
		Debug.Log("receive" + sharedInstance.mStringParam);
		return sharedInstance;
	}

	public static void LoadLevelWithString (string levelName, string stringParam)
	{
		getInstance().mStringParam = stringParam;
		Debug.Log("send" + stringParam);
		Application.LoadLevel(levelName);
	}

	public static void LoadLevelWithObject (string levelName, object theParam)
	{
		getInstance().mParam = theParam;
		Application.LoadLevel(levelName);
	}

	public static void LoadLevelWithParams (string levelName, params object[] theParams)
	{
		getInstance().mParams = theParams;
		Application.LoadLevel(levelName);
	}

	public void Awake ()
	{
		if (sharedInstance == null) {
			Debug.Log("Awake GlobalObject");

			sharedInstance = this;
			DontDestroyOnLoad(gameObject);
		}

		Debug.Log ("StringParam = " + getInstance().StringParam);
	}

	public void execLoadLevelWithString (string stringParam)
	{
		GlobalObjects.LoadLevelWithString ("GameScene", stringParam);
	}
}
