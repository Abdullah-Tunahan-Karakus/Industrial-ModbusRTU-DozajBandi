using System;
using System.Threading.Tasks;

namespace ModbusLibrary.ModbusMaster
{
    public class ModbusMaster
    {
        private SerialPortManager.SerialPortManager _serial;

        public ModbusMaster(SerialPortManager.SerialPortManager serial)
        {
            _serial = serial;
        }

        public bool IsOpen()
        {
            return _serial != null && _serial.IsOpen();
        }

        /// <summary>
        /// ASENKRON OKUMA (Function 03)
        /// Belirtilen adresten itibaren istenen miktarda register okur.
        /// </summary>
        public async Task<byte[]> ReadHoldingRegistersAsync(byte slaveId, ushort start, ushort quantity)
        {
            // Senin kütüphanendeki Function03 sınıfını kullanıyoruz
            byte[] frame = ModbusFrame.Function03.CreateReadHoldingRegistersFrame(slaveId, start, quantity);

            // Canvas dosyasındaki asenkron metodu çağırıyoruz
            return await _serial.SendReceiveAsync(frame);
        }

        /// <summary>
        ///  ASENKRON TEKLİ YAZMA (Function 06)
        /// Belirtilen adrese tek bir ushort değer yazar.
        /// </summary>
        public async Task<byte[]> WriteHoldingRegisterAsync(byte slaveId, ushort start, ushort value)
        {
            byte[] frame = ModbusFrame.Function06.CreateWriteHoldingRegistersFrame(slaveId, start, value);
            return await _serial.SendReceiveAsync(frame);
        }

        /// <summary>
        ///ASENKRON TOPLU YAZMA (Function 16)
        /// Birden fazla register'a dizi halindeki değerleri yazar.
        /// </summary>
        public async Task<byte[]> WriteMultipleRegistersAsync(byte slaveId, ushort start, ushort[] values)
        {
            byte[] frame = ModbusFrame.Function16.CreateWriteMultipleRegistersFrame(slaveId, start, values);
            return await _serial.SendReceiveAsync(frame);
        }
    }
}