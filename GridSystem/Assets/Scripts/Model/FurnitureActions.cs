using System.Collections;
using UnityEngine;

public static class FurnitureActions
{
    public static void Door_UpdateAction(Furniture f, float deltaTime)
    {
        if (f.furnitureParameters["is_opening"] >= 1f)
        {
            f.furnitureParameters["openness"] += deltaTime * 2f;
            if(f.furnitureParameters["openness"] >= 1)
            {
                f.furnitureParameters["is_opening"] = 0;
            }
        }
        else
        {
            f.furnitureParameters["openness"] -= deltaTime * 1.3f;
        }

        f.furnitureParameters["openness"] = Mathf.Clamp01(f.furnitureParameters["openness"]);

        if (f.cbOnChanged != null)
        {
            f.cbOnChanged(f);
        }
    }

    public static ENTERABILITY Door_IsEnterable(Furniture f)
    {
        f.furnitureParameters["is_opening"] = 1;

        if(f.furnitureParameters["openness"] >= 1)
        {
            return ENTERABILITY.Yes;
        } else
        {
            return ENTERABILITY.Soon;
        }

    }


}
