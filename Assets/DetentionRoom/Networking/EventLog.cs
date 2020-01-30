using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DetentionRoom.Networking
{
    public class EventLog : MonoBehaviour
    {
        private static EventLog _instance;

        public TextMeshProUGUI textPrefab;
        public Transform eventLogContainer;

        public Queue<GameObject> entryQueue = new Queue<GameObject>();
    
        private EventLog() {}
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            InvokeRepeating(nameof(Refresh), 0, 1f);
        }
    
        public static EventLog GetInstance()
        {
            return _instance;
        }


        private void Refresh()
        {
            if (entryQueue.Count == 0)
            {
                return;
            }

            var entry = entryQueue.Dequeue();
            Destroy(entry, 3f);
        }
        
        
        public void AddEntryLog(string t)
        {
            var eventLog = Instantiate(textPrefab, eventLogContainer);
            eventLog.text = t;
            
            entryQueue.Enqueue(eventLog.gameObject);
        }
    }
}