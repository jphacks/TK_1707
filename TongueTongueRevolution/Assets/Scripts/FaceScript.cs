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

	private CascadeClassifier faceCascade;

	/*--------------------------------
     : Default Method
     --------------------------------*/
	// Use this for initialization
	void Start()
	{
		//カスケードファイルを読み込ませる
		string faceCascadePath = Application.streamingAssetsPath + "/Cascade/mouth.xml";
		faceCascade = new CascadeClassifier(faceCascadePath);
		if (faceCascade.empty())
		{
			Debug.LogError("NotFound file:'" + faceCascadePath + "'");
		}
		//カメラの初期化
		Init();
	}

	// Update is called once per frame
	void Update()
	{
		//カメラ画像の表示先があるか確認
		if (rawImage) {
			//カメラの取得準備ができているか確認
			if (startCVCam) {
				//CVCameraMatの初期化が終了している
				if (cvCameraMat) {
					//CVCameraMatが撮影中か確認
					if (cvCameraMat.isPlaying ()) {
						//CVCameraMatのフレームが更新されているか確認
						if (cvCameraMat.didUpdateThisFrame ()) {
							//Texture2Dが初期化されているか確認
							if (texture != null) {
								//カメラのMat画像を取得
								Mat cvCamMat = cvCameraMat.GetMat ();
								//Matが取得できているか確認
								if (cvCamMat != null) {
									//Matが空のデータじゃないか確認
									if (!cvCamMat.empty ()) {

										/* 画像加工開始 */

										if (!faceCascade.empty ()) {
											//全体のグレースケールの作成
											Mat grayMat = new Mat ();
											Imgproc.cvtColor (cvCamMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

											//グレースケール画像のヒストグラムの均一化
											Mat equalizeHistMat = new Mat ();
											Imgproc.equalizeHist (grayMat, equalizeHistMat);

											//カスケードで検出
											MatOfRect faces = new MatOfRect ();
											faceCascade.detectMultiScale (equalizeHistMat, faces, 1.1f, 2, 0 | Objdetect.CASCADE_SCALE_IMAGE, new Size (equalizeHistMat.cols () * 0.13, equalizeHistMat.cols () * 0.13), new Size ());

											if (faces.rows () > 0) {
												List<OpenCVForUnity.Rect> rectsList = faces.toList ();
												for (int i = 0; i < rectsList.ToArray().Length; i++) {
													//検出した顔の座標取得
													OpenCVForUnity.Rect faceRect = rectsList[i];

													/* 検出した顔に対する処理開始 */


													/* 検出した顔に対する処理終了 */
												}
											}
										}

										/* 画像加工終了 */

										//加工したMatが存在するか確認
										if (cvCamMat != null) {
											//Matが空のデータじゃないか確認
											if (!cvCamMat.empty ()) {
												try {
													//cvCamMatをTexture2Dに変換して反映する
													cvCameraMat.matToTexture2D (cvCamMat, texture);
												} catch (System.ArgumentException e) {
													Debug.Log (e.Message);
												} catch (System.Exception e) {
													Debug.Log ("OtherError");
												}
											}
										}
									}
									cvCamMat = null;
								}
							}
						}
					}
				}
			}
		} else {
			Debug.LogError("NotFound:rawImage");
		}
	}

	private void Init()
	{
		startCVCam = false;
		//カメラ画像の表示先があるか確認
		if (rawImage) {
			//cvCameraMatを取得していなければ取得
			if (cvCameraMat == null) {
				cvCameraMat = GetComponent<CVCameraMat> ();
			}
			if (cvCameraMat != null) {
				if (cvCameraMat.isInited ()) {
					//初期化している
					//カメラの状態を確認
					if (!cvCameraMat.isPlaying ()) {
						//カメラの撮影が止まっている
						//撮影開始
						cvCameraMat.Play ();
					} else {
						//カメラは撮影中
						//Texture2Dが初期化されているか確認
						if (texture != null) {
							//初期化されている
							startCVCam = true;
						} else {
							//初期化されていない
							//Texture2Dを初期化
							OnCVCameraMatInited();
						}
					}
				} else {
					//初期化していない
					//初期化して起動
					cvCameraMat.Init();
				}
			}
		} else {
			Debug.LogError("NotFound:rawImage");
		}
	}

	/*--------------------------------
     : CVCameraMat Method
     --------------------------------*/
	//CVCameraMatが初期化された時
	public void OnCVCameraMatInited()
	{
		if (rawImage) { 
			//CVMat読み込み待ち
			bool loadMatFlag = false;
			Mat cvCamMat = new Mat ();
			while (!loadMatFlag) {
				if (cvCameraMat) {
					if (cvCameraMat.isPlaying ()) {
						if (cvCameraMat.didUpdateThisFrame ()) {
							cvCamMat = cvCameraMat.GetMat ();
							if (cvCamMat != null) {
								if (!cvCamMat.empty ()) {
									loadMatFlag = true;
								}
							}
						}
					}
				}
			}
			//CVMatをTextureにセット
			if (loadMatFlag) {
				//Texture2Dの作成
				texture = new Texture2D ((int)cvCamMat.cols (), (int)cvCamMat.rows (), TextureFormat.RGBA32, false);
				if (texture) {
					//RawImageへTexture2Dを設定（表示されない時はrawImage.material.mainTexture）
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

	//CVCameraMatが解放された時
	public void OnCVCameraMatDisposed()
	{
		startCVCam = false;
		texture = null;
	}
}