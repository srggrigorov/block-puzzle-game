public interface IPoolableOverride
{
    public void OnCreate();
    public void OnGet();
    public void OnRelease();
    public void OnDelete();
}
