using UnityEditor;
using UnityEngine;

namespace CodeSmileEditor.Luny.Install
{
	public static class LunyPackageInstaller
	{
		[InitializeOnLoadMethod]
		private static void OnLoad() => Debug.Log("OnLoad");
	}
}
