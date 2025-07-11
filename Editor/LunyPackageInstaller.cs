using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace CodeSmileEditor.Luny.Install
{
	internal struct LunyPackageInfo
	{
		public String Name;
		public String GitURL;
	}

	internal static class LunyPackageInstaller
	{
		private static readonly List<LunyPackageInfo> s_RequiredLunyPackages = new()
		{
			new LunyPackageInfo
			{
				Name = "de.codesmile.luacsharp",
				GitURL = "https://github.com/CodeSmile-0000011110110111/de.codesmile.luacsharp.git",
			},
			new LunyPackageInfo
			{
				Name = "de.codesmile.core",
				GitURL = "https://github.com/CodeSmile-0000011110110111/de.codesmile.core.git",
			},
			new LunyPackageInfo
			{
				Name = "de.codesmile.luny",
				GitURL = "https://github.com/CodeSmile-0000011110110111/de.codesmile.luny.git",
			},
		};

		private static ListRequest s_ListRequest;
		private static AddRequest s_AddRequest;

		[InitializeOnLoadMethod]
		private static void OnLoad()
		{
			s_ListRequest = null;
			s_AddRequest = null;
			ListInstalledPackages();
		}

		private static void ListInstalledPackages()
		{
			s_ListRequest = Client.List(true, true);
			EditorApplication.update += OnEditorUpdate;
		}

		private static void OnEditorUpdate()
		{
			if (s_ListRequest != null)
				ProcessListRequest();

			if (s_AddRequest != null)
				ProcessAddRequest();
		}

		private static void ProcessListRequest()
		{
			if (s_ListRequest.IsCompleted)
			{
				EditorApplication.update -= OnEditorUpdate;
				var error = s_ListRequest.Error;
				var status = s_ListRequest.Status;
				var result = s_ListRequest.Result;
				s_ListRequest = null;

				if (status == StatusCode.Success)
					TryAddNextMissingPackage(result);
				else
					Debug.LogWarning($"Luny Installer failed to list packages: {error?.message} ({error?.errorCode})");
			}
		}

		private static void ProcessAddRequest()
		{
			if (s_AddRequest.IsCompleted)
			{
				EditorApplication.update -= OnEditorUpdate;
				var error = s_AddRequest.Error;
				var status = s_AddRequest.Status;
				var result = s_AddRequest.Result;
				s_AddRequest = null;

				if (status == StatusCode.Failure)
					Debug.LogWarning($"Luny Installer failed to add package: {error?.message} ({error?.errorCode})");
			}
		}

		private static void TryAddNextMissingPackage(PackageCollection packages)
		{
			foreach (var requiredPackage in s_RequiredLunyPackages)
			{
				var installedPackage = (from package in packages where package.name == requiredPackage.Name select package)
					.FirstOrDefault();

				if (installedPackage == null)
				{
					Debug.Log($"Adding {requiredPackage.Name} from {requiredPackage.GitURL}");
					s_AddRequest = Client.Add(requiredPackage.GitURL);
					EditorApplication.update += OnEditorUpdate;
					break;
				}
			}
		}
	}
}
