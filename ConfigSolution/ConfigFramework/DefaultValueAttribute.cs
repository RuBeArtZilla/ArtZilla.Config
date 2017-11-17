using System;
using System.Collections.Generic;
using System.Globalization;

namespace ArtZilla.Config {
	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultValueAttribute: Attribute {
		public Object Value { get; }

		public DefaultValueAttribute(Object value)
			=> Value = value;

		public DefaultValueAttribute(Type type, String strValue)
			=> Value = ConvertFromString(type, strValue);

		static Object ConvertFromString(Type type, String strValue)
			=> _converters.TryGetValue(type, out var f)
				? f(strValue)
				: throw new Exception("Can't convert " + strValue + " to instance of type " + type.Name);

		static Dictionary<Type, Func<String, Object>> _converters
			= new Dictionary<Type, Func<String, Object>> {
				[typeof(Byte)] = s => Byte.Parse(s),
				[typeof(SByte)] = s => SByte.Parse(s),
				[typeof(Int16)] = s => Int16.Parse(s),
				[typeof(Int32)] = s => Int32.Parse(s),
				[typeof(Int64)] = s => Int64.Parse(s),
				[typeof(UInt16)] = s => UInt16.Parse(s),
				[typeof(UInt32)] = s => UInt32.Parse(s),
				[typeof(UInt64)] = s => UInt64.Parse(s),

				[typeof(Single)] = s => Single.Parse(s),
				[typeof(Double)] = s => Double.Parse(s),
				[typeof(Decimal)] = s => Decimal.Parse(s, NumberStyles.Number),
				[typeof(Boolean)] = s => Boolean.Parse(s),

				[typeof(String)] = s => s,
			};
	}
}