using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;
using System.Collections.Generic;

[RequireComponent(typeof(CVCameraMat))]
public class FaceScript : MonoBehaviour {

	public RawImage rawImage;
	private CVCameraMat cvCameraMat;
	private Texture2D texture;
	private bool startCVCam = false;
	private int x, y,formerX, formerY, dx, dy;
	public int lineNum;
	private GameManager _gameManager;

	private CascadeClassifier faceCascade;

	void Start()
	{
		_gameManager = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		string faceCascadePath = Application.streamingAssetsPath + "/Cascade/mouth.xml";
		faceCascade = new CascadeClassifier(faceCascadePath);
		if (faceCascade.empty())
		{
			Debug.LogError("NotFound file:'" + faceCascadePath + "'");
		}
		Init();
	}

	void Update()
	{
		if (rawImage) {
			if (startCVCam && cvCameraMat && cvCameraMat.isPlaying () && cvCameraMat.didUpdateThisFrame () && cvCameraMat.didUpdateThisFrame () && cvCameraMat.didUpdateThisFrame () && texture != null) {
				Mat cvCamMat = cvCameraMat.GetMat ();
				if (cvCamMat != null && !cvCamMat.empty () && !faceCascade.empty ()) {
					Mat grayMat = new Mat ();
					Imgproc.cvtColor (cvCamMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

					Mat equalizeHistMat = new Mat ();
					Imgproc.equalizeHist (grayMat, equalizeHistMat);

					MatOfRect faces = new MatOfRect ();
					faceCascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new Size (equalizeHistMat.cols () * 0.13, equalizeHistMat.cols () * 0.13), new Size ());

					if (faces.rows () > 0) {
						List<OpenCVForUnity.Rect> rectsList = faces.toList ();
						for (int i = 0; i < rectsList.ToArray().Length; i++) {
							OpenCVForUnity.Rect faceRect = rectsList[i];
							x = faceRect.x;
							y = faceRect.y;

							if (i > 0) {
								//左上が（０、０）、右下が（１００、１００）
								OpenCVForUnity.Rect beforeFaceRect = rectsList [i-1];
								formerX = beforeFaceRect.x;
								formerY = beforeFaceRect.y;
								dx = x - formerX;
								dy = y - formerY;
								_gameManager.setDxDy (dx, dy);
								Debug.Log(x + ":"+y);
							}
						}
					}
				}

				if (cvCamMat != null && !cvCamMat.empty ()) {
					try {
						cvCameraMat.matToTexture2D (cvCamMat, texture);
					} catch (System.ArgumentException e) {
						Debug.Log (e.Message);
					} catch (System.Exception e) {
						Debug.Log ("OtherError");
					}
					cvCamMat = null;
				}
			}
		} else {
			Debug.LogError("NotFound:rawImage");
		}
	}

	public void Init()
	{
		startCVCam = false;
		if (rawImage) {
			if (cvCameraMat == null) {
				cvCameraMat = GetComponent<CVCameraMat> ();
			}
			if (cvCameraMat != null) {
				if (cvCameraMat.isInited ()) {
					if (!cvCameraMat.isPlaying ()) {
						cvCameraMat.Play ();
					} else {
						if (texture != null) {
							startCVCam = true;
						} else {
							OnCVCameraMatInited();
						}
					}
				} else {
					cvCameraMat.Init();
				}
			}
		} else {
			Debug.LogError("NotFound:rawImage");
		}
	}

	public void OnCVCameraMatInited()
	{
		if (rawImage) {
			bool loadMatFlag = false;
			Mat cvCamMat = new Mat ();
			while (!loadMatFlag) {
				if (cvCameraMat && cvCameraMat.isPlaying () && cvCameraMat.didUpdateThisFrame ()) {
					cvCamMat = cvCameraMat.GetMat ();
					if (cvCamMat != null && !cvCamMat.empty ()) {
						loadMatFlag = true;
					}
				}
			}
			if (loadMatFlag) {
				texture = new Texture2D ((int)cvCamMat.cols (), (int)cvCamMat.rows (), TextureFormat.RGBA32, false);
				if (texture) {
					rawImage.texture = texture;
					startCVCam = true;
				} else {
					Init ();
				}
			}
		} else {
			Debug.LogError("NotFound:rawImage");
		}
	}

	public void OnCVCameraMatDisposed(){
		startCVCam = false;
		texture = null;
	}

	void CheckInput(KeyCode key){
		if (Input.GetKeyDown (key)) {
			_gameManager.GoodTimingFunc (lineNum);
			Destroy (this.gameObject);
		}
	}
}