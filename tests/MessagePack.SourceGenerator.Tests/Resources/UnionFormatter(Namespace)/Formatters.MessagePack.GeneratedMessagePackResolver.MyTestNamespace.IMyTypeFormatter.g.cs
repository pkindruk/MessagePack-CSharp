﻿// <auto-generated />

#pragma warning disable 618, 612, 414, 168, CS1591, SA1129, SA1309, SA1312, SA1403, SA1649

using MsgPack = global::MessagePack;

namespace MessagePack {
partial class GeneratedMessagePackResolver {
internal partial class MyTestNamespace {
	internal sealed class IMyTypeFormatter: MsgPack::Formatters.IMessagePackFormatter<global::MyTestNamespace.IMyType>
	{
		private readonly global::System.Collections.Generic.Dictionary<global::System.RuntimeTypeHandle, global::System.Collections.Generic.KeyValuePair<int, int>> typeToKeyAndJumpMap;
		private readonly global::System.Collections.Generic.Dictionary<int, int> keyToJumpMap;

		public IMyTypeFormatter()
		{
			this.typeToKeyAndJumpMap = new global::System.Collections.Generic.Dictionary<global::System.RuntimeTypeHandle, global::System.Collections.Generic.KeyValuePair<int, int>>(2, MsgPack::Internal.RuntimeTypeHandleEqualityComparer.Default)
			{
				{ typeof(global::MyTestNamespace.Derived1).TypeHandle, new global::System.Collections.Generic.KeyValuePair<int, int>(0, 0) },
				{ typeof(global::MyTestNamespace.Derived2).TypeHandle, new global::System.Collections.Generic.KeyValuePair<int, int>(1, 1) },
			};
			this.keyToJumpMap = new global::System.Collections.Generic.Dictionary<int, int>(2)
			{
				{ 0, 0 },
				{ 1, 1 },
			};
		}

		public void Serialize(ref MsgPack::MessagePackWriter writer, global::MyTestNamespace.IMyType value, MsgPack::MessagePackSerializerOptions options)
		{
			global::System.Collections.Generic.KeyValuePair<int, int> keyValuePair;
			if (value != null && this.typeToKeyAndJumpMap.TryGetValue(value.GetType().TypeHandle, out keyValuePair))
			{
				writer.WriteArrayHeader(2);
				writer.WriteInt32(keyValuePair.Key);
				switch (keyValuePair.Value)
				{
					case 0:
						MsgPack::FormatterResolverExtensions.GetFormatterWithVerify<global::MyTestNamespace.Derived1>(options.Resolver).Serialize(ref writer, (global::MyTestNamespace.Derived1)value, options);
						break;
					case 1:
						MsgPack::FormatterResolverExtensions.GetFormatterWithVerify<global::MyTestNamespace.Derived2>(options.Resolver).Serialize(ref writer, (global::MyTestNamespace.Derived2)value, options);
						break;
					default:
						break;
				}

				return;
			}

			writer.WriteNil();
		}

		public global::MyTestNamespace.IMyType Deserialize(ref MsgPack::MessagePackReader reader, MsgPack::MessagePackSerializerOptions options)
		{
			if (reader.TryReadNil())
			{
				return null;
			}

			if (reader.ReadArrayHeader() != 2)
			{
				throw new global::System.InvalidOperationException("Invalid Union data was detected. Type:global::MyTestNamespace.IMyType");
			}

			options.Security.DepthStep(ref reader);
			var key = reader.ReadInt32();

			if (!this.keyToJumpMap.TryGetValue(key, out key))
			{
				key = -1;
			}

			global::MyTestNamespace.IMyType result = null;
			switch (key)
			{
				case 0:
					result = (global::MyTestNamespace.IMyType)MsgPack::FormatterResolverExtensions.GetFormatterWithVerify<global::MyTestNamespace.Derived1>(options.Resolver).Deserialize(ref reader, options);
					break;
				case 1:
					result = (global::MyTestNamespace.IMyType)MsgPack::FormatterResolverExtensions.GetFormatterWithVerify<global::MyTestNamespace.Derived2>(options.Resolver).Deserialize(ref reader, options);
					break;
				default:
					reader.Skip();
					break;
			}

			reader.Depth--;
			return result;
		}
	}

}
}
}
