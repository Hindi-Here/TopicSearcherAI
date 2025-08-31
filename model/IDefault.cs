namespace ThemeBuilder.model
{
    internal interface IDefault<out T> where T : class
    {
        static abstract T SetDefault();
    }
}
