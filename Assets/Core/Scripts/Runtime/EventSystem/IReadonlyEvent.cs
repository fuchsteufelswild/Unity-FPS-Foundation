namespace Nexora
{
    /// <summary>
    /// Just an identifier without any functionality. Derive from this class
    /// to define your own event data.
    /// </summary>
    /// <remarks>
    /// Event system works only with <b><see langword="readonly struct"/></b>
    /// for better performance. Make sure derived struct is readonly to avoid
    /// unintentional copies and performance spikes.
    /// </remarks>
    public interface IReadonlyEvent
    {

    }
}