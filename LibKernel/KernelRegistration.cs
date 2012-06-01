namespace LibKernel
{
    public interface KernelRegistration
    {
        ResourceRegistry Routes { get; }
        void AddHook(PostProcessHook hook);
    }
}