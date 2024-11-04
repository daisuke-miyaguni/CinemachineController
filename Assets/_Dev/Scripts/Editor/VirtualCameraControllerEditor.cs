using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VirtualCameraController))]
public class VirtualCameraControllerEditor : Editor
{
	static bool BaseGUI = true;
	bool LayerListOpen = true;

	public override void OnInspectorGUI()
	{
		BaseGUI = EditorGUILayout.Foldout(BaseGUI, "BaseGUI");
		if (BaseGUI)
		{
			base.OnInspectorGUI();
			return;
		}
		else
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
			EditorGUI.EndDisabledGroup();

			serializedObject.Update();
			GUIStyle myStyle = new GUIStyle();
			myStyle.alignment = TextAnchor.MiddleCenter;
			myStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

			VirtualCameraController vCamController = target as VirtualCameraController;
			if (vCamController.virtualCameras.Length != vCamController.transform.childCount)
			{
				vCamController.AutoVcamsRegister();
				vCamController.RenderLayerResize();
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("<", GUILayout.Width((EditorGUIUtility.currentViewWidth / 5) - 15), GUILayout.Height(20)))
			{
				vCamController.SetPrevVirtualCamera();
				EditorUtility.SetDirty(vCamController);
			}
			if (GUILayout.Button("VCam Update", GUILayout.Width(EditorGUIUtility.currentViewWidth * 3 / 5), GUILayout.Height(20)))
			{
				vCamController.AutoVcamsRegister();
				vCamController.RenderLayerResize();
				//EditorUtility.SetDirty(vCamController);
			}
			if (GUILayout.Button(">", GUILayout.Width((EditorGUIUtility.currentViewWidth / 5) - 15), GUILayout.Height(20)))
			{
				vCamController.SetNextVirtualCamera();
				EditorUtility.SetDirty(vCamController);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();


			EditorGUILayout.LabelField("Virtual Cameras", myStyle);
			var vCams = vCamController.virtualCameras;

			EditorGUILayout.BeginHorizontal();
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.wordWrap = true;

			var color = GUI.color;

			for (int index = 0; index < vCams.Length; index++)
			{
				if (index == vCamController.GetCurrentVcamIndex())
				{
					GUI.color = new Color(1f, 0f, 0f);
				}
				else
				{
					GUI.color = color;
				}

				if (index > 4 && index % 5 == 0)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
				}

				if (GUILayout.Button(vCams[index].Name, buttonStyle, GUILayout.MinWidth(80), GUILayout.Height(80)))
				{
					vCamController.SetVcam(index);

				}
			}
			EditorGUILayout.EndHorizontal();
			EditorUtility.SetDirty(vCamController);
			GUI.color = color;


			EditorGUILayout.Separator();
			var VCamLayer = vCamController.vCamRenderLayer;
			LayerListOpen = EditorGUILayout.Foldout(LayerListOpen, "Camera CullingMask");

			if (LayerListOpen)
			{
				for (int index = 0; index < VCamLayer.Length; index++)
				{
					VCamLayer[index] = LayerEditorGUILayout.LayerMaskField(string.Format("{0} CameraLayer", vCams[index].Name), VCamLayer[index]);
					EditorUtility.SetDirty(vCamController);
				}
			}

			EditorGUILayout.Separator();
			EditorGUI.BeginChangeCheck();
			vCamController.useGamePadController = EditorGUILayout.Toggle("Use GamePad ControllCamera", vCamController.useGamePadController);
			if (vCamController.useGamePadController)
			{
				vCamController.inputKey = EditorGUILayout.TextField("input key", vCamController.inputKey);
				vCamController.OnThreashold = EditorGUILayout.FloatField("OnThreashold", vCamController.OnThreashold);
				vCamController.OffThreashold = EditorGUILayout.FloatField("OffThreashold", vCamController.OffThreashold);
			}
			vCamController.useNumPad = EditorGUILayout.Toggle("Use NumPad ControllCamera", vCamController.useNumPad);
			if (vCamController.useNumPad)
			{
				for (int index = 0; index < 10; index++)
				{
					vCamController.Kaypad[index] = (KeyCode)EditorGUILayout.EnumPopup("KeyPad", vCamController.Kaypad[index]);
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(vCamController);
			}
			// base.OnInspectorGUI();
		}
	}

}

public static class LayerEditorGUILayout
{

	// LayerMaskFieldはデフォルトでは無いので自作.
	public static LayerMask LayerMaskField(
		string label,
		LayerMask layerMask)
	{
		List<string> layers = new List<string>();
		List<int> layerNumbers = new List<int>();

		for (var i = 0; i < 32; ++i)
		{
			string layerName = LayerMask.LayerToName(i);
			if (!string.IsNullOrEmpty(layerName))
			{
				layers.Add(layerName);
				layerNumbers.Add(i);
			}
		}

		int maskWithoutEmpty = 0;
		for (var i = 0; i < layerNumbers.Count; ++i)
		{
			if (0 < ((1 << layerNumbers[i]) & layerMask.value))
				maskWithoutEmpty |= 1 << i;
		}

		maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
		int mask = 0;
		for (var i = 0; i < layerNumbers.Count; ++i)
		{
			if (0 < (maskWithoutEmpty & (1 << i)))
				mask |= 1 << layerNumbers[i];
		}
		layerMask.value = mask;

		return layerMask;
	}
}