namespace MyShortcutGuide.Core;

public static class ListOrder
{
    /// <summary>Insertion is a boundary in the original list, including the boundary after its last item.</summary>
    public static bool Move<T>(List<T> items, int from, int insertion)
    {
        if (from < 0 || from >= items.Count || insertion < 0 || insertion > items.Count) return false;
        var target = insertion > from ? insertion - 1 : insertion;
        if (target == from) return false;
        var item = items[from]; items.RemoveAt(from); items.Insert(target, item);
        return true;
    }
}
