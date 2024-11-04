using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class VirtualCameraController : MonoBehaviour
{
	public CinemachineVirtualCamera[] virtualCameras = new CinemachineVirtualCamera[] { };
	private int currentCameraIndex = 0;

	[SerializeField] public LayerMask[] vCamRenderLayer = new LayerMask[] { };

	#region カメラ同期用変数
	private int SyncCameraIndex = 0;
	public int SyncCameraIndexProperty
	{
		get { return SyncCameraIndex; }
		set { SyncCameraIndex = value; }
	}
	#endregion

	#region InputSystemでカメラをスイッチさせる機能

	[SerializeField] public bool useGamePadController = true;
	[SerializeField] public string inputKey = "D_Pad_Vertical";
	[SerializeField] public float OnThreashold = 0.9f;
	[SerializeField] public float OffThreashold = 0.25f;

	#endregion GamePadでカメラをスイッチさせる機能

	#region テンキーでカメラをスイッチさせる機能
	[SerializeField] public bool useNumPad = false;

	[SerializeField]
	public KeyCode[] Kaypad =
	{
		KeyCode.Keypad1,
		KeyCode.Keypad2,
		KeyCode.Keypad3,
		KeyCode.Keypad4,
		KeyCode.Keypad5,
		KeyCode.Keypad6,
		KeyCode.Keypad7,
		KeyCode.Keypad8,
		KeyCode.Keypad9,
		KeyCode.Keypad0,
	};

	private void CameraSwichKeypad()
	{
		if (!useNumPad) { return; }
		for (int i = 0; i < Kaypad.Length; i++)
		{
			if (Input.GetKeyDown(Kaypad[i]))
			{
				SetVcam(i);
			}
		}
	}
	#endregion テンキーでカメラをスイッチさせる機能

	private void Reset()
	{
		AutoVcamsRegister();
		RenderLayerResize();
	}

	// Inspector更新CallBack
	public void OnValidate()
	{
		AutoVcamsRegister();
		RenderLayerResize();
	}

	// VirtualCameraとVCamRenderLayerの数が違ったら同じにする
	public void RenderLayerResize()
	{
		while (virtualCameras.Length != vCamRenderLayer.Length)
		{
			if (virtualCameras.Length > vCamRenderLayer.Length)
			{
				System.Array.Resize(ref vCamRenderLayer, vCamRenderLayer.Length + 1);
				vCamRenderLayer[vCamRenderLayer.Length - 1] = ~0;
			}
			else if (virtualCameras.Length < vCamRenderLayer.Length)
			{
				System.Array.Resize(ref vCamRenderLayer, vCamRenderLayer.Length - 1);
			}
		}
	}

	void Start()
	{
		// Vcamの子供を取得して配列に保存
		AutoVcamsRegister();

		// 開始時のカメラIndexを取得
		currentCameraIndex = GetCurrentVcamIndex();

		// 開始時にLayerMaskの設定が漏れていた場合に自動で設定する
		if (vCamRenderLayer.Length != virtualCameras.Length)
		{
			if (vCamRenderLayer != null)
			{
				LayerMask[] initLayerMask = vCamRenderLayer;
				vCamRenderLayer = new LayerMask[virtualCameras.Length];

				for (int i = 0; i < initLayerMask.Length; i++)
				{
					vCamRenderLayer[i] = initLayerMask[i];
				}

				for (int j = initLayerMask.Length; j < vCamRenderLayer.Length; j++)
				{
					vCamRenderLayer[j] = Camera.main.cullingMask;
				}
			}
			else
			{
				vCamRenderLayer = new LayerMask[virtualCameras.Length];
				for (int i = 0; i < vCamRenderLayer.Length; i++)
				{
					vCamRenderLayer[i] = Camera.main.cullingMask;
				}
			}
		}
		// カメラのマスクを設定する
		Camera.main.cullingMask = vCamRenderLayer[currentCameraIndex];

#if UNITY_EDITOR
		SceneView.duringSceneGui += OnSceneView;
#endif
	}

	private void Update()
	{
		CameraSwichKeypad();
	}

	/// <summary>
	/// Cameraを次のカメラに切り変える。
	/// Camera は virtualCameras の範囲でループさせる 
	/// </summary>
	[ContextMenu("Next Vcams")]
	public void SetNextVirtualCamera()
	{
		// 現在のカメラのIndexを取得
		int currentIndex = GetCurrentVcamIndex();

		// CameraのIndexをインクリメントして範囲外なら0に戻す
		currentIndex++;
		if (currentIndex >= virtualCameras.Length) { currentIndex -= virtualCameras.Length; }
		SetVcam(currentIndex);
	}
	[ContextMenu("Prev Vcams")]
	public void SetPrevVirtualCamera()
	{
		// 現在のカメラのIndexを取得
		int currentIndex = GetCurrentVcamIndex();

		currentIndex--;
		if (currentIndex < 0) { currentIndex += virtualCameras.Length; }
		SetVcam(currentIndex);
	}

	public bool SetVcam(int cameraIndex)
	{
		SyncCameraIndex = cameraIndex;
		LayerMask num = LayerMask.GetMask(new string[] { LayerMask.LayerToName(15), LayerMask.LayerToName(16), LayerMask.LayerToName(17), });

		const int HighPriority = 10;
		const int LowPriority = 0;

		currentCameraIndex = cameraIndex;

		if (cameraIndex >= virtualCameras.Length) { return false; }

		// priority のリセット
		for (int i = 0; i < virtualCameras.Length; i++)
		{
			virtualCameras[i].Priority = LowPriority;
		}

		// priority の再セット
		virtualCameras[cameraIndex].Priority = HighPriority;
		Camera.main.cullingMask = vCamRenderLayer[cameraIndex];

		return true;
	}

	public bool SetVcam()
	{
		LayerMask num = LayerMask.GetMask(new string[] { LayerMask.LayerToName(15), LayerMask.LayerToName(16), LayerMask.LayerToName(17), });

		const int HighPriority = 10;
		const int LowPriority = 0;

		currentCameraIndex = SyncCameraIndex;

		if (SyncCameraIndex >= virtualCameras.Length) { return false; }

		// priority のリセット
		for (int i = 0; i < virtualCameras.Length; i++)
		{
			virtualCameras[i].Priority = LowPriority;
		}

		// priority の再セット
		virtualCameras[SyncCameraIndex].Priority = HighPriority;
		Camera.main.cullingMask = vCamRenderLayer[SyncCameraIndex];

		return true;
	}

	public int GetCurrentVcamIndex()
	{
		int maxPriority = int.MinValue;
		int resultIndex = int.MaxValue;

		for (int i = 0; i < virtualCameras.Length; i++)
		{
			int currentPriority = virtualCameras[i].Priority;
			if (maxPriority < currentPriority)
			{
				maxPriority = currentPriority;
				resultIndex = i;
			}
		}

		if (maxPriority == int.MinValue) { Debug.LogError("Get VirtualCamera Index Error!"); }
		return resultIndex;
	}

	[ContextMenu("Auto Vcams Register")]
	public void AutoVcamsRegister()
	{
		virtualCameras = transform.GetComponentsInChildren<CinemachineVirtualCamera>();
	}


#if UNITY_EDITOR
	void OnDestroy()
	{
		SceneView.duringSceneGui -= OnSceneView;
	}


	void OnSceneView(SceneView sceneView)
	{
		const float RecButtonWidth = 70f;
		Handles.BeginGUI();

		GUILayout.BeginArea(new Rect(0, 0
			, Screen.width / EditorGUIUtility.pixelsPerPoint
			, Screen.height / EditorGUIUtility.pixelsPerPoint));

		for (int index = 0; index < virtualCameras.Length; index++)
		{
			if (index == currentCameraIndex)
			{
				// 現在使用中のカメラならラベル。
				GUILayout.Label(virtualCameras[index].name, GUILayout.Width(RecButtonWidth));
			}
			else
			{
				// 使っていないもカメラは切り替えができるようボタンで配置する
				bool isPush = GUILayout.Button(virtualCameras[index].name, GUILayout.Width(RecButtonWidth));

				// ボタンが押されたら切り替える
				if (isPush)
				{
					// Buttonが押されたらそのカメラに切り替える
					SetVcam(index);
				}
			}
		}


		GUILayout.EndArea();

		Handles.EndGUI();
	}
#endif
}
