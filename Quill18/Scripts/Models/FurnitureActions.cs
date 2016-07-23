using UnityEngine;
using System.Collections;

public static class FurnitureActions
{
	public static void Door_UpdateAction (Furniture furniture, float deltaTime)
	{
		//Debug.Log ("Door_UpdateAction: " + furniture.furnitureParameters ["openess"]);
		if (furniture.furnitureParameters [World.is_opening] >= 1) {
			furniture.furnitureParameters [World.openess] += deltaTime;

			if (furniture.furnitureParameters [World.openess] >= 1) {
				furniture.furnitureParameters [World.is_opening] = 0;
			}

			furniture.furnitureParameters [World.openess] = Mathf.Clamp01 (furniture.furnitureParameters [World.openess]);

		} else {
			if (furniture.furnitureParameters [World.openess] > 0) {
				furniture.furnitureParameters [World.openess] -= deltaTime;
				furniture.furnitureParameters [World.openess] = Mathf.Clamp01 (furniture.furnitureParameters [World.openess]);
			}
		}
	}

	public static Accessiblity Door_IsAccessible (Furniture furniture)
	{
		//Debug.Log ("Door_IsAccessible: " + furniture.furnitureParameters ["is_opening"]);
		furniture.furnitureParameters [World.is_opening] = 1;

		if (furniture.furnitureParameters [World.openess] >= 1) {
			return Accessiblity.Yes;
		} else {
			return Accessiblity.Soon;
		}
	}
}
