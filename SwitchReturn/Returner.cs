using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using DV;

using UnityEngine;

namespace SwitchReturn
{
	class Returner
	{
		public static List<ICommsRadioMode> allRadioModes = new();

		[HarmonyPatch(typeof(CommsRadioController), "Awake")]
		static class GetAllModes
		{
			static void Postfix(CommsRadioController __instance)
			{
				if (__instance == null)
				{
					Debug.LogError("Trying to access CommsRadioController when it's not available!");
					return;
				}

				FieldInfo allModesFieldInfo = typeof(CommsRadioController).GetField("allModes", BindingFlags.NonPublic | BindingFlags.Instance);
				List<ICommsRadioMode>? allModes = allModesFieldInfo.GetValue(__instance) as List<ICommsRadioMode>;
				if (allModes == null)
				{
					Debug.LogError("allModes is null, this shouldn't happen!");
					return;
				}

				allRadioModes = allModes;
			}
		}

		[HarmonyPatch(typeof(CommsRadioController), "EnableModeUpdate")]
		static class CommsRadioControllerPatch
		{
			static void Postfix(CommsRadioController __instance, bool enableUpdate)
			{
				if (!Main.settings.enableReturn)
				{
					return;
				}

				if (!enableUpdate)
				{
					return;
				}

				if (allRadioModes == null)
				{
					return;
				}

				var setModeMethod = AccessTools.Method(typeof(CommsRadioController), "SetMode");
				if (setModeMethod == null)
				{
					return;
				}

				if (allRadioModes.Count > 0)
				{
					setModeMethod.Invoke(__instance, new object[] { allRadioModes[Main.settings.selectedModeIndex] });
				}
				else
				{
					setModeMethod.Invoke(__instance, new object[] { allRadioModes[0] });
				}
			}
		}
	}
}
