using System;
using UnityEngine;

namespace Runtime.UI
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SelectTypeAttribute : PropertyAttribute
    {
        public System.Type BaseType { get; private set; }

        public SelectTypeAttribute(Type baseType) =>
            BaseType = baseType;
        
    }
}