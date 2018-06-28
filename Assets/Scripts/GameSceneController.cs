using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour {

	const string SAVE_KEY = "Key:Save";
	
	[SerializeField] private Text text;
	[SerializeField] private Image fadeImage;
	[SerializeField] private Button logButton;
	[SerializeField] private Button saveButton;
	[SerializeField] private Text logText;
	[SerializeField] private GameObject selectButtonArea;
	[SerializeField] private GameObject selectButtonPrefab;

	private List<string> novelLogs;
	private string[] novelTexts;
	private bool isGameStart;
	private bool isWriting;
	private bool isTextEnd;
	private int lineCount;

	void Awake() {
		novelLogs = new List<string> ();
		isGameStart = false;
		isWriting = false;
		isTextEnd = false;
		lineCount = 0;
	}

	// Use this for initialization
	IEnumerator Start () {
		logButton.onClick.AddListener (DrawLog);
		saveButton.onClick.AddListener (Save);

		yield return StartCoroutine (LoadNovel ());

		StartCoroutine(ExecAction ());

		isGameStart = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isGameStart)
			return;

		if (novelTexts.Length > lineCount && Input.GetMouseButtonDown(0)) {
			if (!isWriting) {
				StartCoroutine(ExecAction ());
			} else {
				isTextEnd = true;
			}
		}
	}

	IEnumerator ExecAction() {
		if (novelTexts[lineCount].StartsWith("@FadeIn")) {
			yield return StartCoroutine (FadeIn ());
		} else if (novelTexts[lineCount].StartsWith("@FadeOut")) {
			yield return StartCoroutine (FadeOut ());
		} else if (novelTexts[lineCount].StartsWith("@SELECT")) {
			Select ();
		} else {
			yield return StartCoroutine (WriteText ());
		}

		++lineCount;
	}

	IEnumerator WriteText() {
		isWriting = true;

		var waitForSeconds = new WaitForSeconds (0.1f);
		for (int i = 0; i < novelTexts[lineCount].Length; ++i) {
			if (isTextEnd) {
				text.text = novelTexts [lineCount];
				isTextEnd = false;
				break;
			}

			text.text = novelTexts [lineCount].Substring(0, i);

			yield return waitForSeconds;
		}

		novelLogs.Add (text.text);
		isWriting = false;
	}

	IEnumerator LoadNovel() {
		using(var www = new WWW ("file://" + Application.streamingAssetsPath + "/novel.txt")) {
			yield return www;

			novelTexts = www.text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
		}
	}

	IEnumerator FadeIn() {
		var commands = novelTexts [lineCount].Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

		float fadeTime = float.Parse(commands[1]);
		float elapsedTime = 0f;

		while (fadeImage.color.a > 0f) {
			elapsedTime += Time.deltaTime;

			var color = fadeImage.color;
			color.a = 1 - Mathf.Min (elapsedTime / fadeTime, 1f);
			fadeImage.color = color;

			yield return null;
		}

		fadeImage.raycastTarget = false;
	}

	IEnumerator FadeOut() {
		fadeImage.raycastTarget = true;

		var commands = novelTexts [lineCount].Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

		float fadeTime = float.Parse(commands[1]);
		float elapsedTime = 0f;

		while (fadeImage.color.a < 1f) {
			elapsedTime += Time.deltaTime;

			var color = fadeImage.color;
			color.a = Mathf.Min (elapsedTime / fadeTime, 1f);
			fadeImage.color = color;

			yield return null;
		}

		text.text = String.Empty;
	}

	void Select() {
		var selects = novelTexts [lineCount].Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		int selectNum = selects.Length - 1;
		for (int i = 0; i < selectNum; ++i) {
			var selectButton = Instantiate (selectButtonPrefab, selectButtonArea.transform, false);
//			selectButton.GetComponent<Button> ().onClick.AddListener ();
		}
	}

	void DrawLog() {
		foreach (var log in novelLogs) {
			logText.text += log + Environment.NewLine;
		}

		logText.transform.parent.gameObject.SetActive (true);
	}

	void Save() {
		PlayerPrefs.SetInt (SAVE_KEY, lineCount);
		PlayerPrefs.Save ();
	}

	void Load() {
		lineCount = PlayerPrefs.GetInt (SAVE_KEY);
	}
}
