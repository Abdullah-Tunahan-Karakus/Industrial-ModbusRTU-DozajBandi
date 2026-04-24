using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace ModbusLibrary.SerialPortManager
{
    public class SerialPortManager
    {
        public SerialPort _serialPort;

        public SerialPortManager(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

            // Windows tarafındaki fiziksel tamponların (buffer) boyutunu artırmak performansı iyileştirir
            _serialPort.ReadBufferSize = 4096;
            _serialPort.WriteBufferSize = 4096;
        }

        public bool IsOpen() => _serialPort != null && _serialPort.IsOpen;

        public void Open()
        {
            if (!_serialPort.IsOpen) _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();
        }

        /// <summary>
        /// Asenkron ve "Sessizlik Algılamalı" (Silence Detection) Veri Gönderme/Alma Metodu
        /// </summary>
        public async Task<byte[]> SendReceiveAsync(byte[] frame)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    // 1. Porttaki olası eski parazitleri temizle
                    _serialPort.DiscardInBuffer();

                    // 2. Veriyi asenkron olarak gönder
                    await _serialPort.BaseStream.WriteAsync(frame, 0, frame.Length);

                    int maxTimeoutCount = 50; // Maksimum bekleme süresi: 50 * 10ms = 500ms
                    int loopCount = 0;

                    // 3. İLK verinin cihaza ulaşmasını bekle
                    while (_serialPort.BytesToRead == 0 && loopCount < maxTimeoutCount)
                    {
                        await Task.Delay(10);
                        loopCount++;
                    }

                    // Eğer maksimum süre dolduysa ve hiç veri gelmediyse cihaz kapalı demektir
                    if (_serialPort.BytesToRead == 0) return null;

                    // 4. Veri akışı başladı. Paketin TAMAMININ gelmesini bekle
                    int prevBytes = 0;
                    while (loopCount < maxTimeoutCount)
                    {
                        prevBytes = _serialPort.BytesToRead;

                        // 🔥 DÜZELTME: USB tamponlarının yollama gecikmesini (genelde 16ms'dir) tolere 
                        // etmek için 10ms yerine 30ms bekliyoruz. Böylece paket yarıda kesilmiyor.
                        await Task.Delay(30);

                        // Eğer son 30ms içinde yeni veri gelmediyse paket tamamen inmiş demektir
                        if (_serialPort.BytesToRead == prevBytes && prevBytes > 0)
                        {
                            byte[] buffer = new byte[_serialPort.BytesToRead];
                            await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                            return buffer; // Tamamlanmış paketi geri döndür
                        }

                        loopCount += 3; // 30ms beklediğimiz için sayacı 3 artırıyoruz
                    }
                }
            }
            catch
            {
                return null; // Kablo çıkarsa program çökmesin, null dönsün ve yeniden denesin
            }
            return null;
        }
    }
}





//using System;
//using System.IO.Ports;
//using System.Threading.Tasks;

//namespace ModbusLibrary.SerialPortManager
//{
//    public class SerialPortManager
//    {
//        public SerialPort _serialPort;

//        public SerialPortManager(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
//        {
//            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

//            // Zaman aşımı süreleri (Asenkron yapıda 500ms)
//            _serialPort.ReadTimeout = 100;
//            _serialPort.WriteTimeout = 100;
//        }

//        public bool IsOpen() => _serialPort != null && _serialPort.IsOpen;

//        public void Open()
//        {
//            if (!_serialPort.IsOpen) _serialPort.Open();
//        }

//        public void Close()
//        {
//            if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();
//        }

//        /// <summary>

//        /// </summary>
//        public async Task<byte[]> SendReceiveAsync(byte[] frame)
//        {
//            try
//            {
//                if (_serialPort != null && _serialPort.IsOpen)
//                {
//                    // 1. Porttaki eski kalıntıları temizle
//                    _serialPort.DiscardInBuffer();

//                    // 2. Veriyi asenkron gönder (WriteAsync)
//                    await _serialPort.BaseStream.WriteAsync(frame, 0, frame.Length);

//                    // 3. Thread.Sleep YERİNE Task.Delay 
//                    await Task.Delay(100);

//                    // 4. Gelen veri miktarını kontrol et
//                    int bytesToRead = _serialPort.BytesToRead;
//                    if (bytesToRead > 0)
//                    {
//                        byte[] buffer = new byte[bytesToRead];
//                        // 5. Veriyi asenkron oku
//                        await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length);
//                        return buffer;
//                    }
//                }
//            }
//            catch { return null; }
//            return null;
//        }
//    }
//}