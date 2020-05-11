using System.Collections;
using UnityEngine;

public static class FurnitureActions
{
    public static void Door_UpdateAction(Furniture f, float deltaTime)
    {
        if (f.GetParameter("is_opening") >= 1f)
        {
            f.ChangeParameter("openness", deltaTime * 2f);
            if(f.GetParameter("openness") >= 1)
            {
                f.SetParameter("is_opening", 0);
            }
        }
        else
        {
            f.ChangeParameter("openness", deltaTime * -1.3f);
        }

        f.SetParameter("openness", Mathf.Clamp01(f.GetParameter("openness")));

        if (f.cbOnChanged != null)
        {
            f.cbOnChanged(f);
        }
    }

    public static ENTERABILITY Door_IsEnterable(Furniture f)
    {
        f.SetParameter("is_opening", 1);

        if(f.GetParameter("openness") >= 1)
        {
            return ENTERABILITY.Yes;
        } else
        {
            return ENTERABILITY.Soon;
        }

    }


}
