using System;
using System.IO;

namespace ET
{
    public static class MessageSerializeHelper
    {
        private static MemoryStream GetStream(int count = 0)
        {
            MemoryStream stream;
            if (count > 0)
            {
                stream = new MemoryStream(count);
            }
            else
            {
                stream = new MemoryStream();
            }

            return stream;
        }
        
        /* LCM:
         * ********__。
         * *:ActorID
         * _:opCode
         * +:message 序列化内容，不定长
         */
        public static (ushort, MemoryStream) MessageToStream(object message)
        {
            int headOffset = Packet.ActorIdLength;
            MemoryStream stream = GetStream(headOffset + Packet.OpcodeLength);

            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            //LCM:跳到最后  
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            //LCM:如果指定的值小于流的当前长度，则流将被截断。 如果指定的值大于流的当前长度，则扩展流。 如果流已展开，则不定义新旧长度之间的流的内容。 （意义何在？）
            stream.SetLength(headOffset + Packet.OpcodeLength); 
            //LCM:写入 opCode （不影响流的当前位置）
            stream.GetBuffer().WriteTo(headOffset, opcode);
            //LCM:写入message，之前已经将位置跳到结尾了
            SerializeHelper.Serialize(message, stream);
            //LCM:将流的位置返回最开始
            stream.Seek(0, SeekOrigin.Begin);
            return (opcode, stream);
        }
    }
}