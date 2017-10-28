using UnityEngine;
using System.Collections;
using System;
using OpenCVForUnity;
using UnityEngine.Events;

public class CVCameraMat : MonoBehaviour
{
	public string requestDeviceName = null;
	public bool flipVertical = false;
	public bool flipHorizontal = false;

	public UnityEvent OnInitedEvent;
	public UnityEvent OnDisposedEvent;

	public int requestWidth = 400;
	public int requestHeight = 400;

	private WebCamTexture webCamTexture;
	private WebCamDevice webCamDevice;

	private Mat rgbaMat;
	private Mat rotatedRgbaMat;
	private Color32[] colors;
	private bool initDone = false;
	private ScreenOrientation screenOrientation = ScreenOrientation.Unknown;

	/*------------------------------------------*
     * Default Method
     *------------------------------------------*/
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (initDone)
		{
			if (screenOrientation != Screen.orientation)
			{
				//角度が変わったので再度初期化
				Init();
			}
		}
	}

	/*------------------------------------------*
     * Init Method
     *------------------------------------------*/
	//設定せず初期化を開始
	public void Init()
	{
		if (OnInitedEvent == null)
		{
			OnInitedEvent = new UnityEvent();
		}
		if (OnDisposedEvent == null)
		{
			OnDisposedEvent = new UnityEvent();
		}
		webCamInit ();
	}

	//設定して初期化を開始
	public void Init(string deviceName, int requestWidth, int requestHeight)
	{
		this.requestDeviceName = deviceName;
		this.requestWidth = requestWidth;
		this.requestHeight = requestHeight;
		Init();
	}

	//初期化
	private void webCamInit()
	{
		//すでに初期化されている時は一旦解放する
		if (initDone) {
			Dispose ();
		}

		//カメラの希望があるかどうか確認する
		if (!String.IsNullOrEmpty (requestDeviceName)) {
			//使用できるカメラを参照する
			for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
				//希望のカメラと同じカメラがあったらそれを使用する
				webCamDevice = WebCamTexture.devices [cameraIndex];
				if (webCamDevice.name == requestDeviceName) {
					webCamTexture = new WebCamTexture (requestDeviceName, requestWidth, requestHeight);
				}
			}
		}

		//希望のカメラが無かった場合
		if (webCamTexture == null) {
			//一番最初に認識したカメラを使用する
			if (WebCamTexture.devices.Length > 0) {
				webCamDevice = WebCamTexture.devices [0];
				webCamTexture = new WebCamTexture (webCamDevice.name, requestWidth, requestHeight);
			} else {
				webCamTexture = new WebCamTexture (requestWidth, requestHeight);
			}
		}

		//カメラの準備ができたかどうか確認
		if (webCamTexture) {
			//準備したカメラから映像の取得を開始する
			webCamTexture.Play ();

			//最初の撮影フレームを取得するまでコルーチンで待機
			GameObject coroutineObj = new GameObject("waitWebCamTexture");
			CVCameraMat coroutine = coroutineObj.AddComponent<CVCameraMat> ();
			coroutine.StartCoroutine(waitWebCamFrame(coroutineObj));
		} else {
			//カメラの準備ができなかったので再度初期化する
			webCamInit ();
		}
	}

	//Webカメラの最初のフレームが取得できるまで待機
	public IEnumerator waitWebCamFrame(GameObject coroutine)
	{
		while (true) {
			if (webCamTexture) {
				if (webCamTexture.isPlaying) {
					if (webCamTexture.didUpdateThisFrame) {
						colors = new Color32[webCamTexture.width * webCamTexture.height];
						rgbaMat = new Mat (webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);

						screenOrientation = Screen.orientation;

						initDone = true;

						if (OnInitedEvent != null) {
							OnInitedEvent.Invoke ();
						}

						Destroy(coroutine);
						break;
					}
				}
			}
			yield return new WaitForSeconds (1);
		}
	}

	//初期化したかどうか
	public bool isInited()
	{
		return initDone;
	}

	//破棄処理
	public void Dispose()
	{
		initDone = false;

		if (webCamTexture != null)
		{
			webCamTexture.Stop();
			webCamTexture = null;
		}
		if (rgbaMat != null)
		{
			rgbaMat.Dispose();
			rgbaMat = null;
		}
		if (rotatedRgbaMat != null)
		{
			rotatedRgbaMat.Dispose();
			rotatedRgbaMat = null;
		}
		colors = null;

		if (OnDisposedEvent != null)
			OnDisposedEvent.Invoke();
	}

	/*------------------------------------------*
     * WebCamTexture Method
     *------------------------------------------*/
	//WebCamTextureの撮影開始
	public void Play()
	{
		if (initDone)
		{
			webCamTexture.Play();
		}
	}

	//WebCamTextureの停止
	public void Pause()
	{
		if (initDone)
		{
			webCamTexture.Pause();
		}
	}

	//WebCamTextureの撮影終了
	public void Stop()
	{
		if (initDone)
		{
			webCamTexture.Stop();
		}
	}

	//WebCamTextureが初期化されているか
	public bool isPlaying()
	{
		if (!initDone)
		{
			return false;
		}
		return webCamTexture.isPlaying;
	}

	//WebCamTextureを返す
	public WebCamTexture GetWebCamTexture()
	{
		return webCamTexture;
	}

	//WebCamDeviceを返す
	public WebCamDevice GetWebCamDevice()
	{
		return webCamDevice;
	}

	//WebCamTextureが最後のフレームから更新されているかを返す
	public bool didUpdateThisFrame()
	{
		if (!initDone)
		{
			return false;
		}
		return webCamTexture.didUpdateThisFrame;
	}

	//WebCamTextureをMatに変換して返す
	public Mat GetMat()
	{
		if (!initDone || !webCamTexture.isPlaying)
		{
			if (rotatedRgbaMat != null)
			{
				return rotatedRgbaMat;
			}
			else
			{
				return rgbaMat;
			}
		}

		if (rgbaMat == null)
		{
			rgbaMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);
		}

		Utils.webCamTextureToMat(webCamTexture, rgbaMat, colors);

		int flipCode = int.MinValue;

		if (webCamDevice.isFrontFacing)
		{
			if (webCamTexture.videoRotationAngle == 0)
			{
				flipCode = 1;
			}
			else if (webCamTexture.videoRotationAngle == 90)
			{
				flipCode = 0;
			}
			if (webCamTexture.videoRotationAngle == 180)
			{
				flipCode = 0;
			}
			else if (webCamTexture.videoRotationAngle == 270)
			{
				flipCode = 1;
			}
		}
		else
		{
			if (webCamTexture.videoRotationAngle == 180)
			{
				flipCode = -1;
			}
			else if (webCamTexture.videoRotationAngle == 270)
			{
				flipCode = -1;
			}
		}

		if (flipVertical)
		{
			if (flipCode == int.MinValue)
			{
				flipCode = 0;
			}
			else if (flipCode == 0)
			{
				flipCode = int.MinValue;
			}
			else if (flipCode == 1)
			{
				flipCode = -1;
			}
			else if (flipCode == -1)
			{
				flipCode = 1;
			}
		}

		if (flipHorizontal)
		{
			if (flipCode == int.MinValue)
			{
				flipCode = 1;
			}
			else if (flipCode == 0)
			{
				flipCode = -1;
			}
			else if (flipCode == 1)
			{
				flipCode = int.MinValue;
			}
			else if (flipCode == -1)
			{
				flipCode = 0;
			}
		}

		if (flipCode > int.MinValue)
		{
			Core.flip(rgbaMat, rgbaMat, flipCode);
		}


		if (rotatedRgbaMat != null)
		{

			using (Mat transposeRgbaMat = rgbaMat.t())
			{
				Core.flip(transposeRgbaMat, rotatedRgbaMat, 1);
			}

			return rotatedRgbaMat;
		}
		else
		{
			return rgbaMat;
		}
	}

	/*------------------------------------------*
     * OpenCV Support Method
     *------------------------------------------*/
	//MatをTexture2Dに変換して反映する
	public void matToTexture2D(Mat mat, Texture2D texture)
	{
		Utils.matToTexture2D (mat, texture, colors);
	}
}
