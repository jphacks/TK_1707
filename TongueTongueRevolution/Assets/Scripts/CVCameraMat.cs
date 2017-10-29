using UnityEngine;
using System.Collections;
using System;
using OpenCVForUnity;
using UnityEngine.Events;
using UnityEngine.UI;


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

	void Start()
	{
	}

	void Update()
	{
		if (initDone)
		{
			if (screenOrientation != Screen.orientation)
			{
				Init();
			}
		}
	}

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

	public void Init(string deviceName, int requestWidth, int requestHeight)
	{
		this.requestDeviceName = deviceName;
		this.requestWidth = requestWidth;
		this.requestHeight = requestHeight;
		Init();
	}

	private void webCamInit()
	{
		if (initDone) {
			Dispose ();
		}

		if (!String.IsNullOrEmpty (requestDeviceName)) {
			for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
				webCamDevice = WebCamTexture.devices [cameraIndex];
				if (webCamDevice.name == requestDeviceName) {
					webCamTexture = new WebCamTexture (requestDeviceName, requestWidth, requestHeight);
				}
			}
		}

		if (webCamTexture == null) {
			//縦なら回転させるらしい
			var euler = transform.localRotation.eulerAngles;

			transform.localRotation = Quaternion.Euler( euler.x, euler.y, euler.z - 90 );
			if (WebCamTexture.devices.Length > 0) {

				//スマホならインカメ -> 1
				//webCamDevice = WebCamTexture.devices [1];
				//macなら１つのカメラ -> 0
				webCamDevice = WebCamTexture.devices [0];
				webCamTexture = new WebCamTexture (webCamDevice.name, requestWidth, requestHeight);
			} else {
				webCamTexture = new WebCamTexture (requestWidth, requestHeight);
			}
		}
		if (webCamTexture) {
			webCamTexture.Play ();

			GameObject coroutineObj = new GameObject("waitWebCamTexture");
			CVCameraMat coroutine = coroutineObj.AddComponent<CVCameraMat> ();
			coroutine.StartCoroutine(waitWebCamFrame(coroutineObj));
		} else {
			webCamInit ();
		}
	}

	public IEnumerator waitWebCamFrame(GameObject coroutine)
	{
		while (true) {
			if (webCamTexture) {
				if (webCamTexture.isPlaying && webCamTexture.didUpdateThisFrame) {
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
			yield return new WaitForSeconds (1);
		}
	}

	public bool isInited()
	{
		return initDone;
	}

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

	public void Play()
	{
		if (initDone)
		{
			webCamTexture.Play();
		}
	}

	public void Pause()
	{
		if (initDone)
		{
			webCamTexture.Pause();
		}
	}

	public void Stop()
	{
		if (initDone)
		{
			webCamTexture.Stop();
		}
	}

	public bool isPlaying()
	{
		if (!initDone)
		{
			return false;
		}
		return webCamTexture.isPlaying;
	}

	public WebCamTexture GetWebCamTexture()
	{
		return webCamTexture;
	}

	public WebCamDevice GetWebCamDevice()
	{
		return webCamDevice;
	}

	public bool didUpdateThisFrame()
	{
		if (!initDone)
		{
			return false;
		}
		return webCamTexture.didUpdateThisFrame;
	}

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
		
	public void matToTexture2D(Mat mat, Texture2D texture)
	{
		Utils.matToTexture2D (mat, texture, colors);
	}
}
