using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RWS.Data {
    public class ScriptableObjectIdAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableObjectIdAttribute))]
    public class ScriptableObjectIdDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GUI.enabled = false;
            if (string.IsNullOrEmpty(property.stringValue)) {
                property.stringValue = Guid.NewGuid().ToString();
            }
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif

    [CreateAssetMenu(fileName = "Item", menuName = "RWS/Create Item")]
    public class Item : ScriptableObject, IComparable<Item> {

        public enum Rarity { Normal, Uncommom, Rare, Epic, Legendary }
        public enum ItemType { Common, Key, Weapon, Equipment, Consumable }

        [ScriptableObjectId] private string m_ID;

        [SerializeField] private Sprite m_Icon;
        [SerializeField] private string m_ItemName;
        [SerializeField] private int m_MaxStack = 1;
        [SerializeField] private float m_Weight;
        [SerializeField] private Rarity m_Rarity;
        [SerializeField] private string m_Description;
        [SerializeField] private ItemType m_Type;

        public string GetID() => m_ID;
        public Sprite GetIcon() => m_Icon;
        public virtual string GetName() => m_ItemName;
        public virtual void SetName(string name) { m_ItemName = name; }
        public virtual string GetDescription() => m_Description;
        public int GetMaxStack() => m_MaxStack;
        public float GetWeight() => m_Weight;
        public Rarity GetRarity() => m_Rarity;
        public ItemType MyType() => m_Type;

        public int CompareTo(Item other) {
            if (GetRarity() > other.GetRarity()) {
                return 1;
            }

            if (GetRarity() < other.GetRarity()) {
                return -1;
            }

            return 0;
        }
    }
}
