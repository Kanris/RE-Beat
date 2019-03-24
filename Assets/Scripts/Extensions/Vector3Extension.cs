using UnityEngine;

public static class Vector3Extension  {

    public static Vector3 With (this Vector3 original, 
                             float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? original.x, y ?? original.y, z ?? original.z);
    }

    public static Vector3 Subtract(this Vector3 original,
                             float? x = null, float? y = null, float? z = null)
    {
        var newX = x ?? 0f;
        var newY = y ?? 0f;
        var newZ = z ?? 0f;

        return new Vector3(original.x - newX, original.y - newY, original.z - newZ);
    }

    public static Vector3 Add(this Vector3 original,
                         float? x = null, float? y = null, float? z = null)
    {
        var newX = x ?? 0f;
        var newY = y ?? 0f;
        var newZ = z ?? 0f;

        return new Vector3(original.x + newX, original.y + newY, original.z + newZ);
    }

    public static Vector3 Add(this Vector3 original, Vector3 addVector)
    {
        return new Vector3(original.x + addVector.x, original.y + addVector.y, original.z + addVector.z);
    }

}

public static class Vector2Extension
{
    public static Vector2 With(this Vector2 original,
                         float? x = null, float? y = null)
    {
        return new Vector2(x ?? original.x, y ?? original.y);
    }

    public static Vector2 Subtract(this Vector2 original,
                             float? x = null, float? y = null)
    {
        var newX = x ?? 0f;
        var newY = y ?? 0f;

        return new Vector2(original.x - newX, original.y - newY);
    }

    public static Vector2 Add(this Vector2 original,
                         float? x = null, float? y = null)
    {
        var newX = x ?? 0f;
        var newY = y ?? 0f;

        return new Vector2(original.x + newX, original.y + newY);
    }

    public static Vector2 Add(this Vector2 original, Vector2 addVector)
    {
        return new Vector2(original.x + addVector.x, original.y + addVector.y);
    }
}
