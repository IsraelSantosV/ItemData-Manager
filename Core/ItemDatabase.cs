using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RWS.Utils;

namespace RWS.Data {
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "RWS/Create Database")]
    public class ItemDatabase : ScriptableObject {

        [SerializeField] private List<Item> m_Items = new List<Item>();
        [Tooltip("Create a number of probabilities compatible with the number of rarities!")]
        [SerializeField] private List<float> m_RarityProbabilities = new List<float>();

        private Dictionary<Item.ItemType, Dictionary<Item.Rarity, List<Item>>> m_SortedItems =
            new Dictionary<Item.ItemType, Dictionary<Item.Rarity, List<Item>>>();

#if UNITY_EDITOR
        private void OnValidate() {
            CreateDatabase();
        }

        private void Awake(){
            CreateDatabase();
        }
#endif

        /// <summary>
        /// Generates dictionaries to store the types of each item 
        /// and sub-dictionaries to store the rarities of the items
        /// </summary>
        public void CreateDatabase() {
            m_SortedItems = new Dictionary<Item.ItemType, Dictionary<Item.Rarity, List<Item>>>();
            var categories = System.Enum.GetValues(typeof(Item.ItemType));
            var rarities = System.Enum.GetValues(typeof(Item.Rarity));

            for (int i = 0; i < categories.Length; i++) {
                m_SortedItems.Add((Item.ItemType)categories.GetValue(i), new Dictionary<Item.Rarity, List<Item>>());
                for (int j = 0; j < rarities.Length; j++) {
                    if (m_SortedItems.TryGetValue((Item.ItemType)categories.GetValue(i), out var m_RarityDictionary)) {
                        m_RarityDictionary.Add((Item.Rarity)rarities.GetValue(j), new List<Item>());
                    }
                }
            }

            foreach (var item in m_Items) {
                if (item == null) continue;
                if (m_SortedItems.TryGetValue(item.MyType(), out var m_RarityDictionary)) {
                    if (m_RarityDictionary.TryGetValue(item.GetRarity(), out var m_ItemList)) {
                        m_ItemList.Add(item);
                    }
                }
            }

            foreach (var m_RarityDictionary in m_SortedItems.Values) {
                foreach (var list in m_RarityDictionary.Values) {
                    list.Sort();
                }
            }
        }

        /// <summary>
        /// Used to check if past probabilities are compatible 
        /// with the number of rarities
        /// </summary>
        /// <returns>valid probability list</returns>
        private List<float> GetValidProbabilities() {
            var m_Probabilities = new List<float>(m_RarityProbabilities);
            var m_RarityAmount = System.Enum.GetValues(typeof(Item.Rarity)).Length;
            if (m_Probabilities.Count > m_RarityAmount) {
                int removeValues = m_Probabilities.Count - m_RarityAmount;
                for (int i = 0; i != removeValues; i++) {
                    m_Probabilities.RemoveAt(m_Probabilities.Count - 1);
                }
            }

            return m_Probabilities;
        }

        public Item GetItem(string m_Name) {
            foreach (var item in m_Items) {
                if (item.GetName() == m_Name) {
                    return item;
                }
            }

            return null;
        }

        public Item GetItem(string m_Name, Item.ItemType m_Type) {
            if (m_SortedItems.TryGetValue(m_Type, out var m_RarityDictionary)) {
                foreach (var m_List in m_RarityDictionary.Values) {
                    foreach (var item in m_List) {
                        if (item.GetName() == m_Name)
                            return item;
                    }
                }
            }

            return null;
        }

        public Item[] GetAllGameItems() {
            return m_Items.ToArray();
        }

        /// <summary>
        /// Randomly get any item from the database
        /// </summary>
        /// <returns></returns>
        public Item GetRandomItem() {
            var typesAmount = System.Enum.GetValues(typeof(Item.ItemType)).Length;
            var tryTimes = typesAmount * 3;

            for (int i = 0; i < tryTimes; i++) {
                var randomIndex = Random.Range(0, typesAmount + 1);
                var randomType = (Item.ItemType)randomIndex;
                var item = GetRandomItem(randomType);
                if(item != null) {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Randomly get an item of the past type
        /// </summary>
        /// <param name="m_Type"></param>
        /// <returns></returns>
        public Item GetRandomItem(Item.ItemType m_Type) {
            if (m_SortedItems.TryGetValue(m_Type, out var m_RarityDictionary)) {
                var m_Probabilities = GetValidProbabilities();
                var m_RarityIndex = RandomFromDistribution.RandomChoiceFollowingDistribution(m_Probabilities);

                if (m_RarityDictionary.TryGetValue((Item.Rarity)m_RarityIndex, out var m_TargetList)) {
                    if (m_TargetList.Count <= 0) return null;
                    return m_TargetList[Random.Range(0, m_TargetList.Count)];
                }
            }

            return null;
        }

        /// <summary>
        /// Randomly get an item of the chosen type and rarity
        /// </summary>
        /// <param name="m_Type"></param>
        /// <param name="m_Rarity"></param>
        /// <returns></returns>
        public Item GetRandomItem(Item.ItemType m_Type, Item.Rarity m_Rarity) {
            if (m_SortedItems.TryGetValue(m_Type, out var m_RarityDictionary)) {
                if (m_RarityDictionary.TryGetValue(m_Rarity, out var m_TargetList)) {
                    if (m_TargetList.Count <= 0) return null;
                    return m_TargetList[Random.Range(0, m_TargetList.Count)];
                }
            }

            return null;
        }

        /// <summary>
        /// Randomly get an item of the chosen rarity
        /// </summary>
        /// <param name="m_Rarity"></param>
        /// <returns></returns>
        public Item GetRandomItem(Item.Rarity m_Rarity) {
            var randomType = Random.Range(0, System.Enum.GetValues(typeof(Item.ItemType)).Length);

            if (m_SortedItems.TryGetValue((Item.ItemType)randomType, out var m_RarityDictionary)) {
                if (m_RarityDictionary.TryGetValue(m_Rarity, out var m_TargetList)) {
                    if (m_TargetList.Count <= 0) return null;
                    return m_TargetList[Random.Range(0, m_TargetList.Count)];
                }
            }

            return null;
        }

        /// <summary>
        /// Randomly get an item from the range of rarities contained in the list
        /// </summary>
        /// <param name="m_Rarities"></param>
        /// <returns></returns>
        public Item GetRandomItem(List<Item.Rarity> m_Rarities) {
            var randomType = Random.Range(0, System.Enum.GetValues(typeof(Item.ItemType)).Length);

            if (m_SortedItems.TryGetValue((Item.ItemType)randomType, out var m_RarityDictionary)) {
                var obteinedItems = new List<Item>();
                for (int i = 0; i < m_Rarities.Count; i++) {
                    if (m_RarityDictionary.TryGetValue(m_Rarities[i], out var m_TargetList)) {
                        if (m_TargetList.Count <= 0) continue;
                        obteinedItems.Add(m_TargetList[Random.Range(0, m_TargetList.Count)]);
                    }
                }

                var m_Probabilities = GetValidProbabilities();
                var m_RarityIndex = RandomFromDistribution.RandomChoiceFollowingDistribution(m_Probabilities);
                if (m_RarityIndex < 0 || m_RarityIndex >= obteinedItems.Count) {
                    if (obteinedItems.Count > 0) {
                        return obteinedItems[0];
                    }

                    return null;
                }

                return obteinedItems[m_RarityIndex];
            }

            return null;
        }

        /// <summary>
        /// Randomly get an item from the range of types and rarities contained in the lists
        /// </summary>
        /// <param name="m_ItemTypes"></param>
        /// <param name="m_Rarities"></param>
        /// <returns></returns>
        public Item GetRandomItem(List<Item.ItemType> m_ItemTypes, List<Item.Rarity> m_Rarities) {
            var sortedItems = new List<Item>();

            for (int j = 0; j < m_ItemTypes.Count; j++) {
                if (m_SortedItems.TryGetValue(m_ItemTypes[j], out var m_RarityDictionary)) {
                    var obteinedItems = new List<Item>();
                    for (int i = 0; i < m_Rarities.Count; i++) {
                        if (m_RarityDictionary.TryGetValue(m_Rarities[i], out var m_TargetList)) {
                            if (m_TargetList.Count <= 0) continue;
                            obteinedItems.Add(m_TargetList[Random.Range(0, m_TargetList.Count)]);
                        }
                    }

                    var m_Probabilities = GetValidProbabilities();
                    var m_RarityIndex = RandomFromDistribution.RandomChoiceFollowingDistribution(m_Probabilities);
                    if (m_RarityIndex < 0 || m_RarityIndex >= obteinedItems.Count) {
                        if (obteinedItems.Count > 0) {
                            sortedItems.Add(obteinedItems[0]);
                        }

                        continue;
                    }

                    sortedItems.Add(obteinedItems[m_RarityIndex]);
                }
            }

            if(sortedItems.Count > 0) {
                return sortedItems[Random.Range(0, sortedItems.Count)];
            }

            return null;
        }

    }
}
