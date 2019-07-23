using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    internal static class TestHelper
    {
        public static T CreateInstanced<T>() where T: class
        {
            return FormatterServices.GetSafeUninitializedObject(typeof(T)) as T;
        }

        public static T Set<T>(this T instance, string property, object value) where T : class
        {
            typeof(T).GetProperty(property).SetValue(instance, value);
            return instance;
        }

        internal static T InvokeMember<T>(object  instance, string memberName, params object[] args)
        {
            var type = instance.GetType();
            var result = type.InvokeMember(memberName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, instance, args);
            return (T)result;
        }
    }
}
