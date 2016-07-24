using UnityEngine;
using System.Collections;

public static class FurnitureActions
{
	public static void Door_UpdateAction (Furniture furniture, float deltaTime)
	{
		//Debug.Log ("Door_UpdateAction: " + furniture.furnitureParameters ["openess"]);

		if (furniture.GetParameter (World.is_opening) >= 1) {
			furniture.ChangeParameter (World.openness, deltaTime * 4);

			if (furniture.GetParameter (World.openness) >= 1) {
				furniture.SetParameter (World.is_opening, 0);
			}
		} else {
			if (furniture.GetParameter (World.openness) > 0) {
				furniture.ChangeParameter (World.openness, -(deltaTime * 4));
			}
		}

		furniture.SetParameter (World.openness, Mathf.Clamp01 (furniture.GetParameter (World.openness)));

		if (furniture.onChangedCallback != null) {
			furniture.onChangedCallback (furniture);
		}
	}

	public static Accessiblity Door_IsAccessible (Furniture furniture)
	{
		//Debug.Log ("Door_IsAccessible: " + furniture.furnitureParameters ["is_opening"]);
		furniture.SetParameter (World.is_opening, 1);

		if (furniture.GetParameter (World.openness) >= 1) {
			return Accessiblity.Yes;
		} else {
			return Accessiblity.Soon;
		}
	}
}
