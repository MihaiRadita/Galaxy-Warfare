using UnityEngine;

/// <summary>
/// It's a way of adding methods to already exisiting / implemented 
/// classes. The extended methods can be accessed from anywhere within the program.
/// For more information: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
/// Another difference between a simple method and a method from extension is that
/// the extension method will have with one less parameter than a simple method
/// since that we always use this.
/// </summary>
public static class Extension
{
    //when we type 'this' that means the variable which will call that function will be the first parameter of the method.
    //In this case, for example, this extesion method when called by a Vector3 will have no arguments.
    public static Vector2 ToVector2(this Vector3 vect3)
    {
        return new Vector2(vect3.x, vect3.y);
    }

    public static Vector3 ToVector3(this Vector2 vect2)
    {
        return new Vector3(vect2.x, vect2.y, 0);
    }

    public static Vector2 Clamp(this Vector2 vect2, Vector2 minVect2, Vector2 maxVect2)
    {
        return new Vector2(Mathf.Clamp(vect2.x, minVect2.x, maxVect2.x), Mathf.Clamp(vect2.y, minVect2.y, maxVect2.y));
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static Vector2 Remap(this Vector2 vect2, Vector2 from1, Vector2 to1, Vector2 from2, Vector2 to2)
    {
        return new Vector2(vect2.x.Remap(from1.x, to1.x, from2.x, to2.x), vect2.y.Remap(from1.y, to1.y, from2.y, to2.y));
    }
}