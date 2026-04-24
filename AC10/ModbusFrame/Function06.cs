using ModbusLibrary.Crc;

namespace ModbusLibrary.ModbusFrame
{
    public class Function06
    {
        public static byte[] CreateWriteHoldingRegistersFrame(byte slaveId, ushort startAddress, ushort value)
        {
            // Liste yerine doğrudan 8 byte'lık sabit bir dizi oluşturuyoruz
            byte[] frame = new byte[8];

            frame[0] = slaveId;
            frame[1] = 0x06;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(value >> 8);
            frame[5] = (byte)(value & 0xFF);

            byte[] dataForCrc = new byte[] { frame[0], frame[1], frame[2], frame[3], frame[4], frame[5] };
            byte[] crc = CRC_Calculator.CRC(dataForCrc);

            frame[6] = crc[0];
            frame[7] = crc[1];

            return frame;
        }
    }
}







//using System.Collections.Generic;
//using ModbusLibrary.Crc;

//namespace ModbusLibrary.ModbusFrame
//{
//    public class Function06
//    {
//        public static byte[] CreateWriteHoldingRegistersFrame(byte slavId,ushort startAddress,ushort values)
//        {
//            List <byte> frame = new List <byte> ();

//            frame.Add((byte) (slavId));
//            frame.Add((byte)(0X06));

//            frame.Add((byte)(startAddress >> 8));
//            frame.Add ((byte)(startAddress & 0XFF));

//            frame.Add((byte)(values >> 8));
//            frame.Add((byte)(values & 0xFF));

//            byte[] crc=CRC_Calculator.CRC(frame.ToArray());
//            frame.AddRange (crc);   
//            return frame.ToArray ();
//        }
//    }
//}
