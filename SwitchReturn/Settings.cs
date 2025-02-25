using System.Collections.Generic;
using System.Linq;

using UnityModManagerNet;

using UnityEngine;

namespace SwitchReturn
{
	public class Settings : UnityModManager.ModSettings, IDrawable
	{
		public readonly string? version = Main.mod?.Info.Version;
		public int selectedModeIndex = 0;

		[Draw("Enable logging")]
		public bool isLoggingEnabled =
#if DEBUG
			true;
#else
            false;
#endif

		[Draw("Enable switch return")]
		public bool enableReturn = true;

		public override void Save(UnityModManager.ModEntry entry)
		{
			Save(this, entry);
		}

		public void OnChange()
		{

		}

		public void DrawGUI(UnityModManager.ModEntry modEntry)
		{
			this.Draw(modEntry);
			DrawConfigs();
		}

		private string[] GetRadioModesNames()
		{
			List<string> names = new();

			foreach (var mode in Returner.allRadioModes)
			{
				names.Add(mode.ToString());
			}

			return names.ToArray();
		}

		private void DrawConfigs()
		{
			GUILayout.BeginVertical(GUILayout.MinWidth(800), GUILayout.ExpandWidth(false));

			if (Returner.allRadioModes == null || !Returner.allRadioModes.Any())
			{
				GUILayout.Label("Unable to load mode return selector! Defaulting to switch mode.");
			}
			else
			{
				var radioModes = GetRadioModesNames();

				GUILayout.Label("Select radio mode to return to");

				GUILayout.BeginHorizontal(GUILayout.Width(350));
				UnityModManager.UI.PopupToggleGroup(ref selectedModeIndex, radioModes, "All available radio modes");
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}
	}
}
