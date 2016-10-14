using System.Collections;
using System.Collections.Generic;

namespace Contour.Testing.Plumbing
{
    /// <summary>
    /// Очередь сообщений в брокере.
    /// </summary>
    public class Queue
    {
        /// <summary>
        /// Имя очереди сообщений в брокере.
        /// </summary>
        public string Name { get; set; }

        public Dictionary<string, string> Arguments { get; set; }
    }
}
