namespace Nexora.SaveSystem
{
    /// <summary>
    /// Presents an interface for object to be saveable, implement this interface if you want a 
    /// <see cref="UnityEngine.Component"/> to be saveable.
    /// Define your own save data and use it for serialization.
    /// </summary>
    public interface ISaveableComponent : IMonoBehaviour
    {
        void Load(object data);
        object Save();
    }
}