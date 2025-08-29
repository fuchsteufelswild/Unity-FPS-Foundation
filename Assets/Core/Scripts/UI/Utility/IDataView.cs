using UnityEngine;

namespace Nexora.UI
{
    /// <summary>
    /// Interface that can be used to make panels that works with special data.
    /// e.g Panel where we fill Player's data.
    /// </summary>
    /// <remarks>
    /// This way we can use our panels without any need for a new class, just filling in the data into fields.
    /// </remarks>
    /// <typeparam name="T">Type of the data.</typeparam>
    public interface IDataView<T>
        where T : class
    {
        /// <summary>
        /// Data to populate this view.
        /// </summary>
        T Data { get; }
        void SetData(T data);
        void ClearData();
        void Refresh();
    }
}