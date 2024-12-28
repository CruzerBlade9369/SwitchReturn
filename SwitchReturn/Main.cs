using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DV;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace SwitchReturn
{
	[EnableReloading]
	public static class Main
	{
		public static bool enabled;
		public static UnityModManager.ModEntry? mod;

		public static Settings settings { get; private set; }

		private static bool Load(UnityModManager.ModEntry modEntry)
		{
			Harmony? harmony = null;

			try
			{
				try
				{
					settings = Settings.Load<Settings>(modEntry);
				}
				catch
				{
					Debug.LogWarning("Unabled to load mod settings. Using defaults instead.");
					settings = new Settings();
				}

				mod = modEntry;

				harmony = new Harmony(modEntry.Info.Id);
				harmony.PatchAll(Assembly.GetExecutingAssembly());
				DebugLog("Attempting patch.");

				modEntry.OnGUI = OnGui;
				modEntry.OnSaveGUI = OnSaveGui;
			}
			catch (Exception ex)
			{
				modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
				harmony?.UnpatchAll(modEntry.Info.Id);
				return false;
			}

			return true;
		}

		static void OnGui(UnityModManager.ModEntry modEntry)
		{
			settings.Draw(modEntry);
		}

		static void OnSaveGui(UnityModManager.ModEntry modEntry)
		{
			settings.Save(modEntry);
		}

		public static void DebugLog(string message)
		{
			if (settings.isLoggingEnabled)
				mod?.Logger.Log(message);
		}
	}

	[HarmonyPatch(typeof(CommsRadioController), "EnableModeUpdate")]
	static class CommsRadioControllerPatch
	{
		static void Postfix(CommsRadioController __instance, bool enableUpdate)
		{
			if (!enableUpdate)
			{
				return;
			}

			var activeModeIndexField = AccessTools.Field(typeof(CommsRadioController), "activeModeIndex");
			if (activeModeIndexField != null)
			{
				activeModeIndexField.SetValue(__instance, 0);
			}

			var allModesField = AccessTools.Field(typeof(CommsRadioController), "allModes");
			if (allModesField != null)
			{
				var allModes = allModesField.GetValue(__instance) as List<ICommsRadioMode>;
				if (allModes != null && allModes.Count > 0)
				{
					// Use the SetMode method to switch to the new mode
					var setModeMethod = AccessTools.Method(typeof(CommsRadioController), "SetMode");
					if (setModeMethod != null)
					{
						setModeMethod.Invoke(__instance, new object[] { allModes[0] });
					}
					else
					{
						Debug.LogError("Failed to access SetMode method.");
					}
				}
			}
		}
	}
}
