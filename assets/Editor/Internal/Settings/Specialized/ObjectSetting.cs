// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using Rotorz.Settings.Persisted;
using System;

namespace Rotorz.Settings.Specialized
{
    internal sealed class ObjectSetting<T> : Setting<T>
    {
        static ObjectSetting()
        {
            if (!typeof(T).IsClass) {
                throw new InvalidCastException(string.Format("Specified type '{0}' is not a class.", typeof(T).FullName));
            }
        }

        public ObjectSetting(DynamicSettingGroup group, string key, T defaultValue, FilterValue<T> filter)
            : base(group, key, defaultValue, filter)
        {
        }


        internal override bool IsObjectStateDirty {
            get {
                var dirtyable = this.Value as IDirtyableObject;
                return dirtyable != null && dirtyable.IsDirty;
            }
        }

        private void MarkDirtyableAsClean()
        {
            var dirtyable = this.Value as IDirtyableObject;
            if (dirtyable != null) {
                dirtyable.MarkClean();
            }
        }


        internal override void Serialize(ISettingSerializer serializer)
        {
            base.Serialize(serializer);
            this.MarkDirtyableAsClean();
        }

        internal override T Deserialize(ISettingSerializer serializer)
        {
            T result = base.Deserialize(serializer);
            this.MarkDirtyableAsClean();
            return result;
        }


        internal override bool Equals(T lhs, T rhs)
        {
            return object.Equals(lhs, rhs);
        }
    }
}
