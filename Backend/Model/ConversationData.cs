using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Backend.Model
{
    public class ConversationData
    {
        public SynchronizedCollection<ChatTurn> ConversationHistory { get; set; } = new SynchronizedCollection<ChatTurn>();
        public int TokenCount = 0;
    }
}