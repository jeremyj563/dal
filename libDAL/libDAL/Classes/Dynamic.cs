// Adapted from:
// Creating a dynamic, extensible C# Expando Object
// https://weblog.west-wind.com/posts/2012/feb/08/creating-a-dynamic-extensible-c-expando-object

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace DataRepositories.Classes
{
    public class Dynamic : DynamicObject, ICloneable
    {
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        PropertyInfo[] _InstancePropertyInfo;
        private PropertyInfo[] InstancePropertyInfo
        {
            get
            {
                if (_InstancePropertyInfo == null && Instance != null)
                {
                    var bindingFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    _InstancePropertyInfo = Instance.GetType().GetProperties(bindingFlags);
                }

                return _InstancePropertyInfo;
            }
        }

        private object Instance { get; set; }
        private Type InstanceType { get; set; }

        public Dynamic()
        {
            Initialize(this);
        }

        public Dynamic(object instance)
        {
            Initialize(instance);
        }

        protected virtual void Initialize(object instance)
        {
            this.Instance = instance;
            if (instance != null)
                InstanceType = instance.GetType();
        }

        /// <summary>
        /// Try to retrieve a member by name first from instance properties, followed by the collection entries.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            // First check the Properties collection for member
            if (this.Properties.Keys.Contains(binder.Name))
            {
                result = this.Properties[binder.Name];
                return true;
            }

            // Next check for Public properties via Reflection
            if (this.Instance != null)
            {
                try
                { return GetProperty(this.Instance, binder.Name, out result); }
                catch { /* Silently fail */ }
            }

            // Failed to retrieve a property
            result = null;
            return false;
        }

        /// <summary>
        /// Property setter implementation tries to retrieve value from instance first then into this object
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {

            // first check to see if there's a native property to set
            if (this.Instance != null)
            {
                try
                {
                    bool result = SetProperty(this.Instance, binder.Name, value);
                    if (result)
                    { return true; }
                }
                catch { }
            }

            // no match - set or add to dictionary
            this.Properties[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Dynamic invocation method. Currently allows only for Reflection based operation (no ability to add methods dynamically).
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (this.Instance != null)
            {
                try
                {
                    // check instance passed in for methods to invoke
                    if (InvokeMethod(this.Instance, binder.Name, args, out result))
                    { return true; }
                }
                catch { /* Silently fail */ }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Reflection Helper method to retrieve a property
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="result"></param>
        protected bool GetProperty(object instance, string name, out object result)
        {
            if (instance == null)
            { instance = this; }

            var bindingFlags = (BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
            var memberInfos = InstanceType.GetMember(name, bindingFlags);
            if (memberInfos != null && memberInfos.Length > 0)
            {
                var info = memberInfos[0];
                if (info.MemberType == MemberTypes.Property)
                {
                    result = ((PropertyInfo)info).GetValue(instance, null);
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Reflection helper method to set a property value
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected bool SetProperty(object instance, string name, object value)
        {
            if (instance == null)
            { instance = this; }

            var bindingFlags = (BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
            var memberInfos = InstanceType.GetMember(name, bindingFlags);
            if (memberInfos != null && memberInfos.Length > 0)
            {
                var info = memberInfos[0];
                if (info.MemberType == MemberTypes.Property)
                {
                    ((PropertyInfo)info).SetValue(this.Instance, value, null);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reflection helper method to invoke a method
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        protected bool InvokeMethod(object instance, string name, object[] args, out object result)
        {
            if (instance == null)
            { instance = this; }

            var bindingFlags = (BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);
            var memberInfos = InstanceType.GetMember(name, bindingFlags);
            if (memberInfos != null && memberInfos.Length > 0)
            {
                var info = memberInfos[0] as MethodInfo;
                result = info.Invoke(this.Instance, args);
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Convenience method that provides a string indexer to the properties
        /// collection AND the strongly typed properties of the object by name.
        /// - DYNAMIC: exp["Address"] = "112 nowhere lane"; 
        /// - STRONG: var name = exp["StronglyTypedProperty"] as string; 
        /// </summary>
        /// <remarks>
        /// The getter checks the Properties dictionary first then looks in PropertyInfo for properties.
        /// The setter checks the instance properties before checking the Properties dictionary.
        /// </remarks>
        /// <param name="key"></param>
        public object this[string key]
        {
            get
            {
                try // Try to get from properties collection first
                { return this.Properties[key]; }
                catch (KeyNotFoundException)
                { // Try reflection on instance type
                    object result = null;
                    if (GetProperty(this.Instance, key, out result))
                    { return result; }

                    // Doesn't exist
                    throw;
                }
            }
            set
            {
                if (this.Properties.ContainsKey(key))
                { this.Properties[key] = value; return; }

                // Check instance for existance of type first
                var memberInfos = InstanceType.GetMember(key, BindingFlags.Public | BindingFlags.GetProperty);
                if (memberInfos != null && memberInfos.Length > 0)
                { SetProperty(this.Instance, key, value); }
                else
                { this.Properties[key] = value; }
            }
        }

        /// <summary>
        /// Returns the properties of this instance
        /// </summary>
        /// <param name="includeInstanceProperties"></param>
        public IEnumerable<KeyValuePair<string, object>> GetProperties(bool includeInstanceProperties = false)
        {
            if (includeInstanceProperties && this.Instance != null)
            {
                foreach (var prop in this.InstancePropertyInfo)
                { yield return new KeyValuePair<string, object>(prop.Name, prop.GetValue(this.Instance, null)); }
            }

            foreach (var key in this.Properties.Keys)
            { yield return new KeyValuePair<string, object>(key, this.Properties[key]); }

        }

        /// <summary>
        /// Checks whether a property exists in the property collection or as a property on the instance
        /// </summary>
        /// <param name="item"></param>
        public bool Contains(KeyValuePair<string, object> item, bool includeInstanceProperties = false)
        {
            bool res = Properties.ContainsKey(item.Key);
            if (res)
            { return true; }

            if (includeInstanceProperties && Instance != null)
            {
                foreach (var prop in this.InstancePropertyInfo)
                {
                    if (prop.Name == item.Key)
                    { return true; }
                }
            }

            return false;
        }

        public object Clone()
        {
            return new Dynamic(this);
        }
    }
}
