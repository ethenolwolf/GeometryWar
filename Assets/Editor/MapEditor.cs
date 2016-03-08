using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapGenerator))]
public class MapEditor : Editor {

	public override void OnInspectorGUI() {

		MapGenerator mapGenerator = target as MapGenerator;

		if (DrawDefaultInspector()) {
			mapGenerator.GenerateMap();
		}

		if (GUILayout.Button("Generate Map")) {
			mapGenerator.GenerateMap();
		}
	}
}
