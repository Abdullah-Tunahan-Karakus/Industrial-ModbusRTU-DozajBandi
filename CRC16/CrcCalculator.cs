namespace ModbusLibrary.Crc
{
    //CRC16 Hata Kontrol Algoritması 
    public class CRC_Calculator
    {
        public static byte[] CRC(byte[] data)
        {
            ushort crc = 0xFFFF;//Başlangıç değer crc kaydedici 1 olcak sekılde ayarlama yeri
            for(int pos=0; pos<data.Length;pos++) // Veri döngüsü pakette ki her byte için işlem yapılır.
            {
                crc^=data[pos];//Mevcut byte ile CRC degerini en düşük byte xor işlemine sokulur
                for(int i =0; i<8;i++) 
                {
                    if((crc & 0x0001) != 0) //crc nın en sağda ki lsb bıtı 1 mı dıye bakılır
                    {
                        crc >>= 1; //1 ise 1 bit sağa kaydırılır
                        crc ^= 0xA001;//CRC ile hata ayıklama ıcın kullanılan işlem XOR uygulanır
                    }
                    else //Mevcut byte 0 ise
                    {
                        crc >>= 1; // 1 bırım saga kaydırılır 
                    }
                }
            }
            //Sonuc HAZIRLAMA
            return new byte[]
            {
                (byte)(crc & 0xFF),// CRC nin düşk byte (low)
                (byte)((crc >> 8)& 0xFF)// yüksek byte (high)

            };
        }
    }
}
