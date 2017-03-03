using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SlackLeadsBidder.Utils
{
    public static class EnumMetadata
    {
        public static EnumMetadata<TEnum>.ValueMetadata Value<TEnum>(TEnum key)
        {
            return EnumMetadata<TEnum>.Value(key);
        }

        public static string ToString<TEnum>(TEnum key)
        {
            return EnumMetadata<TEnum>.ToString(key);
        }
    }

    public static class EnumMetadata<TEnum>
    {
        public sealed class ValueMetadata
        {
            private readonly FieldInfo _f;

            public int Value { get; private set; }

            /// <summary>
            /// Either the System.ComponentModel.DataAnnotations.DisplayAttribute.Name property or the programatically assigned name to the field but where underscore has been replaced with period.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// The System.ComponentModel.DataAnnotations.DisplayAttribute.Description property.
            /// </summary>
            public string Description { get; private set; }

            /// <summary>
            /// The System.ComponentModel.DataAnnotations.DisplayAttribute.Order property.
            /// </summary>
            public int Order { get; private set; }

            /// <summary>
            /// The System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType property.
            /// </summary>
            public Type ResourceType { get; private set; }

            public ValueMetadata(FieldInfo f, int value, string name = null, string description = null, int order = 0, Type resourceType = null)
            {
                _f = f;
                Value = value;
                Name = name ?? f.Name.Replace('_', '.'); // fallback to something user friendlier...
                Description = description;
                Order = order;
                ResourceType = resourceType;
            }

            public T GetCustomAttribute<T>()
                where T : Attribute
            {
                return _f.GetCustomAttribute<T>();
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private static readonly Dictionary<TEnum, ValueMetadata> _map;
        private static readonly ReadOnlyCollection<ValueMetadata> _values;

        static EnumMetadata()
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"{enumType} is not a enum type.", "TEnum");
            }
            var underlyingType = Enum.GetUnderlyingType(enumType);
            if (underlyingType != typeof(int))
            {
                throw new ArgumentException($"{enumType} underlying type is not int.", "TEnum");
            }

            var map = new Dictionary<TEnum, ValueMetadata>();

            foreach (var f in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!f.IsLiteral)
                {
                    continue;
                }

                var key = f.GetValue(null);

                var v = (int)key;

                string name = null;
                Type resourceType = null;
                string description = null;
                int order = 0;

                var display = f.GetCustomAttribute<DisplayAttribute>();
                if (display != null)
                {
                    name = display.Name;
                    description = display.Description;
                    order = display.GetOrder() ?? 0;
                    resourceType = display.ResourceType;
                }

                var valueMetadata = new ValueMetadata(f, v, name: name, description: description, order: order, resourceType: resourceType);
                map.Add((TEnum)key, valueMetadata); // john: this will not work if an enum has assigned the same value more than 1 name (I think it's a good thing not to do).
            }

            _map = map;

            // the sort order of values is stable
            _values = map.Values.OrderBy(x => x.Order).ThenBy(x => x.Value).ToList().AsReadOnly();
        }

        public static ReadOnlyCollection<ValueMetadata> Values
        {
            get
            {
                return _values;
            }
        }

        public static ValueMetadata Value(TEnum key)
        {
            ValueMetadata valueMetadata;
            if (_map.TryGetValue(key, out valueMetadata))
            {
                return valueMetadata;
            }
            return null; // undefined
        }

        public static string ToString(TEnum key)
        {
            return Value(key)?.ToString();
        }
    }
}
