using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TraversalPro
{
    public static partial class Utility
    {
        
        internal static string FormatLog(
            string message = "", 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            string fileName = FileName(filePath);
            return $"[{fileName}] [{memberName}]  {message}";
        }

        static string FileName(string filePath)
        {
            // todo ensure this works on Android
            return Path.GetFileNameWithoutExtension(filePath);
        }
        
        internal static bool HasForbiddenComponent<T>(MonoBehaviour owner, bool logError = true)
            where T : Component
        {
            T comp = owner.GetComponent<T>();
            if (comp && logError)
            {
                Debug.LogError($"[{owner.GetType().Name}] The GameObject '{owner.name}' cannot have both {typeof(T).Name} " +
                               $"and {owner.GetType().Name} components on it at the same time.");
            }
            return comp;
        }
        
        internal static void Log(
            string message = "", 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            Debug.Log(FormatLog(message, memberName, filePath));
        }
        
        internal static void LogWarning(
            string message = "", 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            Debug.LogWarning(FormatLog(message, memberName, filePath));
        }
        
        internal static void LogError(
            string message = "", 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            Debug.LogError(FormatLog(message, memberName, filePath));
        }
        
        internal static bool TryValidateRequiredComponent<T>(MonoBehaviour owner, ref T value)
            where T : Component
        {
            if (value && value.gameObject == owner.gameObject) return true;
            value = owner.GetComponent<T>();
            if (value == null)
            {
                Debug.LogError($"[{owner.GetType().Name}] Missing {typeof(T).Name} component on GameObject '{owner.name}'.");
                return false;
            }
            return true;
        }
        
        internal static bool TryValidateRequiredComponent<TInterface>(MonoBehaviour owner, ref InterfaceRef<TInterface> value)
            where TInterface : class
        {
            if (value.Value is Component comp && comp && comp.gameObject == owner.gameObject) return true;
            value.Value = owner.GetComponent<TInterface>();
            if (value.Value == null)
            {
                Debug.LogError($"[{owner.GetType().Name}] Missing {typeof(TInterface).Name} component on GameObject '{owner.name}'.");
                return false;
            }
            return true;
        }

        internal static bool TryValidateRequiredField<T>(MonoBehaviour owner, T value)
            where T : class
        {
            if (value == null)
            {
                Debug.LogError($"[{owner.GetType().Name}] Missing {typeof(T).Name} field on GameObject '{owner.name}'.");
                return false;
            }
            return true;
        }
    }
}