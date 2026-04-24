using ModbusLibrary.Crc;

namespace ModbusLibrary.ModbusFrame
{
    public class Function16
    {
        public static byte[] CreateWriteMultipleRegistersFrame(byte slaveId, ushort startAddress, ushort[] values)
        {
            ushort quantity = (ushort)values.Length;
            byte byteCount = (byte)(quantity * 2);

            // Boyut formülü: SlaveId(1) + Func(1) + Start(2) + Quantity(2) + ByteCount(1) + Data(byteCount) + CRC(2)
            // Toplam Uzunluk = 9 + byteCount
            byte[] frame = new byte[9 + byteCount];

            frame[0] = slaveId;
            frame[1] = 0x10;
            frame[2] = (byte)(startAddress >> 8);
            frame[3] = (byte)(startAddress & 0xFF);
            frame[4] = (byte)(quantity >> 8);
            frame[5] = (byte)(quantity & 0xFF);
            frame[6] = byteCount;

            int index = 7;
            foreach (var val in values)
            {
                frame[index++] = (byte)(val >> 8);
                frame[index++] = (byte)(val & 0xFF);
            }

            // CRC sadece verilerin olduğu kısma (ilk 'index' kadar) uygulanır
            byte[] dataForCrc = new byte[index];
            System.Array.Copy(frame, 0, dataForCrc, 0, index);

            byte[] crc = CRC_Calculator.CRC(dataForCrc);
            frame[index] = crc[0];
            frame[index + 1] = crc[1];

            return frame;
        }
    }
}



//using System.Collections.Generic;
//using ModbusLibrary.Crc;

//namespace ModbusLibrary.ModbusFrame
//{
//    public class Function16
//    {

//            // 'ushort[] values' kullanarak birden fazla değeri dizi olarak alıyoruz
//            public static byte[] CreateWriteMultipleRegistersFrame(byte slavId, ushort startAddress, ushort[] values)
//            {
//                List<byte> frame = new List<byte>();

//                // 1. Cihaz ID (Slave ID)
//                frame.Add(slavId);

//                // 2. Fonksiyon Kodu (Onluk 16, Hex 0x10)
//                frame.Add(0x10);

//                // 3. Başlangıç Adresi (High Byte ve Low Byte)
//                frame.Add((byte)(startAddress >> 8));
//                frame.Add((byte)(startAddress & 0xFF));

//                // 4. Kayıt Sayısı (Kaç tane kutucuğa yazılacak?)
//                ushort quantity = (ushort)values.Length;
//                frame.Add((byte)(quantity >> 8));
//                frame.Add((byte)(quantity & 0xFF));

//                // 5. Toplam Byte Sayısı (Her veri 2 byte olduğu için: miktar * 2)
//                byte byteCount = (byte)(quantity * 2);
//                frame.Add(byteCount);

//                // 6. Verilerin Pakete Eklenmesi (Döngü ile tüm dizi elemanlarını ekliyoruz)
//                foreach (var val in values)
//                {
//                    frame.Add((byte)(val >> 8));
//                    frame.Add((byte)(val & 0xFF));
//                }

//                // 7. CRC Hesaplama (Senin CRC_Calculator sınıfını kullanıyoruz)
//                byte[] crc = CRC_Calculator.CRC(frame.ToArray());
//                frame.AddRange(crc);

//                return frame.ToArray();
//            }
//        }
//    }




