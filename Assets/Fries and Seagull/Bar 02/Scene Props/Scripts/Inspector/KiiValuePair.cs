using UnityEngine;

namespace Seagull.Bar_02.Inspector {
    [System.Serializable]
    public class KiiValuePair { }

    [System.Serializable]
    public class KiiValuePair<K, V> : KiiValuePair { 
        [SerializeField] public K key;
        [SerializeField] public V value;
    }
}