using Unity.Entities;

public static class Utility
{
    public static EntityQueryDesc CopyEntityQueryDesc(EntityQueryDesc src)
    {
        var dst = new EntityQueryDesc
        {
            All = new ComponentType[src.All.Length],
            None = new ComponentType[src.None.Length],
            Any = new ComponentType[src.Any.Length],
            Options = src.Options
        };

        src.All.CopyTo(dst.All, 0);
        src.None.CopyTo(dst.None, 0);
        src.Any.CopyTo(dst.Any, 0);

        return dst;
    }
}


public struct EmptyStruct
{ }