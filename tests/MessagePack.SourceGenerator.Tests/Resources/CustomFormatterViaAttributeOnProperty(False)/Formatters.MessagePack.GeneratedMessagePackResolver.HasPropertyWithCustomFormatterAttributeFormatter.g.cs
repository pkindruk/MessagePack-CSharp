﻿// <auto-generated />

#pragma warning disable 618, 612, 414, 168, CS1591, SA1129, SA1309, SA1312, SA1403, SA1649

#pragma warning disable CS8669 // We may leak nullable annotations into generated code.

using MsgPack = global::MessagePack;

namespace MessagePack {
partial class GeneratedMessagePackResolver {

	internal sealed class HasPropertyWithCustomFormatterAttributeFormatter : MsgPack::Formatters.IMessagePackFormatter<global::HasPropertyWithCustomFormatterAttribute>
	{
		private readonly global::UnserializableRecordFormatter __CustomValueCustomFormatter__ = new global::UnserializableRecordFormatter();

		public void Serialize(ref MsgPack::MessagePackWriter writer, global::HasPropertyWithCustomFormatterAttribute value, MsgPack::MessagePackSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNil();
				return;
			}

			writer.WriteArrayHeader(1);
			this.__CustomValueCustomFormatter__.Serialize(ref writer, value.CustomValue, options);
		}

		public global::HasPropertyWithCustomFormatterAttribute Deserialize(ref MsgPack::MessagePackReader reader, MsgPack::MessagePackSerializerOptions options)
		{
			if (reader.TryReadNil())
			{
				return null;
			}

			options.Security.DepthStep(ref reader);
			var length = reader.ReadArrayHeader();
			var ____result = new global::HasPropertyWithCustomFormatterAttribute();

			for (int i = 0; i < length; i++)
			{
				switch (i)
				{
					case 0:
						____result.CustomValue = this.__CustomValueCustomFormatter__.Deserialize(ref reader, options);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			reader.Depth--;
			return ____result;
		}
	}
}
}
