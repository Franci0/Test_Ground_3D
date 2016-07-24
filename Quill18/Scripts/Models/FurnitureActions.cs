using UnityEngine;
using System.Collections;

public static class FurnitureActions
{
	public static void Door_UpdateAction (Furniture furniture, float deltaTime)
	{
		//Debug.Log ("Door_UpdateAction: " + furniture.furnitureParameters ["openess"]);
		if (furniture.furnitureParameters [World.is_opening] >= 1) {
			furniture.furnitureParameters [World.openness] += deltaTime * 4;

			if (furniture.furnitureParameters [World.openness] >= 1) {
				furniture.furnitureParameters [World.is_opening] = 0;
			}
		} else {
			if (furniture.furnitureParameters [World.openness] > 0) {
				furniture.furnitureParameters [World.openness] -= deltaTime * 4;
			}
		}

		furniture.furnitureParameters [World.openness] = Mathf.Clamp01 (furniture.furnitureParameters [World.openness]);

		if (furniture.onChangedCallback != null) {
			furniture.onChangedCallback (furniture);
		}
	}

	public static Accessiblity Door_IsAccessible (Furniture furniture)
	{
		//Debug.Log ("Door_IsAccessible: " + furniture.furnitureParameters ["is_opening"]);
		furniture.furnitureParameters [World.is_opening] = 1;

		if (furniture.furnitureParameters [World.openness] >= 1) {
			return Accessiblity.Yes;
		} else {
			return Accessiblity.Soon;
		}
	}
}
