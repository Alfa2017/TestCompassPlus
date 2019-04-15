using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//...
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace Compass.TpTp
{
  // материалы по теме:
  //   форматирование
  //   http://www.csharp-examples.net/string-format-int/

  #region Microsoft AsyncClienr
  // State object for receiving data from remote device.
  public class StateObject
  {
    public Socket workSocket = null;  // Client socket.
    public const int BufferSize = 256;
    public byte[] buffer = new byte[BufferSize];   // Receive buffer.
    public StringBuilder sb = new StringBuilder(); // Received data string.
  }

  public class AsynchronousClient
  {
    private const int port = 11000; // The port number for the remote device.

    // ManualResetEvent instances signal completion.
    private static ManualResetEvent connectDone = new ManualResetEvent(false);
    private static ManualResetEvent sendDone = new ManualResetEvent(false);
    private static ManualResetEvent receiveDone = new ManualResetEvent(false);

    private static String response = String.Empty; // The response from the remote device.

    public static Socket client = null;

    public static void StartClient(string host, int port)
    {
      // Connect to a remote device.
      try {
        // Establish the remote endpoint for the socket.
        // The name of the 
        // remote device is "host.contoso.com".
        //IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
        //IPAddress ipAddress = ipHostInfo.AddressList[0];
        //IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.
        //Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Connect to the remote endpoint.
        //client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
        client.BeginConnect(host, port, new AsyncCallback(ConnectCallback), client);
        connectDone.WaitOne();

        // Send test data to the remote device.
        //Send(client, "This is a test<EOF>");
        //sendDone.WaitOne();

        // Receive the response from the remote device.
        Receive(client);
        //receiveDone.WaitOne();

        // Write the response to the console.
        //Console.WriteLine("Response received : {0}", response);

        // Release the socket.
        //client.Shutdown(SocketShutdown.Both);
        //client.Close();

      }
      catch( Exception e ) {
        Console.WriteLine("ERR(start)", e.ToString());
      }
    }

    public static void Connect(Socket client, string host, int port)
    {
      // Connect to the remote endpoint.
      //client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
      client.Close();
      client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      client.BeginConnect(host, port, new AsyncCallback(ConnectCallback), client);
      connectDone.WaitOne();

      // Receive the response from the remote device.
      Receive(client);
      //receiveDone.WaitOne();
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
      try {
        Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
        client.EndConnect(ar); // Complete the connection.

        Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

        connectDone.Set(); // Signal that the connection has been made.
      }
      catch( Exception e ) {
        Console.WriteLine("ERR(scb)", e.ToString());
      }
    }

    public static void Receive(Socket client)
    {
      receiveDone.Reset();
      try {
        StateObject state = new StateObject();  // Create the state object.
        state.workSocket = client;

        // Begin receiving the data from the remote device.
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
      }
      catch( Exception e ) {
        Console.WriteLine("ERR(recv): {0}", e.ToString());
        receiveDone.Set();// <-----????
      }
      receiveDone.WaitOne();
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
      try {
        // Retrieve the state object and the client socket 
        // from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;

        // Read data from the remote device.
        int bytesRead = client.EndReceive(ar);

        if( bytesRead > 0 ) {
          // There might be more data, so store the data received so far.
          state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

          if( state.buffer[0] == 0x05 ) {
            receiveDone.Set();
            return;
          }

          // Get the rest of the data.
          client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }
        else {
          // All the data has arrived; put it in response.
          if( state.sb.Length > 1 ) {
            response = state.sb.ToString();
          }
          // Signal that all bytes have been received.
          receiveDone.Set();
        }
      }
      catch( Exception e ) {
        Console.WriteLine("ERR(rcb): {0}", e.ToString());
        receiveDone.Set();// <-----????
      }
    }

    //private static void Send(Socket client, String data)
    //{
    //  // Convert the string data to byte data using ASCII encoding.
    //  byte[] byteData = Encoding.ASCII.GetBytes(data);

    //  // Begin sending the data to the remote device.
    //  client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
    //}

    public static void Send(Socket client, byte[] byteData)
    {
      // Convert the string data to byte data using ASCII encoding.
      //byte[] byteData = Encoding.ASCII.GetBytes(data);

      // Begin sending the data to the remote device.
      client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
      sendDone.WaitOne();
    }

    private static void SendCallback(IAsyncResult ar)
    {
      try {
        // Retrieve the socket from the state object.
        Socket client = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.
        int bytesSent = client.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to server.", bytesSent);

        // Signal that all bytes have been sent.
        sendDone.Set();
      }
      catch( Exception e ) {
        Console.WriteLine(e.ToString());
      }
    }

    //public static int Main(String[] args)
    //{
    //  StartClient();
    //  return 0;
    //}
  }
  #endregion
  #region   test TcpClientAsync
  // http://robjdavey.wordpress.com/2011/07/29/improved-asynchronous-tcp-client-example/
  // http://robjdavey.wordpress.com/2011/02/11/asynchronous-tcp-client-example/
  // Represents an asynchronous Tcp Client.
  public class Client
  {
    private const int DefaultClientReadBufferLength = 4096;  // The default length for the read buffer.
    private readonly TcpClient client;  // The tcp client used for the outgoing connection.
    private readonly int port;  // The port to connect to on the remote server.
    //private readonly ManualResetEvent dnsGetHostAddressesResetEvent = null; // A reset event for use if a DNS lookup is required.
    private readonly int clientReadBufferLength; /// The length of the read buffer.
    //private IPAddress[] addresses;  // The addresses to try connection to.
    private string host;
    private int retries; // How many times to retry connection.
    public event EventHandler Connected; // Occurs when the client connects to the server.
    public event EventHandler Disconnected; // Occurs when the client disconnects from the server.
    public event EventHandler<DataReadEventArgs> DataRead; // Occurs when data is read by the client.
    public event EventHandler<DataWrittenEventArgs> DataWritten; // Occurs when data is written by the client.
    public event EventHandler<ExceptionEventArgs> ClientConnectException; // Occurs when an exception is thrown during connection.
    public event EventHandler<ExceptionEventArgs> ClientReadException; // Occurs when an exception is thrown while reading data.
    public event EventHandler<ExceptionEventArgs> ClientWriteException; // Occurs when an exception is thrown while writing data.
    //public event EventHandler<ExceptionEventArgs> DnsGetHostAddressesException; // Occurs when an exception is thrown while performing the DNS lookup.

    private static ManualResetEvent connectDone = new ManualResetEvent(false);
    private static ManualResetEvent sendDone = new ManualResetEvent(false);
    private static ManualResetEvent receiveDone = new ManualResetEvent(false);
 
    // Constructor for a new client object based on a host name or server address string and a port.
    //public Client(string host, int port, int clientReadBufferLength = DefaultClientReadBufferLength) : this(port, clientReadBufferLength)
    //{
    //    //this.dnsGetHostAddressesResetEvent = new ManualResetEvent(false);
    //    //Dns.BeginGetHostAddresses(hostNameOrAddress, this.DnsGetHostAddressesCallback, null);
    //}
 
    // Constructor for a new client object based on a number of IP Addresses and a port.
    public Client(string host, int port, int clientReadBufferLength = DefaultClientReadBufferLength) : this(port, clientReadBufferLength)
    {
        //this.addresses = addresses;
        this.host = host;
    }
 
    // Constructor for a new client object based on a single IP Address and a port.
    //public Client(string host, int port, int clientReadBufferLength = DefaultClientReadBufferLength) : this(host, port, clientReadBufferLength)
    //{
    //}
 
    // Private constructor for a new client object.
    private Client(int port, int clientReadBufferLength)
    {
        this.client = new TcpClient();
        this.port = port;
        this.clientReadBufferLength = clientReadBufferLength;
    }
 
    // Starts an asynchronous connection to the remote server.
    public void Connect()
    {
        //if (this.dnsGetHostAddressesResetEvent != null)
        //    this.dnsGetHostAddressesResetEvent.WaitOne();
        this.retries = 0;
        this.client.BeginConnect(this.host, this.port, this.ClientConnectCallback, null);
        connectDone.WaitOne();
    }
 
    // Writes a string to the server using a given encoding.
    public Guid Write(string value, Encoding encoding)
    {
        byte[] buffer = encoding.GetBytes(value);
        return this.Write(buffer);
    }
 
    // Writes a byte array to the server.
    public Guid Write(byte[] buffer)
    {
        Guid guid = Guid.NewGuid();
        NetworkStream networkStream = this.client.GetStream();
        networkStream.BeginWrite(buffer, 0, buffer.Length, this.ClientWriteCallback, guid);
        return guid;
    }
 
    // Callback from the asynchronous DNS lookup.
    //private void DnsGetHostAddressesCallback(IAsyncResult asyncResult)
    //{
    //    try
    //    {
    //        this.addresses = Dns.EndGetHostAddresses(asyncResult);
    //        this.dnsGetHostAddressesResetEvent.Set();
    //    }
    //    catch (Exception ex)
    //    {
    //        if (this.DnsGetHostAddressesException != null)
    //            this.DnsGetHostAddressesException(this, new ExceptionEventArgs(ex));
    //    }
    //}
 
    // Callback from the asynchronous Connect method.
    private void ClientConnectCallback(IAsyncResult asyncResult)
    {
        try {
          this.client.EndConnect(asyncResult);
          if( this.Connected != null )
            this.Connected(this, new EventArgs());
        }
        catch( Exception ex ) {
          retries++;
          if( retries < 3 ) {
            this.client.BeginConnect(this.host, this.port, this.ClientConnectCallback, null);
          }
          else {
            if (this.ClientConnectException != null)
              this.ClientConnectException(this, new ExceptionEventArgs(ex));
          }
          return;
        }
 
        try {
          NetworkStream networkStream = this.client.GetStream();
          byte[] buffer = new byte[this.clientReadBufferLength];
          networkStream.BeginRead(buffer, 0, buffer.Length, this.ClientReadCallback, buffer);
        }
        catch (Exception ex) {
          if( this.ClientReadException != null )
            this.ClientReadException(this, new ExceptionEventArgs(ex));
        }
    }
 
    // Callback from the asynchronous Read method.
    private void ClientReadCallback(IAsyncResult asyncResult)
    {
      try {
        NetworkStream stream = this.client.GetStream();
        int read = stream.EndRead(asyncResult);
 
        if( read == 0 ) {
          if( this.Disconnected != null )
            this.Disconnected(this, new EventArgs());
        }
 
        byte[] buffer = asyncResult.AsyncState as byte[];
        if( buffer != null ) {
          byte[] data = new byte[read];
          Buffer.BlockCopy(buffer, 0, data, 0, read);
          stream.BeginRead(buffer, 0, buffer.Length, this.ClientReadCallback, buffer);
          if( this.DataRead != null )
            this.DataRead(this, new DataReadEventArgs(data));

          //SGN
          connectDone.Set();
        }
      }
      catch( Exception ex ) {
        if( this.ClientReadException != null )
          this.ClientReadException(this, new ExceptionEventArgs(ex));
        }
    }
 
    // Callback from the asynchronous write callback.
    private void ClientWriteCallback(IAsyncResult asyncResult)
    {
      try {
        NetworkStream stream = this.client.GetStream();
        stream.EndWrite(asyncResult);
        Guid guid = (Guid)asyncResult.AsyncState;
        if( this.DataWritten != null )
          this.DataWritten(this, new DataWrittenEventArgs(guid));
        }
        catch( Exception ex ) {
          if( this.ClientWriteException != null )
            this.ClientWriteException(this, new ExceptionEventArgs(ex));
        }
    }
  }
 
  // Provides data for an exception occuring event.
  public class ExceptionEventArgs : EventArgs
  {
    // Constructor for a new Exception Event Args object.
    public ExceptionEventArgs(Exception ex)
    {
      this.Exception = ex;
    }
 
    public Exception Exception { get; private set; }
  }
 
  // Provides data for a data read event.
  public class DataReadEventArgs : EventArgs
  {
    // Constructor for a new Data Read Event Args object.
    public DataReadEventArgs(byte[] data)
    {
      this.Data = data;
    }
 
    // Gets the data that has been read.
    public byte[] Data { get; private set; }
  }
 
  // Provides data for a data write event.
  public class DataWrittenEventArgs : EventArgs
  {
    // Constructor for a Data Written Event Args object.
    public DataWrittenEventArgs(Guid guid)
    {
      this.Guid = guid;
    }
 
    // Gets the Guid used to match the data written to the confirmation event.
    public Guid Guid { get; private set; }
  }
  #endregion

  #region TpTpClient

  public enum EscType : byte
  {
    SOH = 0x00,
    STX = 0x02,
    ETX = 0x03,
    EOT = 0x04,
    ENQ = 0x05,
    ACK = 0x06,
    СК  = 0x0D,
    NAK = 0x15
  }

  [Flags]
  public enum TpTpStatus : int
  {
    Unknown = 0x000,
    Connected = 0x001,
    Disconnected = 0x002,
    Send = 0x004,
    Read = 0x008,
    Error = 0x010,
    BadLRC = 0x020,
    Timeout = 0x040,
    Confirmed = 0x080,
    Forced = 0x100
  }

  public class TpTpClient
  {
    private bool doWork = true;
    private TcpClient tcpClient = null;
    //private TcpClient tcpClient = new TcpClient();
    //public TcpClient tcpClient = new TcpClient(); //TODO: ВРЕМЕННО!!!!!!!!!!!!!!!!!!!
    private NetworkStream stream = null;
    private TpTpStatus status = TpTpStatus.Disconnected;
    private string host;
    private int port;
    private int failedAttempts;
    private byte transmissionNumber = 0;

    public delegate void LogHandler(string message);
    public LogHandler Logger = null;

    public TpTpClient(string host, int port)
    {
      this.host = host;
      this.port = port;
      this.Encoding = Encoding.GetEncoding(866); //Encoding.Default;
      //this.tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, false); //true
    }

    public void Connect()
    {
      //TpTpStatus sts = TpTpStatus.Connected;
      //sts = TpTpStatus.ReadingError;
      //sts = TpTpStatus.Connected;
      //sts |= TpTpStatus.ReadingError;   // устанавливаем флаг
      //sts &= ~TpTpStatus.ReadingError;  // сбрасываем флаг

      //if( this.stream != null ) {
      //  throw new Exception("соединение уже установлено");
      //}
      //if( this.tcpClient.Connected ) {
      //  this.tcpClient.Client.Disconnect(false);
      //}
      this.tcpClient = new TcpClient(host, port); //this.tcpClient.Connect(host, port);
      this.tcpClient.NoDelay = true;
      this.tcpClient.SendTimeout = this.SendTimeout; // 10000; // ждем 10 сек
      this.tcpClient.ReceiveTimeout = this.ReceiveTimeout; //  5000; // ждем 5 сек
      this.stream = this.tcpClient.GetStream();

      this.status = TpTpStatus.Connected;
      this.status &= ~TpTpStatus.Error;  // сбрасываем флаг
      //this.status &= ~TpTpStatus.Confirmed;

      // устанавливаем соединение в сервером
      int maxCnt = 0;  // максимальное кол-во попыток ожидания готовности сервера
      while( this.doWork ) {
        if( !this.stream.DataAvailable ) {
          Thread.Sleep(50);
          if( maxCnt > 100 ) //SGN: случайное число...
            this.doWork = false;
          ++maxCnt;
        }
        else {
          int esc = this.stream.ReadByte();
          if( esc == (int)EscType.ENQ ) { // <-- сервер подтвердил соединение
            // мы успешно подключились и считали esc символ успешного подтверждения соединения
            //this.status |= TpTpStatus.Confirmed;
            break;
          }
          else {
            throw new Exception("[Err] не обнаружен символ подтверждения соединения (ENQ)");
          }
        }
      }
      failedAttempts = 0; //сбрасываем счетчик ошибок сетевого канала
    }

    public void Disconnect()
    {
      if( this.tcpClient != null ) {
        if( this.stream != null ) {
          if( this.tcpClient.Connected ) {

            this.status = TpTpStatus.Disconnected;
            this.status &= ~TpTpStatus.Error;  // сбрасываем флаг
            //this.status &= ~TpTpStatus.Confirmed;

            // управляющий символ окончания связи
            this.stream.WriteByte((byte)Compass.TpTp.EscType.ACK);
            this.stream.Flush();
            // ждем от сервера подтверждения окончания связи
            int b = this.stream.ReadByte();
            if( b == (int)Compass.TpTp.EscType.EOT ) {
              //this.status |= TpTpStatus.Confirmed;
              if( this.Logger != null ) this.Logger("[Inf] запрос на разрыв соединения подтвержден (EOT)");
              //Console.WriteLine("[Inf] запрос на разрыв соединения подтвержден (EOT)");
            }
            else if( b == (int)Compass.TpTp.EscType.STX ) {
              if( this.Logger != null ) this.Logger(string.Format("[Wrn] получен символ (STX: 0x{0:x2}) начала сообщения, пока игнорируем...", b));

              //--------------------------------
              //Console.WriteLine("УБРАТЬ:добавлено для тестирования проблем с сетью!!!!!!!!!");
              //// читаем ответ от сервера
              //byte[] bs = new byte[4000];
              //int nn = 0;
              //int i = 0;
              //do {
              //  nn = this.stream.ReadByte();
              //  bs[i] = (byte)nn;
              //  ++i;
              //  if( i > bs.Length ) {
              //    throw new Exception("буфер мал для получения сообщения");
              //  }
              //}
              //while( nn != (int)EscType.ETX );
              //int lrc = this.stream.ReadByte();
              ////string s3 = System.Text.Encoding.GetEncoding(866).GetString(bs, 0, i);
              //strins s3 = this.Encoding.GetString(bs, 0, i);
              //Console.WriteLine("{0,-38}:rs=[{1}],lrc=[0x{2:x2}]", "response", s3, lrc);
              //if( this.Logger != null ) this.Logger(string.Format("{0,-38}:rs=[{1}],lrc=[0x{2:x2}]", "response", s3, lrc));
              //nn = this.stream.ReadByte();
              //-----------------------------

            }
            else {
              if( this.Logger != null ) this.Logger(string.Format("[Err] получен неожиданные символ (0x{0:x2}) при попытке разрыва связи", b));
            }
            //-------------
            this.tcpClient.Client.Disconnect(false);
            //this.status |= TpTpStatus.Confirmed;
          }
          else {
            this.status |= TpTpStatus.Forced;
            if( this.Logger != null ) this.Logger(string.Format("[Wrn] соединение уже разорвано"));
            //Console.WriteLine("[Wrn] соединение уже разорвано");
          }
          this.stream.Close();
          this.stream = null;
        }
        this.tcpClient.Close();
        this.tcpClient = null;
      }
    }

    public void Send(Message message)
    {
      //this.status |= TpTpStatus.Send;
      this.status &= ~TpTpStatus.Error;  // сбрасываем флаг
      try {
        byte[] bb = message.Encode(this.Encoding);
        this.stream.Write(bb, 0, bb.Length);
        this.stream.Flush();
      }
      catch( Exception e ) {
        this.status |= TpTpStatus.Error; // устанавливаем флаг
        throw new Exception("err: ошибка при отправке запроса", e);
      }
    }

    public void Read(Message message)
    {
      //this.status |= TpTpStatus.Read;
      this.status &= ~TpTpStatus.Error;  // сбрасываем флаг
      try {
        byte[] bs = new byte[3200]; //2200, 4000

        do {
          int nn = this.stream.ReadByte();
          if( nn == (int)EscType.NAK ) { // <-- сервер вернул ошибку
            //++failedAttempts;
            if( this.Logger != null ) this.Logger(string.Format("[Err] ошибка: сервер вернул NAK... возможно, не верна сумма LRC"));
            if( failedAttempts > 3 ) {
              this.doWork = false;
              throw new Exception("[Err] превышено максимальное кол-во попыток по отправке сообщения [сервер вернул ошибку (NAK)]");
            }
            else {
              //повторно отправляем сообщение
              Thread.Sleep(500);
              this.Send(message);
            }
            //continue;
          }
          else if( nn == (int)EscType.EOT ) {
            // получен символ разрыва связи (перед disconnect'ом от сервера???)
            if( this.Logger != null ) this.Logger(string.Format("[Wrn] получен символ (EOT: 0x{0:x2}) -> неожиданный разрыв связи при получении ответа", nn));
            this.status |= TpTpStatus.Forced;
            this.status &= ~TpTpStatus.Connected;
            this.tcpClient.Close();
            break;
          }
          else if( nn == (int)EscType.STX ) {
            // читаем ответ от сервера
            int i = 0;
            do {
              nn = this.stream.ReadByte();
              //TODO: проверять ли здесь возможность получения esc символа NAK?
              bs[i] = (byte)nn;
              ++i;
              if( i > bs.Length ) throw new Exception("[Err] буфер мал для получения сообщения");
            }
            while( nn != (int)EscType.ETX );

            //проверяем контрольную сумму принятого сообщения
            int lrc = this.stream.ReadByte(); // <- считываем контрольную сумму (LRC) принятого ответа
            nn = 0; // сброс значения в 0
            for( int ii = 0; ii < bs.Length; ++ii ) {
              nn ^= bs[ii];
            }
            if( nn != lrc ) {
              //++failedAttempts;
              if( failedAttempts > 3 ) {
                throw new Exception(string.Format("[Err] обнаружено несовпадение контрольной суммы [rs=0x{0:x2} != callc=0x{1:x2}]", lrc, nn));
              }
              else {
                string s3 = this.Encoding.GetString(bs, 0, i);
                byte tnRq = message.Header.TransmissionNumber;
                byte tnRs = message.HeaderRs.TransmissionNumber;
                string s4 = string.Format("[Err] {0}", message.Header.TransactionCode);
                if( this.Logger != null ) this.Logger(string.Format("{0,-38}:[{1}: rs={2}, cl={3}]", s4, "несовпадение суммы LRC", lrc, nn));

                //сообщаем о несовпадении контрольной суммы и просим сервер повторно переслать сообщение
                Thread.Sleep(500);
                this.stream.WriteByte((byte)EscType.NAK);
                this.stream.Flush();
                //continue;
              }
            }
            else {
              failedAttempts = 0;                 // <-- сбрасываем счетчик сетевых ошибок
              message.Decode(bs, this.Encoding);  // <-- декодируем сообщение

              string s3 = this.Encoding.GetString(bs, 0, i);
              byte tnRq = message.Header.TransmissionNumber;
              byte tnRs = message.HeaderRs.TransmissionNumber;
              //Console.WriteLine("{0,-38}:rq=[{1}]", s4, message.GetFormatedValue());
              //string s4 = string.Format("[Inf] {0}({1}:{2}:{0:x})", message.HeaderRs.TransactionCode, (char)message.HeaderRs.MessageType, (char)message.HeaderRs.MessageSubType);    //ERR: шестнадцатиричное представление
              //string s4 = string.Format("[Inf] {0}({1}:{2}:{3:D2})", message.HeaderRs.TransactionCode, (char)message.HeaderRs.MessageType, (char)message.HeaderRs.MessageSubType, (sbyte)message.HeaderRs.TransactionCode);
              string s4 = string.Format("[Inf] {0}({1}:{2}:{3:D2}:{4}:{5})", message.HeaderRs.TransactionCode, (char)message.HeaderRs.MessageType, (char)message.HeaderRs.MessageSubType, (sbyte)message.HeaderRs.TransactionCode, (char)message.HeaderRs.ProcessingFlag1, (char)message.HeaderRs.ProcessingFlag2);
              //string s4 = string.Format("[Inf] {0}({1}:{2}:{0:D})", message.HeaderRs.TransactionCode, (char)message.HeaderRs.MessageType, (char)message.HeaderRs.MessageSubType);    //ERR: разная длинна
              //string s4 = string.Format("[Inf] {0}({1}:{2}:{0,2:D})", message.HeaderRs.TransactionCode, (char)message.HeaderRs.MessageType, (char)message.HeaderRs.MessageSubType);  //ERR: заполняется не "0", а пробелами
              if( this.Logger != null ) this.Logger(string.Format("{0,-38}:rs=[{1}],lrc=[0x{2:x2}], TrNumRq=[{3}], TrNumRs=[{4}]", s4, s3, lrc, tnRq, tnRs));
              break;
            }
          }

          ++failedAttempts; // если пришли сюда, значит что-то не в порядке...
        }
        while( failedAttempts < 3);   // && ((this.status & TpTpStatus.Connected) == TpTpStatus.Connected) );
      }
      catch( Exception e ) {
        this.status |= TpTpStatus.Error;  // устанавливаем флаг
        throw new Exception("err: ошибка при получении ответа", e);
      }
    }

    public int SendTimeout { get; set; }
    public int ReceiveTimeout { get; set; }
    public Encoding Encoding { get; set; }
    public TpTpStatus Status
    {
      get { return this.status; }
      //private set { this.status = value; }
      set { this.status = value; }  // только для тестирования сетевых ошибок!!!
    }
    public byte TransmissionNumber
    {
      get { return this.transmissionNumber; }
      set {
        if( value > 99 ) this.transmissionNumber = 1;
        else this.transmissionNumber = value;
      }
    }
  }

  #endregion

  #region Messages [ сообщения протокола tptp ]
  // в ответе на Logon приходит поле "h"
  public class LogonRequest : Message
  {
    public LogonRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.LogonRequest)
    {
      //нет дополнительных полей
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
    }
  }

  public class LogoffRequest : Message
  {
    public LogoffRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.LogoffRequest)
    {
      //нет дополнительных полей
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
    }
  }

  public class HandShakeRequest : Message
  {
    public HandShakeRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.HandshakeRequest)
    {
      //нет дополнительных полей
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
    }
  }

  public class DownloadRequest : Message
  {
    public DownloadRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.DownloadRequest)
    {
      //TODO: заменить старый подход к наполнению сообщения на новый (как это сделано здесь)!!!
      //base.Fields.Add(typeof(DownloadKeyField).Name, new DownloadKeyField());
      Add<DownloadKeyField>();
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  // административные :: для сверки финансовых результатов

  public class CloseBatchRequest : Message
  {
    public CloseBatchRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.CloseBatchRequest)
    {
      Add<TotalsBatchField>();
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class CloseShiftRequest : Message
  {
    public CloseShiftRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.CloseShiftRequest)
    {
      Add<TotalsShiftField>();
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class CloseDayRequest : Message
  {
    public CloseDayRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.CloseDayRequest)
    {
      Add<TotalsDayField>();
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }
  
  public class SubtotalsBatchRequest : Message
  {
    public SubtotalsBatchRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.SubtotalsBatchRequest)
    {
      Add<TotalsBatchField>();
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class SubtotalsShiftRequest : Message
  {
    public SubtotalsShiftRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.SubtotalsShiftRequest)
    {
      Add<TotalsShiftField>();
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class SubtotalsDayRequest : Message
  {
    public SubtotalsDayRequest() : base(MessageType.Admin, MessageSubType.OnlineTransaction, TransactionCode.SubtotalsDayRequest)
    {
      Add<TotalsDayField>();
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }
  
  // финансовые транзакции

  public class ReplenishmentRequest : Message
  {
    public ReplenishmentRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.Replenishment)
    {
      Add<InvoiceNumberField>();
      Add<Amount1Field>();
      Add<Track2Field>();

      //формируем необходимые суб поля в составном поле OptionalDataField
      OptionalDataField optdata = new OptionalDataField();
      optdata.Add<StringSubField>(SubFieldType.C);  // <-- валюта транзакции
      Add<OptionalDataField>(optdata);
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class PurchaseRequest : Message
  {
    public PurchaseRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.Purchase)
    {
      this.Add<InvoiceNumberField>();
      //this.Add<Amount1Field>();   //sgn : feb 17, 2014
      this.Add<Track2Field>();

      //формируем необходимые суб поля в составном поле OptionalDataField
      OptionalDataField optdata = new OptionalDataField();
      optdata.Add(Compass.TpTp.SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
      //base.Fields.Add(typeof(OptionalDataField).Name, optdata);
      this.Add<OptionalDataField>(optdata);
    }

    //public void Init(string track, double amount) //SGN: непонятно как реализовывать этот метод при древовидной системе вложенных полей...

    public override void Decode(byte[] data, Encoding encoding)
    {
      // декодируем header и body
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  //public class EMVTransactionRequest : Message
  //{
  //  public EMVTransactionRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.Purchase)
  //  {
  //    ////////формируем необходимые суб поля в составном поле ProductSubFIDsField
  //    //////OptionalDataField optdata = new OptionalDataField();
  //    //////optdata.Add(Compass.TpTp.SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
  //    ////////base.Fields.Add(typeof(OptionalDataField).Name, optdata);
  //    //////this.Add<OptionalDataField>(optdata);

  //    //ProductSubFIDsField proddata = new ProductSubFIDsField();
  //    //proddata.Add(Compass.TpTp.SubFieldf6Type.fO, )
  //    //this.Add<ProductSubFIDsField>(proddata);
  //  }

  //  //public void Init(string track, double amount) //SGN: непонятно как реализовывать этот метод при древовидной системе вложенных полей...

  //  public override void Decode(byte[] data, Encoding encoding)
  //  {
  //    // декодируем header и body
  //    base.DecodeHeader(data, encoding);
  //    base.DecodeBody(data, encoding);
  //  }
  //}

  public class MailOrTelephoneOrderRequest : Message
  {
    public InvoiceNumberField invoice = new InvoiceNumberField();
    public Amount1Field amount = new Amount1Field();
    public Track2Field track = new Track2Field();
    public OptionalDataField optdata = new OptionalDataField();
    public POSConditionCodeField posConditionCode = new POSConditionCodeField();

    public MailOrTelephoneOrderRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.MailOrTelephoneOrder)
    {
      //base.Fields.Add(typeof(InvoiceNumberField).Name, invoice);
      //base.Fields.Add(typeof(Amount1Field).Name, amount);
      //base.Fields.Add(typeof(Track2Field).Name, track);

      ////формируем необходимые суб поля в составном поле OptionalDataField
      //optdata.Add(SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
      //base.Fields.Add(typeof(OptionalDataField).Name, optdata);

      //base.Fields.Add(typeof(POSConditionCodeField).Name, posConditionCode);

      Add<InvoiceNumberField>(invoice);
      Add<Amount1Field>(amount);
      Add<Track2Field>(track);

      //формируем необходимые суб поля в составном поле OptionalDataField
      optdata.Add<StringSubField>(SubFieldType.C);  // <-- валюта транзакции
      Add<OptionalDataField>(optdata);
      Add<POSConditionCodeField>(posConditionCode);
    }

    //public void Init(string track, double amount) //SGN: непонятно как реализовывать этот метод при древовидной системе вложенных полей...

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class MerchandiseReturnRequest : Message
  {
    public InvoiceNumberField invoice = new InvoiceNumberField();
    public Amount1Field amount = new Amount1Field();
    public Track2Field track = new Track2Field();

    public MerchandiseReturnRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.MerchandiseReturn)
    {
      //base.Fields.Add(typeof(InvoiceNumberField).Name, invoice);
      //base.Fields.Add(typeof(Amount1Field).Name, amount);
      //base.Fields.Add(typeof(Track2Field).Name, track);

      Add<InvoiceNumberField>(invoice);
      Add<Amount1Field>(amount);
      Add<Track2Field>(track);

      //формируем необходимые суб поля в составном поле OptionalDataField
      OptionalDataField optdata = new OptionalDataField();
      optdata.Add(Compass.TpTp.SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
      //base.Fields.Add(typeof(OptionalDataField).Name, optdata);
      this.Add<OptionalDataField>(optdata);
    }

    //public void Init(string track, double amount) //SGN: непонятно как реализовывать этот метод при древовидной системе вложенных полей...

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class CardVerificationRequest : Message
  {
    public CardVerificationRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.CardVerification)
    {
      //base.Fields.Add(typeof(InvoiceNumberField).Name, new InvoiceNumberField());
      //base.Fields.Add(typeof(Track2Field).Name, new Track2Field());

      ////формируем необходимые суб поля в составном поле OptionalDataField
      //OptionalDataField optdata = new OptionalDataField();
      //optdata.Add(SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
      //base.Fields.Add(typeof(OptionalDataField).Name, optdata);

      Add<InvoiceNumberField>();
      Add<Track2Field>();

      //формируем необходимые субполя в составном поле OptionalDataField
      OptionalDataField optdata = new OptionalDataField();
      optdata.Add<StringSubField>(SubFieldType.C);  // <-- валюта транзакции
      Add<OptionalDataField>(optdata);
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class BalanceInquiryRequest : Message
  {
    public BalanceInquiryRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.BalanceInquiry)
    {
      //base.Fields.Add(typeof(InvoiceNumberField).Name, new InvoiceNumberField());
      //base.Fields.Add(typeof(Track2Field).Name, new Track2Field());
      ////base.Fields.Add(typeof(EchoDataField).Name, new EchoDataField());

      ////формируем необходимые суб поля в составном поле OptionalDataField
      //OptionalDataField optdata = new OptionalDataField();
      //optdata.Add(SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
      //base.Fields.Add(typeof(OptionalDataField).Name, optdata);

      Add<InvoiceNumberField>();
      Add<Track2Field>();
      Add<EchoDataField>();

      //формируем необходимые субполя в составном поле OptionalDataField
      OptionalDataField optdata = new OptionalDataField();
      //optdata.Add(SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
      optdata.Add<StringSubField>(SubFieldType.C).Value = "840";  // <-- валюта транзакции RUR:810, USD:840, EUR:978
      Add<OptionalDataField>(optdata);
    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
      base.DecodeBody(data, encoding);
    }
  }

  public class PaymentRequest : Message
  {
    public PaymentRequest() : base(MessageType.Finance, MessageSubType.OnlineTransaction, TransactionCode.Payment)
    {
      //base.Fields.Add(typeof(Amount1Field).Name, new Amount1Field());
      //base.Fields.Add(typeof(Track2Field).Name, new Track2Field());

      ////формируем необходимые суб поля в составном поле OptionalDataField
      //OptionalDataField optdata = new OptionalDataField();
      //optdata.Add(SubFieldType.C, new StringSubField(SubFieldType.C));  // <-- валюта транзакции
      //base.Fields.Add(typeof(OptionalDataField).Name, optdata);

      Add<Amount1Field>();
      Add<Track2Field>();

      //формируем необходимые суб поля в составном поле OptionalDataField
      OptionalDataField optdata = new OptionalDataField();
      optdata.Add<StringSubField>(SubFieldType.C);  // <-- валюта транзакции
      Add<OptionalDataField>(optdata);

    }

    public override void Decode(byte[] data, Encoding encoding)
    {
      base.DecodeHeader(data, encoding);
    }
  }
  #endregion Messages +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

  #region Fields [ поля протокола tptp ]
  public class EchoDataField : Field
  {
    private string value;

    public EchoDataField() : base(Compass.TpTp.FieldType.Q)
    {
    }

    protected override string GetValue()
    {
      if( string.IsNullOrEmpty(value) ) return null;
      if( value.Length > 16 ) throw new Exception(string.Format("значение поля ({0}) превысило установленную длинну 16", value));
      return value;
    }

    public override int Size()
    {
      if( string.IsNullOrEmpty(value) ) return 0;
      return value.Length;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value.Trim(); }
    }
  }

  public class DownloadKeyField : Field
  {
    public enum AccessCodeType : sbyte
    {
      Begin = (sbyte)'1',
      Continue = (sbyte)'2',
      Partial = (sbyte)'5'
    }

    private string category = "00";
    private AccessCodeType accessCode;
    private string processingFlag;
    private string filler = "0000000000";

    public DownloadKeyField() : base(Compass.TpTp.FieldType.V)
    {
    }

    protected override string GetValue()
    {
      return string.Format("{0,2}{1,1}{2,-2}{3,10}", category, (char)accessCode, processingFlag, filler);
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      this.category = this.Encoding.GetString(data, pos + 1, 2); //Encoding.GetEncoding(866).GetString(data, pos + 1, 3);
      this.accessCode = (AccessCodeType)data[pos + 1 + 2];
      this.processingFlag = this.Encoding.GetString(data, pos + 1 + 3, 2);
      this.filler = this.Encoding.GetString(data, pos + 1 + 5, 10);
    }

    public override int Size()
    {
      return 15;
    }

    public string Category
    {
      get { return this.category; }
    }

    public AccessCodeType AccessCode
    {
      get { return this.accessCode; }
      set { this.accessCode = value; }
    }

    public string ProcessingFlag
    {
      get { return this.processingFlag; }
      set { this.processingFlag = value; }
    }

    public string Filler
    {
      get { return this.filler; }
      set { this.filler = value; }
    }
  }

  public class Amount1Field : Field
  {
    //private double value;
    private long value;

    public Amount1Field() : base(Compass.TpTp.FieldType.B)
    {
      //this.value = amount;
    }

    protected override string GetValue()
    {
      //return string.Format("{0,18}", value.ToString("F2"));  //TODO: проверить, пока только тест
      //return value.ToString("F2").PadLeft(18, '0');
      return value.ToString().PadLeft(18, '0');
    }

    internal override void Decode(int pos, byte[] data)
    {
      string s = this.Encoding.GetString(data, pos + 1, 18);
      value = Convert.ToInt64(s);
    }

    public override int Size()
    {
      return 18;
    }

    public long Amount
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public class CustomerIDField : Field
  {
    private string value;

    public CustomerIDField() : base(Compass.TpTp.FieldType.N)
    {
    }

    protected override string GetValue()
    {
      if( string.IsNullOrEmpty(value) ) return null;
      if( value.Length > 40 ) throw new Exception(string.Format("значение поля ({0}) превысило установленную длинну 40 символов", value));
      return value;
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      int i = 0;
      do {
        if( data[pos + 1 + i] == Message.FS ) break; // обнаружили разделитель следующего поля
        if( data[pos + 1 + i] == (byte)EscType.ETX ) break; // обнаружили конец сообщения
        ++i;
      }
      while( i < 40 );
      if( i > 0 ) value = this.Encoding.GetString(data, pos + 1, i + 1);
      else value = string.Empty;
    }

    public override int Size()
    {
      if( string.IsNullOrEmpty(value) ) return 0;
      return value.Length;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  //TODO: для составных полей, вроде FID 'a' или FID 'W', сделать специальный базовый класс FieldCollection?
  public class OptionalDataField : Field
  {
    //protected Fields<string, SubField> fields = new Fields<string, SubField>(); //OK
    private Fields<SubFieldType, SubField> fields = new Fields<SubFieldType, SubField>();  //protected
    //protected HashSet<SubField> fields = new HashSet<SubField>();
    //protected const sbyte SFSB = (sbyte)'&'; // Sub Field Separator Beginer
    //protected const sbyte SFSE = (sbyte)'#'; // Sub Field Separator Ender

    public OptionalDataField() : base(Compass.TpTp.FieldType.a)
    {
    }

    public U Get<U>(SubFieldType type) where U : SubField
    {
      SubField f = null;
      if( this.fields.TryGetValue(type, out f) ) {
        return (f as U);
      }
      return null;

      // первый вариант: генерирует исключение при отсутсвии требуемого поля...
      //return (this.Fields[type] as U);
    }

    protected override string GetValue()
    {
      StringBuilder sb = new StringBuilder();
      if( this.fields != null ) {
        foreach( var f in fields ) {
          string val = f.GetValue();
          if( !String.IsNullOrEmpty( f.GetValue()) ) {
            sb.Append(string.Format("{0:D1}{1:D1}{2}{3:D1}", (char)'&', (char)f.SFID, val, (char)'#'));
          }
        }
      }
      if( sb.Length == 0 ) {
        return String.Empty;
      }
      return sb.ToString();
    }

    internal override void Decode(int pos, byte[] data)
    {
      if( data[pos] == (byte)FieldType.a ) {
        SubField sf = null;
        SubFieldType fieldType = SubFieldType.None;
        bool fieldFound = true;
        byte state = 0; // 0 - none, 1 - fieldstart, 2 - fielddecoding, 3 - endofdata, 4 - subfieldstart 5 - subfielddecoding
        for(int i = 1; i < 4000; ++i ) {
          if( (state == 0) && (data[pos + i] == (byte)EscType.ETX) ) {
            // встретили окончание сообщения - выходим из цикла
            state = 3;
            break;
          }
          else if( (state == 0) && (data[pos + i] == Message.FS) ) {
            // встретили начало нового поля (FS <- разделитель полей)
            state = 1;
            break;
          }
          else if( (state == 0) && (data[pos + i] == (byte)'&') ) {
            // встретили разделитель начала субполя
            state = 4;
          }
          else if( (state == 0) && (data[pos + i] == (byte)'#') ) {
            // встретили разделитель конца субполя
            state = 0;
          }
          else if( (state == 4) && (data[pos + i] == (byte)SubFieldType.C) ) {
            fieldType = Compass.TpTp.SubFieldType.C;
            if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
              sf = new StringSubField(SubFieldType.C);
            }
          }
          else if( (state == 4) && (data[pos + i] == (byte)SubFieldType.R) ) {
            fieldType = Compass.TpTp.SubFieldType.R;
            if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
              sf = new StringSubField(SubFieldType.R);
            }
          }
          else if( (state == 4) && (data[pos + i] == (byte)SubFieldType.V) ) {
            fieldType = Compass.TpTp.SubFieldType.V;
            if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
              sf = new StringSubField(SubFieldType.V); // <-- CVV2, 3CSC (3 символа), 4CSC (4 символа) – может указываться в запросе для транзакций с ручным вводом номера карты
            }
          }
          else if( (state == 4) && (data[pos + i] == (byte)SubFieldType.W) ) {
            fieldType = Compass.TpTp.SubFieldType.W;
            if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
              sf = new StringSubField(SubFieldType.W);
            }
          }
          else {
            state = 0;
          }
          if( sf != null ) {
            try {
              sf.Decode(pos + i, data);
              if( !fieldFound ) {
                this.Fields.Add(fieldType, sf); //this.Fields.Add(sf.Name, sf);
              }
            }
            catch( Exception ) {
              Console.WriteLine("[Err] не удалось декодировать субполе ({0})", fieldType);
            }
            i += sf.Size(); //увеличиваем позицию на размер поля
            sf = null; // сброс

            state = 0; // чтобы были готовы считывать следующее поле
          }
        }
      }
      else {
        // ошибка: попытка декодирования "неизвестного" поля
      }
    }

    public override int Size()
    {
      //throw new NotImplementedException();
      int len = 0;
      foreach( var f in fields ) {
        string val = f.GetValue();
        if( !string.IsNullOrEmpty( f.GetValue()) ) {
          len += f.Size();
        }
      }
      return len;
    }

    public void Add(int id, SubFieldType code, SubField subField)
    {
      this.fields.Add(id, code, subField);
    }

    public void Add(SubFieldType code, SubField subField)
    {
      this.fields.Add(code, subField);
    }

    public U Add<U>(SubFieldType code) where U : SubField //, new()
    {
      U subField = Activator.CreateInstance(typeof(U), code) as U;
      //U subField = new U(code);
      this.fields.Add(code, subField);
      return subField;
    }

    public Fields<SubFieldType, SubField> Fields
    {
      get { return fields; }
    }
  }

  // используется, например, при прогрузке терминала
  public class DownloadTextField : Field
  {
    //private Fields<SubFieldType, SubField> fields = new Fields<SubFieldType, SubField>();
    private Fields<string, SubFieldfW> fields = new Fields<string, SubFieldfW>();

    public DownloadTextField() : base(Compass.TpTp.FieldType.W)
    {
      //throw new NotImplementedException();
    }

    protected override string GetValue()
    {
      //throw new NotImplementedException();
      return string.Empty;
    }

    internal override void Decode(int pos, byte[] data)
    {
      if( data[pos] == (byte)FieldType.W ) {
        SubFieldfW sf = null;
        string fieldType = null;
        bool fieldFound = true;
        byte state = 0; // 0 - none, 1 - fieldstart, 2 - fielddecoding, 3 - endofdata, 4 - subfieldstart 5 - subfielddecoding
        for( int i = 2; i < (956 + 1); ++i ) {
          if( state == 0 ) {
            if( data[pos + i] == (byte)EscType.ETX ) {
              // встретили окончание сообщения - выходим из цикла
              state = 3;
              break;
            }
            else if( data[pos + i] == Message.FS ) {
              // встретили начало нового поля (FS <- разделитель полей)
              state = 1;
              break;
            }
            else if( data[pos + i] == SubFieldfW.GS ) {
              // встретили разделитель начала субполя -> Group Separator (GS)
              state = 4;
            }
            else {
              //Console.WriteLine("[Dbg] что-то не так как надо... нужно разобраться... state={0}, data[pos + i]={1}", state, data[pos + i]);
            }
          }
          else if( state == 4 ) {
            string did = this.Encoding.GetString(data, pos + i, 2).TrimEnd();
            ++i; // <- так как did имеет длинну 2 байта (символа), то корректируем указательн i
            if( did == "A" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fA, 0);
              }
            }
            else if( did == "B" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fB, 0);
              }
            }
            else if( did == "C" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fC, 0);
              }
            }
            else if( did == "D" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fD, 0);
              }
            }
            else if( did == "E" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fE, 0);
              }
            }
            else if( did == "F" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fF, 0);
              }
            }
            else if( did == "G" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fG, 0);
              }
            }
            else if( did == "H" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fH, 0);
              }
            }
            else if( did == "a" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fa, 0);
              }
            }
            else if( did == "b" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fb, 0);
              }
            }
            else if( did == "c" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fc, 0);
              }
            }
            else if( did == "f" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.ff, 0);
              }
            }
            else if( did == "g" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fg, 0);
              }
            }
            else if( did == "h" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fh, 0);
              }
            }
            else if( did == "p" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fp, 0);
              }
            }
            else if( did == "q" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fq, 0);
              }
            }
            else if( did == "r" ) {
              fieldType = did;
              if( !(fieldFound = this.Fields.TryGetValue(fieldType, out sf)) ) {
                sf = new StringSubFieldfW(SubFieldfWType.fr, 0);
              }
            }
            else {
              //Console.WriteLine("[Dbg] что-то не так как надо... нужно разобраться... state={0}, did={1}", state, did);
              state = 0; //пока делаем так:
            }
          }
          else {
            state = 0;
          }

          if( sf != null ) {
            try {
              sf.Decode(pos + i, data);
              if( !fieldFound ) {
                this.Fields.Add(fieldType, sf); //this.Fields.Add(sf.Name, sf);
              }
            }
            catch( Exception ) {
              Console.WriteLine("[Err] не удалось декодировать субполе ({0})", fieldType);
            }
            i += sf.Size(); //увеличиваем позицию на размер поля
            sf = null; // сброс

            state = 0; // чтобы были готовы считывать следующее поле
          }
        }
      }
      else {
        Console.WriteLine("[Err] попытка декодирования \"неизвестного\" поля: {0}", (byte)data[pos]);
      }
    }

    public override int Size()
    {
      //throw new NotImplementedException();
      int len = 0;
      foreach( var f in fields ) {
        string val = f.GetValue();
        if( !string.IsNullOrEmpty(f.GetValue()) ) {
          len += f.Size();
        }
      }
      return len;
    }

    public Fields<string, SubFieldfW> Fields
    {
      get { return this.fields; }
    }
  }

  //// используется, например, при EMV-транзакциях или для передачи CVV2 кода
  //public class ProductSubFIDsField : Field
  //{
  //  private Fields<SubFieldf6Type, SubField> fields = new Fields<SubFieldf6Type, SubField>();

  //  public ProductSubFIDsField() : base(Compass.TpTp.FieldType.f6)
  //  {
  //    throw new NotImplementedException();
  //  }

  //  public U Get<U>(SubFieldf6Type type) where U : SubField
  //  {
  //    SubField f = null;
  //    if( this.fields.TryGetValue(type, out f) ) {
  //      return (f as U);
  //    }
  //    return null;

  //    // первый вариант: генерирует исключение при отсутсвии требуемого поля...
  //    //return (this.Fields[type] as U);
  //  }

  //  protected override string GetValue()
  //  {
  //    throw new NotImplementedException();
  //  }

  //  internal override void Decode(int pos, byte[] data)
  //  {
  //    throw new NotImplementedException();
  //  }

  //  public override int Size()
  //  {
  //    throw new NotImplementedException();
  //  }

  //  public void Add(int id, SubFieldf6Type code, SubField subField)
  //  {
  //    this.fields.Add(id, code, subField);
  //  }

  //  public void Add(SubFieldf6Type code, SubField subField)
  //  {
  //    this.fields.Add(code, subField);
  //  }

  //  public U Add<U>(SubFieldf6Type code) where U : SubField //, new()
  //  {
  //    U subField = Activator.CreateInstance(typeof(U), code) as U;
  //    //U subField = new U(code);
  //    this.fields.Add(code, subField);
  //    return subField;
  //  }

  //  public Fields<SubFieldf6Type, SubField> Fields
  //  {
  //    get { return fields; }
  //  }
  //}

  //...........................................................................................

  public class ApprovalCodeField : Field
  {
    private string value;

    public ApprovalCodeField() : base(Compass.TpTp.FieldType.F)
    {
    }

    protected override string GetValue()
    {
      if( value.Length > 8 ) throw new Exception(string.Format("значение поля ({0}) превысило установленную длинну 8", value));
      return value;
    }

    internal override void Decode(int pos, byte[] data)
    {
      value = this.Encoding.GetString(data, pos + 1, 6);  // берем только первые 6-ть символов
    }

    public override int Size()
    {
      return 8;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public class AvailableBalanceField : Field
  {
    private long value;

    public AvailableBalanceField() : base(Compass.TpTp.FieldType.J)
    {
    }

    protected override string GetValue()
    {
      //TODO: проверить
      return value.ToString("D").PadLeft(18);
    }

    internal override void Decode(int pos, byte[] data)
    {
      string ss = this.Encoding.GetString(data, pos + 1, 18); //string ss = Encoding.GetEncoding(866).GetString(data, pos + 1, 18);
      //исключения ловим при декодировании в базовом классе message
      value = Convert.ToInt64(ss);
    }

    public override int Size()
    {
      return 18;
    }

    public long Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public class CardTypeField : Field
  {
    public enum CardTypeType : sbyte
    {
      CreditCard = (sbyte)'C',
      DebitCard = (sbyte)'D'
    }

    private CardTypeType value;

    public CardTypeField() : base(Compass.TpTp.FieldType.R)
    {
      value = CardTypeType.DebitCard;
    }

    protected override string GetValue()
    {
      //TODO: проверить
      return string.Format("{0,1}", (char)value);
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      value = (CardTypeType)data[pos+1];
    }

    public override int Size()
    {
      return 1;
    }

    public CardTypeType Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public class AdditionalReceiptDataField : Field
  {
    private string value;

    public AdditionalReceiptDataField() : base(Compass.TpTp.FieldType.g)
    {
      value = string.Empty;
    }

    protected override string GetValue()
    {
      //TODO: проверить
      return value;
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      int i = 0;
      do {
        if( data[pos + 1 + i] == Message.FS ) break; // обнаружили разделитель следующего поля
        if( data[pos + 1 + i] == (byte)EscType.ETX ) break; // обнаружили конец сообщения
        ++i;
      }
      while( i < 600 );
      if( i > 0 ) value = this.Encoding.GetString(data, pos + 1, i);
      else value = string.Empty;
    }

    public override int Size()
    {
      if( string.IsNullOrEmpty(value) ) return 0;
      else return value.Length;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  // используется для указания ьанку типа счета при списывания денег ( то есть можно по одной и той-же карте, еспользуя это поле, списывать деньги с разных счетов)
  public class ApplicationAccountTypeField : Field
  {
    public enum ApplicationAccountTypeEnum : sbyte
    {
      Default = (sbyte)'0',  // Не установлено (использовать настройки по умолчанию на хосте)
      Checking = (sbyte)'1', 
      Savings = (sbyte)'2', 
      Credit = (sbyte)'4',
      Bonus = (sbyte)'8'
    }

    private ApplicationAccountTypeEnum accountType;

    public ApplicationAccountTypeField() : base(Compass.TpTp.FieldType.D)
    {
    }

    protected override string GetValue()
    {
      return string.Format("{0}", (char)accountType);
    }

    internal override void Decode(int pos, byte[] data)
    {
      accountType = (ApplicationAccountTypeEnum)data[pos + 1];
    }

    public override int Size()
    {
      return 1;
    }

    public ApplicationAccountTypeEnum AccountType
    {
      get { return this.accountType; }
      set { this.accountType = value; }
    }
  }

  public class SequenceNumberField : Field
  {
    public enum SequenceNumberResetFlag : sbyte
    {
      NotReset = (sbyte)'0',
      Reset = (sbyte)'1'
    }

    private ushort shiftNumber;
    private ushort batchNumber;
    private ushort seqNumber;
    private SequenceNumberResetFlag resetFlag;

    public SequenceNumberField() : base(Compass.TpTp.FieldType.h)
    {
      resetFlag = SequenceNumberResetFlag.NotReset;
    }

    protected override string GetValue()
    {
      //TODO: проверить
      return string.Format("{0:D3}{1:D3}{2:D3}{3}", shiftNumber, batchNumber, seqNumber, (char)resetFlag);
    }

    internal override void Decode(int pos, byte[] data)
    {
      //TODO: проверить
      //исключения ловим при декодировании в базовом классе message
      string ss = this.Encoding.GetString(data, pos + 1, 3); //Encoding.GetEncoding(866).GetString(data, pos + 1, 3);
      shiftNumber = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 3, 3); //Encoding.GetEncoding(866).GetString(data, pos + 1 + 3, 3);
      batchNumber = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 6, 3); //Encoding.GetEncoding(866).GetString(data, pos + 1 + 6, 3);
      seqNumber = Convert.ToUInt16(ss);
      resetFlag = (SequenceNumberResetFlag)data[pos + 1 + 9];
    }

    public override int Size()
    {
      return 10;
    }

    public ushort ShiftNumber
    {
      get { return this.shiftNumber; }
      set { this.shiftNumber = value; }
    }

    public ushort BatchNumber
    {
      get { return this.batchNumber; }
      set { this.batchNumber = value; }
    }

    public ushort SeqNumber
    {
      get { return this.seqNumber; }
      set { this.seqNumber = value; }
    }

    public SequenceNumberResetFlag ResetFlag
    {
      get { return this.resetFlag; }
      set { this.resetFlag = value; }
    }
  }

  public class InvoiceNumberField : Field
  {
    private string number;

    public InvoiceNumberField() : base(Compass.TpTp.FieldType.S)
    {
    }

    protected override string GetValue()
    {
      if( string.IsNullOrEmpty(number) ) return null;
      if( number.Length > 16 ) throw new Exception(string.Format("значение поля ({0}) превысило установленную длинну 16", number));
      return number;
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      number = this.Encoding.GetString(data, pos + 1, 16).TrimEnd(); 
    }

    public override int Size()
    {
      return 16;
    }

    public string Number
    {
      get { return this.number; }
      set { this.number = value; }
    }
  }

  public class TransactionIDField : Field
  {
    private string value;

    public TransactionIDField() : base(Compass.TpTp.FieldType.t)
    {
      value = string.Empty;
    }

    protected override string GetValue()
    {
      //return number.PadRight(16);
      if( value.Length > 18 ) throw new Exception(string.Format("значение поля ({0}) превысило установленную длинну 18", value));
      return value;
    }

    internal override void Decode(int pos, byte[] data)
    {
      //TODO: проверить
      //исключения ловим при декодировании в базовом классе message
      int i = 0;
      do {
        if( data[pos + 1 + i] == Message.FS ) break; // обнаружили разделитель следующего поля
        if( data[pos + 1 + i] == (byte)EscType.ETX ) break; // обнаружили конец сообщения
        ++i;
      }
      while( i < 600 );

      if( i > 0 )
        value = this.Encoding.GetString(data, pos + 1, i); //value = Encoding.GetEncoding(866).GetString(data, pos + 1, i);
      else
        value = string.Empty;
    }

    public override int Size()
    {
      if( string.IsNullOrEmpty(value) )
        return 0;
      else
        return value.Length;
    }

    public string Number
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public class Track2Field : Field
  {
    public enum EnterType : sbyte
    {
      Manual = (sbyte)'M',
      CardReader = (sbyte)';',
    }

    private Track2Field.EnterType type;
    private string pan;
    private string data;
    private string expDate; // (DateTime)

    public Track2Field() : base(Compass.TpTp.FieldType.q)
    {
    }

    protected override string GetValue()
    {
      if( type == Track2Field.EnterType.Manual ) {
        if( string.IsNullOrEmpty(pan) && string.IsNullOrEmpty(expDate) ) return null;
        //return string.Format("{0}{1:19}={2:2}", (char)type, pan, exporationDate.ToString("YYMM"));
        //return string.Format("{0}{1,-19}={2,-2}?", (char)type, pan, exporationDate);
        return string.Format("{0}{1}={2,-2}?", (char)type, pan, expDate);
      }
      if( string.IsNullOrEmpty(pan) && string.IsNullOrEmpty(expDate) && string.IsNullOrEmpty(data) ) return null;
      //return string.Format("{0}{1,-19}={2,-2}{3,-13}?", (char)type, pan, exporationDate, data);
      //return string.Format("{0}{1}={2,2}{3}?", (char)type, pan, exporationDate, data);
      return string.Format("{0}{1}={2,2}{3}?", (char)type, pan, expDate, data);
    }

    public override int Size()
    {
      throw new NotImplementedException();
    }

    public Track2Field.EnterType Type
    {
      get { return this.type; }
      set { this.type = value; }
    }
    public string PAN
    {
      get { return this.pan; }
      set { this.pan = value; }
    }
    public string Data
    {
      get { return this.data; }
      set { this.data = value; }
    }
    public string ExpDate //YYMM
    {
      get { return this.expDate; }
      set { this.expDate = value; }
    }
  }

  public class POSConditionCodeField : Field
  {
    private POSConditionCodeType value;

    public enum POSConditionCodeType : byte
    {
      NormalPresentment = 00,
      CustomerNotPresent = 01,
      UnattendedTerminalAbleToRetainCard = 02,
      MerchantSuspicious = 03,
      CustomerPresentOrCardNotPresent = 05,
      MailOrTelephoneOrder = 08,
      CustomerIdentityVerified = 10,
      VerificationOnly = 51,
      RecurringPayment = 52,
      InstallmentPayment = 53,
      Referral = 71,
      HardwareCryptographicCustomerAuthentication = 72,
      SoftwareCryptographicCustomerAuthentication = 73,
      EMVChip = 91
    }

    public POSConditionCodeField() : base(Compass.TpTp.FieldType.e)
    {
    }

    protected override string GetValue()
    {
      return string.Format("{0:D2}", (byte)value);
    }

    public override int Size()
    {
      return 2;
    }

    public POSConditionCodeType Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public class PINField : Field
  {
    private string value;

    public PINField() : base(Compass.TpTp.FieldType.b)
    {
    }

    protected override string GetValue()
    {
      if( string.IsNullOrEmpty(this.value) ) return null;
      if( this.value.Length > 16 ) throw new Exception(string.Format("значение поля ({0}) превысило установленную длинну 16", this.value));
      return this.value;
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      throw new NotImplementedException();
    }

    public override int Size()
    {
      //if( string.IsNullOrEmpty(value) )
      //  return 0;
      //else
      //  return value.Length;

      //фиксированная длинна
      return 16;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public class AuthenticationField : Field
  {
    private string value;

    public AuthenticationField() : base(Compass.TpTp.FieldType.G)
    {
    }

    protected override string GetValue()
    {
      if( string.IsNullOrEmpty(this.value) )
        return null;
      if( this.value.Length > 8 )
        throw new Exception(string.Format("значение поля ({0}) превысило установленную длинну 8", this.value));
      return this.value;
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      throw new NotImplementedException();
    }

    public override int Size()
    {
      //фиксированная длинна
      return 8;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  //..[ сверочные поля, испоьзуются при закрытии периодов или получении отчетов ]................
  
  // итоги по пакету
  public class TotalsBatchField : Field
  {
    private ushort shiftNumber;  // номер смены  (3)
    private ushort batchNumber;  // номер пакета (3)
    private ushort val3;  // Количество дебетных транзакций в пакете (4)
    private long val4;   // Сумма дебетных транзакций в пакете (19)
    private ushort val5;  // Количество кредитных транзакций в пакете (4)
    private long val6;   // Сумма кредитных транзакций в пакете (19)
    private ushort val7;  // Количество транзакций корректировок в пакете (4)
    private long val8;   // Сумма транзакций корректировок в пакете (19)

    public TotalsBatchField() : base(Compass.TpTp.FieldType.l)
    {
    }

    protected override string GetValue()
    {
      //return string.Format("{0:000}{1:000}{2:0000}{3:0000000000000000000}{4:0000}{5:0000000000000000000}{6:0000}{7:0000000000000000000}", shiftNumber, batchNumber, val3, val4, val5, val6, val7, val8);
      //return string.Format("{0:000}{1:000}{2:0000}{3:0000000000000000000}{4:0000}{5:0000000000000000000}{6:0000}{7:0000000000000000000}", shiftNumber, batchNumber, val3, val4, val5, val6, val7, val8);
      return string.Format("{0:000}{1:000}{2:0000}{3:+000000000000000000;-000000000000000000}{4:0000}{5:+000000000000000000;-000000000000000000}{6:0000}{7:+000000000000000000;-000000000000000000}", shiftNumber, batchNumber, val3, val4, val5, val6, val7, val8);
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      string ss = string.Empty;
      ss = this.Encoding.GetString(data, pos + 1, 3);
      shiftNumber = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 +  3, 3);
      batchNumber = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 +  6, 4);
      val3 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 10, 19);
      val4 = Convert.ToInt64(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 29, 4);
      val5 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 33, 19);
      val6 = Convert.ToInt64(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 52, 4);
      val7 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 56, 19);
      val8 = Convert.ToInt64(ss);
    }

    public override int Size()
    {
      //фиксированная длинна
      return 75;
    }

    public ushort ShiftNumber
    {
      get { return shiftNumber; }
      set {
        if( value > 999 ) {
          shiftNumber = 999;
        }
        else if( value == 000 ) {
          shiftNumber = 1;
        }
        else {
          shiftNumber = value;
        }
      }
    }

    public ushort BatchNumber
    {
      get { return batchNumber; }
      set { batchNumber = value; }
    }

    public ushort DebetTransactionCount // в пакете
    {
      get { return val3; }
      set { val3 = value; }
    }

    public long DebetTransactionSum
    {
      get { return val4; }
      set { val4 = value; }
    }

    public ushort CreditTransactionCount
    {
      get { return val5; }
      set { val5 = value; }
    }

    public long CreditTransactionSum
    {
      get { return val6; }
      set { val6 = value; }
    }

    public ushort CorrectionTransactionCount
    {
      get { return val7; }
      set { val7 = value; }
    }

    public long CorrectionTransactionSum
    {
      get { return val8; }
      set { val8 = value; }
    }
  }
  // итоги за день
  public class TotalsShiftField : Field
  {
    private ushort shiftCount;  // Количество смен за день (3)
    private ushort batchCount;  // Количество пакетов за день (3)
    private ushort val3;  // Количество дебетных транзакций за смену (4)
    private long val4;   // Сумма дебетных транзакций за смену (19)
    private ushort val5;  // Количество кредитных транзакций за смену (4)
    private long val6;   // Сумма кредитных транзакций за смену (19)
    private ushort val7;  // Количество транзакций корректировок за смену (4)
    private long val8;   // Сумма транзакций корректировок за смену (19)

    public TotalsShiftField() : base(Compass.TpTp.FieldType.o)
    {
    }

    protected override string GetValue()
    {
      //return string.Format("{0:000}{1:000}{2:0000}{3:0000000000000000000}{4:0000}{5:0000000000000000000}{6:0000}{7:0000000000000000000}", shiftNumber, batchNumber, val3, val4, val5, val6, val7, val8);
      return string.Format("{0:000}{1:000}{2:0000}{3:+000000000000000000;-000000000000000000}{4:0000}{5:+000000000000000000;-000000000000000000}{6:0000}{7:+000000000000000000;-000000000000000000}", shiftCount, batchCount, val3, val4, val5, val6, val7, val8);
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      string ss = string.Empty;
      ss = this.Encoding.GetString(data, pos + 1, 3);
      shiftCount = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 3, 3);
      batchCount = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 6, 4);
      val3 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 10, 19);
      val4 = Convert.ToInt64(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 29, 4);
      val5 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 33, 19);
      val6 = Convert.ToInt64(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 52, 4);
      val7 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 56, 19);
      val8 = Convert.ToInt64(ss);
    }

    public override int Size()
    {
      //фиксированная длинна
      return 75;
    }

    public ushort ShiftCount // кол-во смен за день
    {
      get { return shiftCount; }
      set { shiftCount = value; }
    }

    public ushort BatchCount // кол-во пакетов за день
    {
      get { return batchCount; }
      set { batchCount = value; }
    }

    public ushort DebetTransactionCount // за смену
    {
      get { return val3; }
      set { val3 = value; }
    }

    public long DebetTransactionSum
    {
      get { return val4; }
      set { val4 = value; }
    }

    public ushort CreditTransactionCount
    {
      get { return val5; }
      set { val5 = value; }
    }

    public long CreditTransactionSum
    {
      get { return val6; }
      set { val6 = value; }
    }

    public ushort CorrectionTransactionCount
    {
      get { return val7; }
      set { val7 = value; }
    }

    public long CorrectionTransactionSum
    {
      get { return val8; }
      set { val8 = value; }
    }
  }
  // итоги за день
  public class TotalsDayField : Field
  {
    private ushort shiftCount;  // номер смены  (3)
    private ushort batchCount;  // номер пакета (3)
    private ushort val3;  // Количество дебетных транзакций за день (4)
    private long val4;   // Сумма дебетных транзакций за день (19)
    private ushort val5;  // Количество кредитных транзакций за день (4)
    private long val6;   // Сумма кредитных транзакций за день (19)
    private ushort val7;  // Количество транзакций корректировок за день (4)
    private long val8;   // Сумма транзакций корректировок за день (19)

    public TotalsDayField() : base(Compass.TpTp.FieldType.m)
    {
    }

    protected override string GetValue()
    {
      //return string.Format("{0:000}{1:000}{2:0000}{3:0000000000000000000}{4:0000}{5:0000000000000000000}{6:0000}{7:0000000000000000000}", shiftNumber, batchNumber, val3, val4, val5, val6, val7, val8);
      return string.Format("{0:000}{1:000}{2:0000}{3:+000000000000000000;-000000000000000000}{4:0000}{5:+000000000000000000;-000000000000000000}{6:0000}{7:+000000000000000000;-000000000000000000}", shiftCount, batchCount, val3, val4, val5, val6, val7, val8);
    }

    internal override void Decode(int pos, byte[] data)
    {
      //исключения ловим при декодировании в базовом классе message
      string ss = string.Empty;
      ss = this.Encoding.GetString(data, pos + 1, 3);
      shiftCount = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 +  3, 3);
      batchCount = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 +  6, 4);
      val3 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 10, 19);
      val4 = Convert.ToInt64(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 29, 4);
      val5 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 33, 19);
      val6 = Convert.ToInt64(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 52, 4);
      val7 = Convert.ToUInt16(ss);
      ss = this.Encoding.GetString(data, pos + 1 + 56, 19);
      val8 = Convert.ToInt64(ss);
    }

    public override int Size()
    {
      //фиксированная длинна
      return 75;
    }

    public ushort ShiftCount // кол-во смен за день
    {
      get { return shiftCount; }
      set { shiftCount = value; }
    }

    public ushort BatchCount // кол-во пакетов за день
    {
      get { return batchCount; }
      set { batchCount = value; }
    }

    public ushort DebetTransactionCount // за день
    {
      get { return val3; }
      set { val3 = value; }
    }

    public long DebetTransactionSum
    {
      get { return val4; }
      set { val4 = value; }
    }

    public ushort CreditTransactionCount
    {
      get { return val5; }
      set { val5 = value; }
    }

    public long CreditTransactionSum
    {
      get { return val6; }
      set { val6 = value; }
    }

    public ushort CorrectionTransactionCount
    {
      get { return val7; }
      set { val7 = value; }
    }

    public long CorrectionTransactionSum
    {
      get { return val8; }
      set { val8 = value; }
    }
  }


  #endregion Fields

  #region Header [ заголовок, длинна 48 байт ]
  public enum MessageType : sbyte
  {
    Admin = (sbyte)'A',
    Finance = (sbyte)'F'
  }

  public enum MessageSubType : sbyte
  {
    ReversalTimeoutA = (sbyte)'A',   // Реверс по timeout, если он не получен ответ на свой предыдущий запрос
    ReversalTimeout = (sbyte)'T',    // Реверс по timeout, если он не получен ответ на свой предыдущий запрос
    ReversalWrongMAC = (sbyte)'R',   // Реверс, инициируемый терминалом при получении ответа с неправильным МАС
    ReversalFraudAlert = (sbyte)'M', // Реверс, проводимый в связи с высоким риском мошеннической операции (Fraud Alert)
    ReversalClient = (sbyte)'U',     // Реверс, проводимый по требованию клиента
    ReversalOther = (sbyte)'C',      // Реверс, проводимый терминалом по любым другим причинам
    OnlineTransaction = (sbyte)'O',  // Online транзакция
    ForsedTransaction = (sbyte)'F',  // Принудительное проведение транзакции. Например, когда POS в отсутствии связи запоминает транзакцию, а потом передает ее на хост. Транзакция расценивается хостом как электронный slip
    OfflineTransaction = (sbyte)'S'  // Транзакция, проведенная в offline (например, below floor limit) либо транзакция, требующая безусловного исполнения на хосте
  }

  public enum TransactionCode : sbyte
  {
    Purchase              = 0,  // Продажа
    MailOrTelephoneOrder  = 3,  // Заказ по почте или по телефону
    MerchandiseReturn     = 4,  // Возврат товара/Депозит
    CashAdvance           = 5,  // Выдача наличных
    CardVerification      = 6,  // Проверка карты
    BalanceInquiry        = 7,  // Запрос баланса
    PurchaseWithCashback  = 8,  // Покупка со сдачей
    Replenishment         = 17, // Пополнение счета
    Payment               = 41, // Платеж на счет вендора
    CashPayment           = 43, // Бескарточный POS-платеж
    StatementRequest      = 47, // Запрос выписки
    LogonRequest = 50,
    LogoffRequest = 51,
    CloseBatchRequest     = 60, // запрос на закрытие пакета
    CloseShiftRequest     = 61, // запрос на закрытие смены
    CloseDayRequest       = 62, // запрос на закрытие дня
    SubtotalsBatchRequest = 65, // запрос итогов по пакету
    SubtotalsShiftRequest = 66, // запрос итогов за смену
    SubtotalsDayRequest   = 67, // запрос итогов за день
    DownloadRequest       = 90, // Запрос на прогрузку терминала.
    FileDownloadRequest   = 93, // Запрос на загрузку конфигурационного файла
    HandshakeRequest      = 95  // Проверка связи
  }

  public enum ProcessingFlag1 : sbyte
  {
    ReplayAndDisconnect = (sbyte)'0',
    ReplayAndWait = (sbyte)'1'
  }

  public enum ProcessingFlag2 : sbyte
  {
    NoLoad = (sbyte)'0',  // NeedLoadParameters
    NeedLoad = (sbyte)'1'
  }

  #region ResponseCode (does'n used jet)
  public enum ResponseCodes : short
  {
    rc000 = 000, // Approved, balances available
    rc001 = 001, // Approved, no balances available
    rc003 = 003, // Approved, additional identification requested
    rc007 = 007, // Approved administrative transaction
    rc010 = 010, // Approved for a lesser amount
    rc050 = 050, // Genera (Финансовую транзакцию выполнить не удалось)
    rc051 = 051, // Expired card (Карта клиента просрочена)
    rc052 = 052, // Number of PIN tries exceeded (Превышено число попыток ввода PIN)
    rc053 = 053, // No sharing allowed (Не удалось маршрутизировать транзакцию)
    rc055 = 055, // Invalid transaction (Транзакция имеет некорректные атрибуты или данная операция на данном терминале не разрешена)
    rc056 = 056, // Transaction not supported by institution (Запрашиваемая операция не поддерживается хостом)
    rc057 = 057, // Lost or stolen card (Карта клиента имеет статус «потеряна» или «украдена»)
    rc058 = 058, // Invalid card status (Карта клиента имеет неправильный статус)
    rc059 = 059, // Restricted status (Карта клиента имеет ограниченные возможности)
    rc060 = 060, // Account not found (Не найден вендор с указанным номером счета)
    rc061 = 061, // Wrong customer information for payment (Неверное количество информационных полей для заданного вендора)
    rc062 = 062, // Customer information format error (Неверный формат информационного поля платежа.)
    rc063 = 063, // Prepaid Code not found (Не найден prepaid-код на указанную сумму)
    rc064 = 064, // Bad track information (Track2 карты клиента содержит неверную информацию)
    rc800 = 800, // Format error (Ошибка формата)
    rc801 = 801, // Original transaction not found (Не найдена оригинальная транзакция для реверса)
    rc809 = 809, // Invalid close transaction (Неверная операция закрытия периода (пакета, смены, дня)
    rc810 = 810, // Transaction timeout (Произошел тайм-аут)
    rc811 = 811, // System error (Системная ошибка)
    rc820 = 820, // Invalid terminal identifier (Неправильный идентификатор терминала)
    rc880 = 880, // Download has been received in its entirety (Был послан последний пакет - прогрузка успешно завершена)
    rc881 = 881, // Download received successfully and there is more data for this download (Предыдущий этап прогрузки был успешно выполнен –имеются еще данные для прогрузки.)
    rc882 = 882, // Download aborted (call for service) (Прогрузка терминала остановлена. Необходимо позвонить в процессинговый центр)
    rc897 = 897, // Invalid cryptogram (Получена неверная криптограмма в транзакции с использованием шифрования трафика)
    rc898 = 898, // Invalid MAC (Получен неверный MAC)
    rc899 = 899, // Sequence error—resync(Произошла рассинхронизация. Возникает, когда Sequence Number предыдущей транзакции + 1 не равен Sequence Number следующей транзакции. Для реверса возникает, когда Transmission Number реверса не равен Transmission Number предыдущей транзакции.)
    rc900 = 900, // Pin Tries Limit Exceeded (Привышено число попыток ввода PIN. Требуется захват карты.)
    rc901 = 901, // Expired Card (Карта просрочена, требуется захват карты.)
    rc909 = 909, // External Decline Special Condition (Требуется захват карты)
    rc959 = 959, // Administrative transaction not supported (Административная транзакция не поддерживается)
    rc999 = 999  // SGN: добавлен мной для заполнения поля в запросе и возможности сравнения значений в запросе и ответе
  }
  #endregion

  public class Header
  {
    public Header()
    {
      DeviceType = "9.";
      TransmissionNumber = 0; // контроль дублей не используется
      TerminalID = "MYBILLR"; //"MYBILLR"; "MYBILLB";
      EmployeeID = "u0450u";  // просто призвольный код ответственного лица
      CurrentDate = DateTime.Now.ToString("yyMMdd");
      CurrentTime = DateTime.Now.ToString("HHmmss");
      MessageType = MessageType.Admin;
      MessageSubType = MessageSubType.OnlineTransaction;
      TransactionCode = TransactionCode.HandshakeRequest;
      //ProcessingFlag1 = ProcessingFlag1.ReplayAndWait;     // ReplayAndDisconnect;
      //ProcessingFlag2 = ProcessingFlag2.NoLoad;            // NeedLoad
      ProcessingFlag1 = ProcessingFlag1.ReplayAndDisconnect; // ReplayAndDisconnect;
      ProcessingFlag2 = ProcessingFlag2.NoLoad;              // NeedLoad
      ProcessingFlag3 = '0';  // "любой символ"
      ResponseCode = "999"; // используется только в ответе
    }

    public int Size()
    {
      return 48;
    }

    public string GetFormatedValue()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DeviceType);
      sb.Append(TransmissionNumber.ToString("D2"));

      if( TerminalID.Length > 16 ) throw new Exception("неверная длина поля TerminalID");
      else sb.Append(TerminalID.PadRight(16));
      
      if( EmployeeID.Length > 6 ) throw new Exception("неверная длина поля EmployeeID");
      else sb.Append(EmployeeID.PadRight(6));
      //sb.Append(EmployeeID.PadRight(6));
      
      if( CurrentDate.Length > 6 ) throw new Exception("неверная длина поля CurrentDate");
      else sb.Append(CurrentDate.PadRight(6));

      if( CurrentTime.Length > 6 ) throw new Exception("неверная длина поля CurrentTime");
      else sb.Append(CurrentTime.PadRight(6));

      sb.Append((char)MessageType);
      sb.Append((char)MessageSubType);
      sb.Append(((sbyte)TransactionCode).ToString("D2"));
      sb.Append((char)ProcessingFlag1);
      sb.Append((char)ProcessingFlag2);
      sb.Append(ProcessingFlag3);
      sb.Append(ResponseCode);
      if( sb.Length != 48 ) throw new Exception("неверная длина поля заголовка");
      //Console.WriteLine("ВНИМАНИЕ! контроль длинны заголовка временно отключен!!!");
      return sb.ToString();
    }

    public string DeviceType { get; set; }  //privat? internal?
    public byte TransmissionNumber { get; set; }
    public string TerminalID { get; set; }
    public string EmployeeID { get; set; }
    public string CurrentDate { get; set; }
    public string CurrentTime { get; set; }
    public MessageType MessageType { get; set; }
    public MessageSubType MessageSubType { get; set; }
    public TransactionCode TransactionCode { get; set; }
    public ProcessingFlag1 ProcessingFlag1 { get; set; }
    public ProcessingFlag2 ProcessingFlag2 { get; set; }
    public char ProcessingFlag3 { get; set; }
    public string ResponseCode { get; set; }
  }
  #endregion Header

  #region Message (abstract)
  public abstract class Message
  {
    public const byte FS = 0x1C; // Field Separator
    
    protected Header header = new Header();
    protected Header headerRs = new Header();

    // generic методы
    // http://stackoverflow.com/questions/196661/calling-a-static-method-on-a-generic-type-parameter
    // http://stackoverflow.com/questions/3360661/call-a-method-of-type-parameter
    // generic свойства
    // http://stackoverflow.com/questions/2587236/generic-property-in-c-sharp

    private Fields<string, Field> fields = new Fields<string, Field>();
    private Fields<string, Field> fieldsRs = new Fields<string, Field>();

    //private Fields<FieldType, Field> fields = new Fields<FieldType, Field>(); // <- хочется так сделать, но пока не получается из-за ограничений generic'ов
    private byte lrc; // контрольная суммма

    public Message(MessageType type, MessageSubType subtype, TransactionCode transcode)
    {
      this.header.MessageType = type;
      this.header.MessageSubType = subtype;
      this.header.TransactionCode = transcode;
    }

    public U Add<U>(U field) where U : Field //, new()
    {
      this.Fields.Add(typeof(U).Name, field);
      return field;
    }

    public U Add<U>() where U : Field, new()
    {
      U field = new U();
      this.Fields.Add(typeof(U).Name, field);
      return field;
    }

    //sgn : feb 17, 2014
    //public void Del<U>() where U : Field
    //{
    //  //U field = new U();
    //  this.Fields.Del(typeof(U).Name);
    //  //return field;
    //}

    public U Get<U>() where U : Field
    {
      Field f = null;
      if( this.Fields.TryGetValue(typeof(U).Name, out f) ) {
        return (f as U);
      }
      return null;

      // первый вариант: генерирует исключение при отсутсвии требуемого поля...
      //return (this.Fields[typeof(U).Name] as U);
    }

    //public U Get<U>(FieldType type) where U : Field
    //{
    //  //string s1 = typeof(U).Name;
    //  return (this.Fields[type] as U);
    //}

    public U GetRs<U>() where U : Field
    {
      Field f = null;
      if( this.FieldsRs.TryGetValue(typeof(U).Name, out f) ) {
        return (f as U);
      }
      return null;
    }


    public string GetFormatedValue() // только из запроса
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(header.GetFormatedValue());
      foreach( var f in this.fields ) {
        string val = f.GetFormatedValue();
        if( !string.IsNullOrEmpty(val) ) {
          sb.Append(string.Format("{0:D1}{1}", (char)Message.FS, val));
        }
      }
      return sb.ToString();
    }

    public virtual void SetReversal(MessageSubType messageSubType)
    {
      if( (messageSubType == MessageSubType.ReversalTimeoutA) ||
          (messageSubType == MessageSubType.ReversalTimeout) ||
          (messageSubType == MessageSubType.ReversalClient) ||
          (messageSubType == MessageSubType.ReversalFraudAlert) ||
          (messageSubType == MessageSubType.ReversalWrongMAC) ||
          (messageSubType == MessageSubType.ReversalOther) ) {
        this.header.MessageSubType = messageSubType;
      }
      else {
        //TODO: проверить результат форматирования
        throw new Exception(string.Format("данный субтип [{0}] неприменим для транзакции реверса", messageSubType));
      }
    }

    public virtual byte[] Encode(Encoding encoding)
    {
      string body = this.GetFormatedValue();

      // подсчитываем контрольную сумму
      byte lrc = 0;
      for( int i = 0; i < body.Length; ++i ) {
        lrc ^= (byte)body[i];
      }
      lrc ^= (byte)Compass.TpTp.EscType.ETX;
      this.lrc = lrc;

      body = string.Format("{0}{1}{2}{3}", (char)Compass.TpTp.EscType.STX, body, (char)Compass.TpTp.EscType.ETX, (char)this.lrc);
      return encoding.GetBytes(body);
    }

    public abstract void Decode(byte[] data, Encoding encoding);

    protected void DecodeHeader(byte[] data, Encoding encoding)
    {
      //this.header.ProcessingFlag2 = (ProcessingFlag2)data[43];
      //this.header.ProcessingFlag3 = encoding.GetString(data, 44, 1);
      //this.header.ResponseCode = encoding.GetString(data, 45, 3);

      //TODO: все это переместить в отдельный заголовок для ответа
      //      потом можно будет сравнивать поля заголовков в запросе и ответе
      //this.header.TransmissionNumberRs = Convert.ToByte(encoding.GetString(data, 2, 2));
      //this.header.CurrentDateRs = encoding.GetString(data, 26, 6);
      //this.header.CurrentTimeRs = encoding.GetString(data, 32, 6);

      // парсим заголовок ответа
      this.headerRs.DeviceType = encoding.GetString(data, 00, 2);
      this.headerRs.TransmissionNumber = Convert.ToByte(encoding.GetString(data, 02, 2));
      this.headerRs.TerminalID = encoding.GetString(data, 04, 16);
      this.headerRs.EmployeeID = encoding.GetString(data, 20, 6);
      this.headerRs.CurrentDate = encoding.GetString(data, 26, 6);
      this.headerRs.CurrentTime = encoding.GetString(data, 32, 6);
      this.headerRs.MessageType = (MessageType)data[38];
      this.headerRs.MessageSubType = (MessageSubType)data[39];
      this.headerRs.TransactionCode = (TransactionCode)Convert.ToByte(encoding.GetString(data, 40, 2));
      this.headerRs.ProcessingFlag1 = (ProcessingFlag1)data[42];
      this.headerRs.ProcessingFlag2 = (ProcessingFlag2)data[43];
      this.headerRs.ProcessingFlag3 = (char)data[44];
      this.headerRs.ResponseCode = encoding.GetString(data, 45, 3);
    }

    protected void DecodeBody(byte[] data, Encoding encoding)
    {
      byte state = 0; // 0 - none, 1 - fieldstart, 2 - fielddecoding, 3 - endofdata

      bool fieldFound = true;
      string fieldName = string.Empty;
      Field f = null;
      int pos = this.header.Size();
      while( pos < data.Length ) {
        if( (state == 0) && (data[pos] == (byte)EscType.ETX) ) {
          // встретили окончание сообщения - выходим из цикла
          state = 3;
          break;
        }
        else if( (state == 0) && (data[pos] == Message.FS) ) {
          // встретили начало нового поля (FS <- разделитель полей)
          state = 1;
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.F) ) {
          // встретили поле 'F' [F108152 A]
          fieldName = typeof(ApprovalCodeField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {  //if( !this.Fields.TryGetValue(FieldType.F, out f) ) {
            f = new ApprovalCodeField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.J) ) {
          // встретили поле 'J' [J           3373727]
          fieldName = typeof(AvailableBalanceField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new AvailableBalanceField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.N) ) {
          // встретили поле 'N' [?]
          fieldName = typeof(CustomerIDField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new CustomerIDField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.R) ) {
          // встретили поле 'R' [RD]
          fieldName = typeof(CardTypeField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new CardTypeField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.S) ) {
          // встретили поле 'S' [S130426031025    ]
          fieldName = typeof(InvoiceNumberField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new InvoiceNumberField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.V) ) {
          // встретили поле 'V' [??????]
          fieldName = typeof(DownloadKeyField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new DownloadKeyField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.W) ) {
          // встретили поле 'W' [??????]
          fieldName = typeof(DownloadTextField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new DownloadTextField();
          }
        }
        //TODO: пока не знаю как обрабатывать поля переменной длины...
        else if( (state == 1) && (data[pos] == (byte)FieldType.g) ) {
          // встретили поле 'g' [gПример данных для поля g]
          fieldName = typeof(AdditionalReceiptDataField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new AdditionalReceiptDataField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.h) ) {
          // встретили поле 'h' [h0010010740]
          fieldName = typeof(SequenceNumberField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new SequenceNumberField();
          }
        }
        else if( (state == 1) && (data[pos] == (byte)FieldType.a) ) {
          // встретили поле 'a' [a&C840#&R01#]
          fieldName = typeof(OptionalDataField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new OptionalDataField();
          }
        }
        //- сверки -------------
        else if( (state == 1) && (data[pos] == (byte)FieldType.l) ) {
          // встретили поле 'l' [l0010020224+0000000000048006230074+0000000000074215790000+000000000000000000]
          fieldName = typeof(TotalsBatchField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new TotalsBatchField();
          }
        }
        //---------------------
        else if( (state == 1) && (data[pos] == (byte)FieldType.t) ) {
          // встретили поле 't' [t1483754]
          fieldName = typeof(TransactionIDField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new TransactionIDField();
          }
        }
        //-[ sgn :jan 30, 2014 ]-------------------
        else if( (state == 1) && (data[pos] == (byte)FieldType.f6) ) {
          // встретили поле '6' [6Q010506C231C5D1EEE20A3030R019F1804001934BE861D8424000218DC5747BE315D8A60D6E5337332B5D8595D966103E4BE439B]
          fieldName = typeof(Emv.ProductSubFIDsField).Name;
          if( !(fieldFound = this.FieldsRs.TryGetValue(fieldName, out f)) ) {
            f = new Emv.ProductSubFIDsField();
          }
        }
        else {
          // так как поле, которое мы встретили, нами не обрабатывается, то пропускаем его
          state = 0;
        }
        if( f != null ) { //if( fieldName != string.Empty ) {
          try {
            f.Decode(pos, data);
            if( !fieldFound ) {
              this.FieldsRs.Add(fieldName, f);
              //this.FieldsRs.Add(f.Name, f); // можно и как-то так????
              //AddRs<OptionalDataField>(f);  // <- как бы так сделать????
            }
          }
          catch( Exception e ) {
            Console.WriteLine("[Err] не удалось декодировать поле ({0}) msg: {1}", f.FID, e.Message);
          }
          pos += f.Size(); //увеличиваем позицию на размер поля
          //fieldName = string.Empty;
          f = null; // сброс

          state = 0; // чтобы были готовы считывать следующее поле
        }
        ++pos;
      }
    }

    public Header Header
    {
      get { return this.header; }
    }

    public Header HeaderRs
    {
      get { return this.headerRs; }
    }

    public Fields<string, Field> Fields  // поля из запроса
    {
      get { return this.fields; }
    }

    public Fields<string, Field> FieldsRs // поля из ответа
    {
      get { return this.fieldsRs; }
    }

    public byte LRC
    {
      get { return this.lrc; }
      //set { this.lrc = value; }
    }
  }
  #endregion message

  #region Field (abstract)
  public enum FieldType : sbyte
  {
    A = (sbyte)'A',  // CustomerBillingAddress
    B = (sbyte)'B',  // Amount1
    C = (sbyte)'C',  // Amount2
    D = (sbyte)'D',  // ApplicationAcountType
    E = (sbyte)'E',  // ApplicationAcountNumber
    F = (sbyte)'F',  // Approval Code
    G = (sbyte)'G',  // Authentication Code
    J = (sbyte)'J',  // Available Balance
    N = (sbyte)'N',  // Customer ID
    Q = (sbyte)'Q',  // Echo Data
    R = (sbyte)'R',  // Card Type
    S = (sbyte)'S',  // Invoice Number
    T = (sbyte)'T',  // Invoice Number/Original
    U = (sbyte)'U',  // Language Code
    V = (sbyte)'V',  // Download Key/Download File
    W = (sbyte)'W',  // Download Text
    a = (sbyte)'a',  // Optional Data
    b = (sbyte)'b',  // PIN/Customer
    c = (sbyte)'c',  // New PIN/Customer
    e = (sbyte)'e',  // POS Condition Code
    f = (sbyte)'f',  // Receipt data–response
    g = (sbyte)'g',  // Additional Receipt Data
    h = (sbyte)'h',  // Sequence Number
    l = (sbyte)'l',  // Totals/Batch
    m = (sbyte)'m',  // Totals/Day
    o = (sbyte)'o',  // Totals/Shift
    q = (sbyte)'q',  // Track2/Customer
    t = (sbyte)'t',  // Transaction ID
    f6 = (sbyte)'6'  // Product SubFIDs
  }

  #region old Field
  //public class Field
  //{
  //  protected HashSet<SubField> fields;
  //  public FieldType FID;
  //  protected sbyte SFSB = (sbyte)'&'; // Sub Field Separator Beginer  // <- const????
  //  protected sbyte SFSE = (sbyte)'#'; // Sub Field Separator Ender    // <- const????

  //  public Field(FieldType fid)
  //  {
  //    FID = fid;

  //    // создаем хранилище для субполей только для определенных типов
  //    switch( fid){
  //      case FieldType.a:
  //        this.fields = new HashSet<SubField>();
  //        break;
  //      default:
  //        this.fields = null;
  //        break;
  //    }
  //  }

  //  protected virtual string GetValue()
  //  {
  //    throw new Exception("the method Field::GetValue() should be overrided");
  //  }

  //  public string GetFormatedValue()
  //  {
  //    StringBuilder sb = new StringBuilder();
  //    sb.Append(GetValue());
  //    if( this.fields != null ) {
  //      foreach( var f in fields ) {
  //        sb.Append(string.Format("{0:D1}{1}{2:D1}", (char)this.SFSB, f.GetValue(), (char)this.SFSE));
  //      }
  //    }
  //    return sb.ToString();
  //  }
  //}

  //public class Field<T> where T : FieldType
  //{
  //  public T FID;
  //  public object value = new object();


  //  public virtual string GetFormatedValue()
  //  {
  //    throw new Exception("method has been not implemented");
  //  }
  //}
  #endregion

  public abstract class Field
  {
    public Field(FieldType fid)
    {
      this.FID = fid;
      this.Encoding = Encoding.GetEncoding(866);
    }

    // если класс не абстрактный
    //protected virtual string GetValue()
    //{
    //  throw new Exception("the method Field::GetValue() should be overrided");
    //}

    protected abstract string GetValue();
    public abstract int Size();
    internal virtual void Decode(int pos, byte[] data)
    {
      //TODO: Field::Decode(...)
      Console.WriteLine("[Wrn] Field::Decode(...) ничего не делаем????");
    }

    public string GetFormatedValue()
    {
      string val = GetValue();
      if( string.IsNullOrEmpty(val) ) return null; //string.Empty;
      return string.Format("{0}{1}",(char)FID, val);
    }

    public FieldType FID { get; private set; }
    public Encoding Encoding { get; set; }
  }
  #endregion field

  #region SubField для FID 'a'

  // вообще, это подполя основного поля FID 'a'
  public enum SubFieldType : sbyte
  {
    A = (sbyte)'A',  //
    B = (sbyte)'B',  //
    C = (sbyte)'C',  // цифровой код валюты в формате ISO 4217
    P = (sbyte)'P',  // информация о выполняемом платеже (см. формат в ...)
    R = (sbyte)'R',  // Код ответа авторизатора хоста (см. справочник TWO “Коды ответа авторизатора”)
    T = (sbyte)'T',  // текстовое сообщение от владельца терминала (см. формат в ...)
    V = (sbyte)'V',  // CVV2, 3CSC (3 символа), 4CSC (4 символа) – может указываться в запросе для транзакций с ручным вводом номера карты
    W = (sbyte)'W',  // результат проверки CVV и VISA CAVV
    //...
    X = (sbyte)'X',  //
    None = (sbyte)0
  }

  public class StringSubField : SubField
  {
    private string value;

    public StringSubField(SubFieldType sfid) : base(sfid)
    {
    }

    public override string GetValue()
    {
      if( string.IsNullOrEmpty(value) ) return null;
      return this.value;
    }

    public override void Decode(int pos, byte[] data)
    {
      //примечание: позиционер pos указывает на код субполя
      //            данные начинаются с позиции pos + 1
      //исключения ловим при декодировании в базовом классе message
      int i = 0;
      do {
        if( data[pos + 1 + i] == (byte)'#' ) break; // обнаружили разделитель окончания поля
        if( data[pos + 1 + i] == (byte)'&' ) break; // обнаружили разделитель начала поля
        if( data[pos + 1 + i] == (byte)Message.FS ) break; // обнаружили начало следующего поля
        if( data[pos + 1 + i] == (byte)EscType.ETX ) break; // обнаружили конец сообщения
        ++i;
      }
      while( i < 600 );
      if( i > 0 ) value = this.Encoding.GetString(data, pos + 1, i);
      else value = string.Empty;
    }

    public override int Size()
    {
      if( string.IsNullOrEmpty(value) ) return 0;
      return value.Length + 3; // <- длина значения поля + длина кода поля + длинна 2-ух разделителей
    }

    public override string ToString()
    {
      return this.value;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public abstract class SubField
  {
    public SubField(SubFieldType sfid)
    {
      this.SFID = sfid;
      this.Encoding = Encoding.GetEncoding(866);
    }

    //public virtual string GetValue()
    //{
    //  throw new Exception("the method SubField::GetValue() should be overrided");
    //}

    public abstract string GetValue();
    public virtual void Decode(int pos, byte[] data)
    {
    }
    public abstract int Size();

    public SubFieldType SFID { get; private set; }
    public Encoding Encoding { get; set; }
  }
  #endregion subfield

  #region SubField для FID '6'

  public enum SubFieldf6Type : sbyte
  {
    fA = (sbyte)'B',  // Manual CVD-Customer
    fE = (sbyte)'E',  // POS Entry Mode
    fH = (sbyte)'H',  // CVD Presence Indicator and Result
    fI = (sbyte)'I',  // Transaction Currency Code
    fO = (sbyte)'O',  // EMV Request Data
    fP = (sbyte)'P',  // EMV Additional Request Data
    fQ = (sbyte)'Q',  // EMV Response Data
    fR = (sbyte)'R',  // EMV Additional Response Data
    fS = (sbyte)'S',  // EMV Additional Request Data for Advice/Reversal
    fT = (sbyte)'T',  // DUKPT Key Serial Number and Descriptor
    ft = (sbyte)'t',  // DUKPT Key Serial Number and Descriptor 2
    None = (sbyte)0
  }

  #endregion 

  #region SubField для FID 'W'

  public enum SubFieldfWType : sbyte
  {
    fA = (sbyte)'A',  // Referral phone, Backup phone, разделены пробелами
    fB = (sbyte)'B',  // Время автоматического завершения дня (ЧЧММ)
    fC = (sbyte)'C',  // Конфигурация приложения. Описание структуры поля см. в описании протокола TpTp
    fD = (sbyte)'D',  // Параметры повторного использования. Описание структуры поля см. в описании протокола TpTp
    fE = (sbyte)'E',  // Максимальное число offline-транзакций
    fF = (sbyte)'F',  // Количество offline-транзакций, по достижении которого запускается принудительная пересылка пакета накопленных на терминале offline-транзакций на хост
    fG = (sbyte)'G',  // Код валюты терминала в формате: DTPxxx,T здесь DTP- идентификатор поля, xxx – ISO код валюты, номер транзакции по умолчанию. 1 байт в виде символа Purchase=0, Pre-purchase=1, Mail/Phone Order=3, Cash advance=5, Balance inquiry=7, Deposit=9, Quasi-cash=N
    fH = (sbyte)'H',  // Последняя строка чека
    fa = (sbyte)'a',  // Наименование и месторасположения терминала
    fb = (sbyte)'b',  // Наименование города и области, где установлен терминал
    fc = (sbyte)'c',  // Описание владельца терминала
    ff = (sbyte)'f',  // Описание предприятия, отвечающего за сервисное обслуживание терминала.
    fg = (sbyte)'g',  // PIN-ключ, генерируется хостом
    fh = (sbyte)'h',  // В этом поле на терминал присылается пара ключей: «ключ аутентификации» (MAC-ключ) и опционально «ключ для шифрования трафика» (EWK-ключ) разделенных между собой запятой.
    fp = (sbyte)'p',  // Список разрешенных на терминале транзакций
    fq = (sbyte)'q',  // Идентификатор торговой организации
    fr = (sbyte)'r',  // Название торговой организации
    None = (sbyte)0
  }

  public class StringSubFieldfW : SubFieldfW
  {
    private string value;

    public StringSubFieldfW(SubFieldfWType sfid, int len) : base(sfid, len)
    {
    }

    public override string GetValue()
    {
      if( string.IsNullOrEmpty(value) ) return null;
      return this.value;
    }

    public override void Decode(int pos, byte[] data)
    {
      //примечание: позиционер pos указывает на код субполя
      //            данные начинаются с позиции pos + 1
      //исключения ловим при декодировании в базовом классе message
      int i = 0;
      do {
        if( data[pos + 1 + i] == (byte)SubFieldfW.GS ) break; // обнаружили разделитель начала поля
        if( data[pos + 1 + i] == (byte)Message.FS ) break;    // обнаружили начало следующего поля
        if( data[pos + 1 + i] == (byte)EscType.ETX ) break;   // обнаружили конец сообщения
        ++i;
      }
      while( i < 130 );
      if( i > 0 ) value = this.Encoding.GetString(data, pos + 1, i);
      else value = string.Empty;
    }

    public override int Size()
    {
      if( this.len == 0 ) {
        if( string.IsNullOrEmpty(value) ) return 0;
        return value.Length;
      }
      else return this.len;
    }

    public override string ToString()
    {
      return this.value;
    }

    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }

  public abstract class SubFieldfW
  {
    protected int len = 0;
    public const byte GS = 0x1D; // Group Separator (GS)

    public SubFieldfW(SubFieldfWType sfid, int length)
    {
      this.SFID = sfid;
      this.len = length; // 0 - пересенной длинныж; если 0, то фиксированной длинны
      this.Encoding = Encoding.GetEncoding(866);
    }

    //public virtual string GetValue()
    //{
    //  throw new Exception("the method SubField::GetValue() should be overrided");
    //}

    public abstract string GetValue();
    public virtual void Decode(int pos, byte[] data)
    {
    }
    public abstract int Size();

    public SubFieldfWType SFID { get; private set; }
    public Encoding Encoding { get; set; }
  }
  #endregion

  #region Field List [ класс Fields<T> ]
  //public class Fields<T> where T : DFObject
  //public class Fields<T> : IEnumerable<T> where T : Field  //ERR
  public class Fields<K, T> : IEnumerable<T>  //OK
  {
    //private TableDef parent = null;
    //private Dictionary<string, T> fieldByCode = new Dictionary<string, T>();  //???
    //private Dictionary<string, T> fieldByCode = new Dictionary<string, T>();  //OK
    private Dictionary<K, T> fieldByCode = new Dictionary<K, T>();  //???
    private Dictionary<int, T> fieldById = new Dictionary<int, T>();

    public Fields()
    {
    }
    public T this[K Name]
    {
      get {
        T field;
        if( fieldByCode.TryGetValue(Name, out field) ) {
          return field;
        }
        throw new ArgumentOutOfRangeException("неизвестный код (code): " + Name);
      }
      // оказывается эта часть (set) не нужна....
      //set {
      //  fieldByCode.Add(Name, value);
      //}
    }
    public T this[int Id]
    {
      get {
        T field;
        if( fieldById.TryGetValue(Id, out field) ) {
          return field;
        }
        throw new ArgumentOutOfRangeException("неизвестный иднтификатор (id): " + Id);
      }
      // оказывается эта часть (set) не нужна....
      //set {
      //  fieldById.Add(Id, value);
      //}
    }
    public bool TryGetValue(K Name, out T Value)
    {
      return fieldByCode.TryGetValue(Name, out Value);
    }
    public bool TryGetValue(int Id, out T Value)
    {
      return fieldById.TryGetValue(Id, out Value);
    }
    //public string GetString()
    //{
    //  StringBuilder sb = new StringBuilder();
    //  foreach( KeyValuePair<string, T> kp in fields ) {
    //    sb.Append( ' ', kp.Value.Size );
    //  }
    //  return sb.ToString();
    //}
    public void Add(int Id, K Code, T Field)
    {
      fieldById.Add(Id, Field);
      fieldByCode.Add(Code, Field);
    }
    public void Add(K Code, T Field)
    {
      fieldByCode.Add(Code, Field);
      fieldById.Add(fieldById.Count, Field);
    }
    //public void Add(T Field)// where T : Field
    //{
    //  fieldByCode.Add((typeof(T).Name as K), Field);
    //  fieldById.Add(fieldById.Count, Field);
    //}

    ////sgn : feb 17, 2014
    //public void Del(K Code)
    //{
    //  T val;
    //  if( fieldByCode.TryGetValue(Code, out val) ) {
    //    fieldById.Remove(T.);
    //    fieldByCode.Remove(Code);
    //  }
    //}
    public int Count
    {
      get { return fieldById.Count; }
    }

    // материалы по теме:
    //  - http://msdn.microsoft.com/en-us/library/78dfe2yb%28v=VS.100%29.aspx
    //private class FieldsEnumirator : IEnumerator
    private class FieldsEnumirator : IEnumerator<T>
    {
      IEnumerator<KeyValuePair<int, T>> enumerator;

      public FieldsEnumirator(Dictionary<int, T> dictionary)
      {
        enumerator = dictionary.GetEnumerator();
      }
      public T Current
      {
        get { return enumerator.Current.Value; }
      }
      object IEnumerator.Current
      {
        get { return enumerator.Current.Value; }
      }
      public bool MoveNext()
      {
        return enumerator.MoveNext();
      }
      public void Reset()
      {
        enumerator.Reset();
      }
      public void Dispose()
      {
        //TODO: требуется реализовать?
      }
      //void IDisposable.Dispose() { }
    }

    #region IEnumerable Members
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      //return ((IDictionary)this).GetEnumerator();
      return new FieldsEnumirator(fieldById);
      //return fieldById.GetEnumerator();
      //return null;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new FieldsEnumirator(fieldById);
    }
    #endregion
  }
  #endregion
}
