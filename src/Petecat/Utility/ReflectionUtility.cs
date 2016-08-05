﻿using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Petecat.Utility
{
    public static class ReflectionUtility
    {
        public static bool TryGetType(string typeName, out Type targetType)
        {
            try
            {
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    targetType = type;
                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.LoggerManager.Get().LogEvent(Assembly.GetExecutingAssembly().FullName, Logging.LoggerLevel.Error, string.Format("target type not found. type name={0}", typeName), e);
            }

            targetType = null;
            return false;
        }

        public static T GetInstance<T>(string typeName, params object[] parameters) where T : class
        {
            try
            {
                var targetType = Type.GetType(typeName);
                return Activator.CreateInstance(targetType, parameters) as T;
            }
            catch (Exception e)
            {
                Logging.LoggerManager.Get().LogEvent(Assembly.GetExecutingAssembly().FullName, Logging.LoggerLevel.Error, string.Format("target type not found. type name={0}", typeName), e);
                return null;
            }
        }

        public static bool ContainsCustomAttribute<TAttribute>(MemberInfo memberInfo) where TAttribute : class
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(TAttribute), false);
            return attributes != null && attributes.Length > 0;
        }

        public static TAttribute GetCustomAttribute<TAttribute>(MemberInfo memberInfo) where TAttribute : class
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(TAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes.FirstOrDefault() as TAttribute;
            }
            else
            {
                return null;
            }
        }

        public static bool TryGetCustomAttribute<TAttribute>(MemberInfo memberInfo, Predicate<TAttribute> predicate, out TAttribute attribute) where TAttribute : class
        {
            var attr = GetCustomAttribute<TAttribute>(memberInfo);
            if (attr != null && (predicate == null || predicate(attr)))
            {
                attribute = attr;
                return true;
            }

            attribute = null;
            return false;
        }
    }
}