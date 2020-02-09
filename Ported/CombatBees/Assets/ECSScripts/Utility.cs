using Unity.Entities;

public static class Utility
{
    public static EntityQueryDesc CopyEntityQueryDesc(EntityQueryDesc src)
    {
        var dst = new EntityQueryDesc();
        dst.All = new ComponentType[src.All.Length];
        dst.None = new ComponentType[src.None.Length];
        dst.Any = new ComponentType[src.Any.Length];
        src.All.CopyTo(dst.All, 0);
        src.None.CopyTo(dst.None, 0);
        src.Any.CopyTo(dst.Any, 0);
        dst.Options = src.Options;

        return dst;
    }
}
