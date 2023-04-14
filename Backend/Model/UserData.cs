using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Backend.Model
{
    /// <summary>
    ///     Provider Type
    /// </summary>
    public enum ProviderType
    {
        /// <summary>
        ///     OpenAi Provider
        /// </summary>
        OpenAi = 1,

        /// <summary>
        ///     Azure Provider
        /// </summary>
        Azure = 2
    }

    public class UserData
    {
        public ProviderType ServiceProvider { get; set; }= ProviderType.Azure;
    }
}