using ModbusLibrary.Crc;

namespace ModbusLibrary.ModbusFrame
{
    public class Function03
    {
        public static byte[] CreateReadHoldingRegistersFrame(byte slaveId, ushort startAddress, ushort quantity)
        {
            // Liste yerine doğrudan 8 byte'lık sabit bir dizi oluşturuyoruz
            byte[] frame = new byte[8];

            frame[0] = slaveId;
            frame[1] = 0x03;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(quantity >> 8);
            frame[5] = (byte)(quantity & 0xFF);

            // Sadece ilk 6 byte'ı CRC hesaplamasına gönderiyoruz
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
//    public class Function03
//    {
//        public static byte[] CreateReadHoldingRegistersFrame(byte slaveId,ushort startAddress,ushort quantity)
//        {   //Dinamiik bir Liste oluşturuluyo eklmesıi ve çıkarması için sınır olmadığı için.
//            List<byte> frame = new List<byte>();
//            //paketin ilk byte her zaman slaveId gösterır
//            //slaveıd haberleşceğimiz alet ıle bağın sağlanması ve habgı slaveden okunması ya ad yazılması gerektıgı anlaşılır
//            frame.Add(slaveId);
//            frame.Add(0X03);//Function Code

//            //Paketin ilk 8 bit sağa kaydırılr MSB alınır
//            frame.Add((byte)(startAddress >> 8));
//            frame.Add((byte)(startAddress & 0xFF));//start adersidnde ise en dusuk bytle LSB ile 

//            //Okuncak maddde bılgıısı 
//            frame.Add((byte)(quantity >> 8));
//            frame.Add((byte)(quantity & 0xFF));

//            byte[] crc = CRC_Calculator.CRC(frame.ToArray()); //CRC Hesaplama
//            frame.AddRange(crc);//crc ıle olustruualan 2 bytlek sınır
//            return frame.ToArray();//byte dizisine dondurulcek
//        }
//    }
//}
