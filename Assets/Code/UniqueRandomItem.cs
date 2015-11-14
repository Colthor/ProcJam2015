using UnityEngine;
using System.Collections.Generic;

namespace UniqueRandomItem
{
    public class UniqueRandomItem<T>
    {
        private List<T> m_chooseFrom = new List<T>();

	    public UniqueRandomItem()
	    {
	    }

        public void AddItem(T item)
        {
            m_chooseFrom.Add(item);
        }

        public void AddAll(IEnumerable<T> collection)
        {
            m_chooseFrom.AddRange(collection);
        }

        public T PickItem()
        {
            T item = m_chooseFrom[Random.Range(0, m_chooseFrom.Count)];
            m_chooseFrom.Remove(item);
            return item;
        }
    }

}
