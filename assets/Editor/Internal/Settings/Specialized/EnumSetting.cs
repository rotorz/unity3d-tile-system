// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings.Persisted;
using System;
using System.Linq.Expressions;

namespace Rotorz.Settings.Specialized
{
    internal sealed class EnumSetting<TEnum> : Setting<TEnum>
    {
        #region Utility Functions

        internal static class CastTo<T>
        {
            public static T From<S>(S s)
            {
                return Cache<S>.caster(s);
            }

            internal static class Cache<S>
            {
                internal static readonly Func<S, T> caster = Get();

                internal static Func<S, T> Get()
                {
                    var p = Expression.Parameter(typeof(S), "S");
                    var c = Expression.ConvertChecked(p, typeof(T));
                    return Expression.Lambda<Func<S, T>>(c, p).Compile();
                }
            }
        }

        #endregion


        static EnumSetting()
        {
            if (!typeof(TEnum).IsEnum) {
                throw new InvalidCastException(string.Format("Specified type '{0}' is not an enumeration.", typeof(TEnum).FullName));
            }
        }

        public EnumSetting(DynamicSettingGroup group, string key, TEnum defaultValue, FilterValue<TEnum> filter)
            : base(group, key, defaultValue, filter)
        {
        }


        internal override void Serialize(ISettingSerializer serializer)
        {
            serializer.Serialize(this, CastTo<long>.From<TEnum>(this.Value));
        }

        internal override TEnum Deserialize(ISettingSerializer serializer)
        {
            long persistedValue = serializer.Deserialize(this, CastTo<long>.From<TEnum>(DefaultValue));
            return CastTo<TEnum>.From<long>(persistedValue);
        }


        internal override bool Equals(TEnum lhs, TEnum rhs)
        {
            return CastTo<long>.From<TEnum>(lhs) == CastTo<long>.From<TEnum>(rhs);
        }
    }
}
