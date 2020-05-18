using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace NfcPcSc
{
    class NfcApi
    {
        [DllImport("winscard.dll")]
        public static extern uint SCardEstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext);
 
        [DllImport("winscard.dll", EntryPoint = "SCardListReadersW", CharSet = CharSet.Unicode)]
        public static extern uint SCardListReaders(
          IntPtr hContext, byte[] mszGroups, byte[] mszReaders, ref UInt32 pcchReaders);
 
        [DllImport("WinScard.dll")]
        public static extern uint SCardReleaseContext(IntPtr phContext);
 
        [DllImport("winscard.dll", EntryPoint = "SCardConnectW", CharSet = CharSet.Unicode)]
        public static extern uint SCardConnect(IntPtr hContext,　string szReader,
             uint dwShareMode, uint dwPreferredProtocols, ref IntPtr phCard,
             ref IntPtr pdwActiveProtocol);
 
        [DllImport("WinScard.dll")]
        public static extern uint SCardDisconnect(IntPtr hCard, int Disposition);
 
        [StructLayout(LayoutKind.Sequential)]
        internal class SCARD_IO_REQUEST
        {
            internal uint dwProtocol;
            internal int cbPciLength;
            public SCARD_IO_REQUEST()
            {
                dwProtocol = 0;
            }
        }
 
        [DllImport("winscard.dll")]
        public static extern uint SCardTransmit(IntPtr hCard, IntPtr pioSendRequest, byte[] SendBuff, int SendBuffLen, SCARD_IO_REQUEST pioRecvRequest,
                byte[] RecvBuff, ref int RecvBuffLen);
 
        [DllImport("winscard.dll")]
        public static extern uint SCardControl(IntPtr hCard, int controlCode, byte[] inBuffer, int inBufferLen, byte[] outBuffer, int outBufferLen, ref int bytesReturned);
 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SCARD_READERSTATE
        {
            /// <summary>
            /// Reader
            /// </summary>
            internal string szReader;
            /// <summary>
            /// User Data
            /// </summary>
            internal IntPtr pvUserData;
            /// <summary>
            /// Current State
            /// </summary>
            internal UInt32 dwCurrentState;
            /// <summary>
            /// Event State/ New State
            /// </summary>
            internal UInt32 dwEventState;
            /// <summary>
            /// ATR Length
            /// </summary>
            internal UInt32 cbAtr;
            /// <summary>
            /// Card ATR
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            internal byte[] rgbAtr;
        }
 
        [DllImport("winscard.dll", EntryPoint = "SCardGetStatusChangeW", CharSet = CharSet.Unicode)]
        public static extern uint SCardGetStatusChange(IntPtr hContext, int dwTimeout, [In, Out] SCARD_READERSTATE[] rgReaderStates, int cReaders);
 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);
 
        [DllImport("kernel32.dll")]
        public static extern void FreeLibrary(IntPtr handle);
 
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string procName);
 
    }

    class ReadResult
    {
        /// <summary>
        /// リーダーの製造番号
        /// </summary>
        public string readerSerialNumber;
        /// <summary>
        /// カードのID
        /// </summary>
        public string cardId;
        /// <summary>
        /// カードの残高
        /// </summary>
        public int cardBalance;
    }

    class NfcConstant
    {
        public const uint SCARD_S_SUCCESS = 0;
        public const uint SCARD_E_NO_SERVICE = 0x8010001D;
        public const uint SCARD_E_TIMEOUT = 0x8010000A;
 
        public const uint SCARD_SCOPE_USER = 0;
        public const uint SCARD_SCOPE_TERMINAL = 1;
        public const uint SCARD_SCOPE_SYSTEM = 2;
 
        public const int SCARD_STATE_UNAWARE = 0x0000;
        public const int SCARD_STATE_CHANGED = 0x00000002;
		// This implies that there is a
        // difference between the state believed by the application, and
        // the state known by the Service Manager.  When this bit is set,
        // the application may assume a significant state change has
        // occurred on this reader.
        public const int SCARD_STATE_PRESENT = 0x00000020;// This implies that there is a card
        // in the reader.
        public const UInt32 SCARD_STATE_EMPTY = 0x00000010;  // This implies that there is not
        // card in the reader.  If this bit is set, all the following bits will be clear.
 
        public const int SCARD_SHARE_SHARED = 0x00000002; // - This application will allow others to share the reader
        public const int SCARD_SHARE_EXCLUSIVE = 0x00000001; // - This application will NOT allow others to share the reader
        public const int SCARD_SHARE_DIRECT = 0x00000003; // - Direct control of the reader, even without a card
 
 
        public const int SCARD_PROTOCOL_T0 = 1; // - Use the T=0 protocol (value = 0x00000001)
        public const int SCARD_PROTOCOL_T1 = 2;// - Use the T=1 protocol (value = 0x00000002)
        public const int SCARD_PROTOCOL_RAW = 4;// - Use with memory type cards (value = 0x00000004)
 
        public const int SCARD_LEAVE_CARD = 0; // Don't do anything special on close
        public const int SCARD_RESET_CARD = 1; // Reset the card on close
        public const int SCARD_UNPOWER_CARD = 2; // Power down the card on close
        public const int SCARD_EJECT_CARD = 3; // Eject the card on close
    }

    class NfcMain
    {
        public void start()
        {
            IntPtr context = establishContext();
 
            List<string> readersList = getReaders(context);
 
            NfcApi.SCARD_READERSTATE[] readerStateArray = initializeReaderState(context, readersList);
 
            bool quit = false; 
            while (!quit)
            {
                waitReaderStatusChange(context, readerStateArray, 1000);
                if ((readerStateArray[0].dwEventState & NfcConstant.SCARD_STATE_PRESENT) == NfcConstant.SCARD_STATE_PRESENT)
                {
                    ReadResult result2 = readCard(context, readerStateArray[0].szReader);
                    SendCommand(context, readerStateArray[0].szReader);
                    quit = true;
                }
            }
            uint ret = NfcApi.SCardReleaseContext(context);
        }
 
        private IntPtr establishContext()
        {
            IntPtr context = IntPtr.Zero;
 
            uint ret = NfcApi.SCardEstablishContext(NfcConstant.SCARD_SCOPE_USER, IntPtr.Zero, IntPtr.Zero, out context);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                string message;
                switch (ret)
                {
                    case NfcConstant.SCARD_E_NO_SERVICE:
                        message = "サービスが起動されていません。";
                        break;
                    default:
                        message = "Smart Cardサービスに接続できません。code = " + ret;
                        break;
                }
                Debug.WriteLine(message);
                return IntPtr.Zero;
            }
            Debug.WriteLine("Smart Cardサービスに接続しました。");
            return context;
        }
 
        List<string> getReaders(IntPtr hContext)
        {
            uint pcchReaders = 0;
 
            uint ret = NfcApi.SCardListReaders(hContext, null, null, ref pcchReaders);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                return new List<string>();//リーダーの情報が取得できません。
            }
 
            byte[] mszReaders = new byte[pcchReaders * 2]; // 1文字2byte
 
            // Fill readers buffer with second call.
            ret = NfcApi.SCardListReaders(hContext, null, mszReaders, ref pcchReaders);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                return new List<string>();//リーダーの情報が取得できません。
            }
 
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            string readerNameMultiString = unicodeEncoding.GetString(mszReaders);
 
            Debug.WriteLine("リーダー名を\\0で接続した文字列: " + readerNameMultiString);
            Debug.WriteLine(" ");
 
            List<string> readersList = new List<string>();
            int nullindex = readerNameMultiString.IndexOf((char)0);   // 装置は１台のみ
            readersList.Add(readerNameMultiString.Substring(0, nullindex));
            return readersList;
        }
 
        NfcApi.SCARD_READERSTATE[] initializeReaderState(IntPtr hContext, List<string> readerNameList)
        {
            NfcApi.SCARD_READERSTATE[] readerStateArray = new NfcApi.SCARD_READERSTATE[readerNameList.Count];
            int i = 0;
            foreach (string readerName in readerNameList)
            {
                readerStateArray[i].dwCurrentState = NfcConstant.SCARD_STATE_UNAWARE;
                readerStateArray[i].szReader = readerName;
                i++;
            }
            uint ret = NfcApi.SCardGetStatusChange(hContext, 100/*msec*/, readerStateArray, readerStateArray.Length);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("リーダーの初期状態の取得に失敗。code = " + ret);
            }
 
            return readerStateArray;
        }
 
        void waitReaderStatusChange(IntPtr hContext, NfcApi.SCARD_READERSTATE[] readerStateArray, int timeoutMillis)
        {
            uint ret = NfcApi.SCardGetStatusChange(hContext, timeoutMillis/*msec*/, readerStateArray, readerStateArray.Length);
            switch (ret)
            {
                case NfcConstant.SCARD_S_SUCCESS:
                    break;
                case NfcConstant.SCARD_E_TIMEOUT:
                    throw new TimeoutException();
                default:
                    throw new ApplicationException("リーダーの状態変化の取得に失敗。code = " + ret);
            }
 
        }
 
        ReadResult readCard(IntPtr context, string readerName)
        {
            IntPtr hCard = connect(context, readerName);
            string readerSerialNumber = readReaderSerialNumber(hCard);
            string cardId = readCardId(hCard);
            Debug.WriteLine(readerName + " (S/N " + readerSerialNumber + ") から、カードを読み取りました。" + cardId);
            disconnect(hCard);
 
            ReadResult result = new ReadResult();
            result.readerSerialNumber = readerSerialNumber;
            result.cardId = cardId;
            return result;
 
        }
 
        string readReaderSerialNumber(IntPtr hCard)
        {
            int controlCode = 0x003136b0; // SCARD_CTL_CODE(3500) の値 
            // IOCTL_PCSC_CCID_ESCAPE
            // SONY SDK for NFC M579_PC_SC_2.1j.pdf 3.1.1 IOCTRL_PCSC_CCID_ESCAPE
            byte[] sendBuffer = new byte[] { 0xc0, 0x08 }; // ESC_CMD_GET_INFO / Product Serial Number 
            byte[] recvBuffer = new byte[64];
            int recvLength = control(hCard, controlCode, sendBuffer, recvBuffer);
 
            ASCIIEncoding asciiEncoding = new ASCIIEncoding();
            string serialNumber = asciiEncoding.GetString(recvBuffer, 0, recvLength - 1); // recvBufferには\0で終わる文字列が取得されるので、長さを-1する。
            return serialNumber;
        }
 
        string readCardId(IntPtr hCard)
        {
            byte maxRecvDataLen = 64;
            byte[] recvBuffer = new byte[maxRecvDataLen + 2];
            byte[] sendBuffer = new byte[] { 0xff, 0xca, 0x00, 0x00, maxRecvDataLen };
            int recvLength = transmit(hCard, sendBuffer, recvBuffer);
 
            string cardId = BitConverter.ToString(recvBuffer, 0, recvLength - 2).Replace("-", "");
            return cardId;
        }
 
        void SendCommand(IntPtr hContext, string readerName)
        {
            int dwResponseSize;
            byte[] response = new byte[2048];
            long lResult;
 
            byte[] commnadSelectFile = { 0xff, 0xA4, 0x00, 0x01, 0x02, 0x0f, 0x09 };
            byte[] commnadReadBinary = { 0xff, 0xb0, 0x00, 0x00, 0x00 };
 
            IntPtr SCARD_PCI_T1 = getPciT1();
            NfcApi.SCARD_IO_REQUEST ioRecv = new NfcApi.SCARD_IO_REQUEST();
            ioRecv.cbPciLength = 2048;
            IntPtr hCard = connect(hContext, readerName);
 
            dwResponseSize = response.Length;
            lResult = NfcApi.SCardTransmit(hCard, SCARD_PCI_T1, commnadSelectFile, commnadSelectFile.Length, ioRecv, response, ref dwResponseSize);
            if (lResult != NfcConstant.SCARD_S_SUCCESS)
            {
                Debug.WriteLine("SelectFile error\n");
                return;
            }
            dwResponseSize = response.Length;
            lResult = NfcApi.SCardTransmit(hCard, SCARD_PCI_T1, commnadReadBinary, commnadReadBinary.Length, ioRecv, response, ref dwResponseSize);
            if (lResult != NfcConstant.SCARD_S_SUCCESS)
            {
                Debug.WriteLine("ReadBinary error\n");
                return;
            }
            parse_tag(response);
        }
 
        private void parse_tag(byte[] data)
        {
            int loop = 0;
 
            Debug.WriteLine("\nSuica履歴データ：" + BitConverter.ToString(data,0,20) + "\n");
  
        }
 
        private IntPtr getPciT1()
        {
            IntPtr handle = NfcApi.LoadLibrary("Winscard.dll");
            IntPtr pci = NfcApi.GetProcAddress(handle, "g_rgSCardT1Pci");
            NfcApi.FreeLibrary(handle);
            return pci;
        }
 
        IntPtr connect(IntPtr hContext, string readerName)
        {
           IntPtr hCard = IntPtr.Zero;
            IntPtr activeProtocol = IntPtr.Zero;
            uint ret = NfcApi.SCardConnect(hContext, readerName, NfcConstant.SCARD_SHARE_SHARED, NfcConstant.SCARD_PROTOCOL_T1, ref hCard, ref activeProtocol);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("カードに接続できません。code = " + ret);
            }
            return hCard;
        }
 
        void disconnect(IntPtr hCard)
        {
            uint ret = NfcApi.SCardDisconnect(hCard, NfcConstant.SCARD_LEAVE_CARD);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("カードとの接続を切断できません。code = " + ret);
            }
        }
 
        int control(IntPtr hCard, int controlCode, byte[] sendBuffer, byte[] recvBuffer)
        {
            int bytesReturned = 0;
            uint ret = NfcApi.SCardControl(hCard, controlCode, sendBuffer, sendBuffer.Length, recvBuffer, recvBuffer.Length, ref bytesReturned);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("カードへの制御命令送信に失敗しました。code = " + ret);
            }
            return bytesReturned;
        }
 
        int transmit(IntPtr hCard, byte[] sendBuffer, byte[] recvBuffer)
        {
            NfcApi.SCARD_IO_REQUEST ioRecv = new NfcApi.SCARD_IO_REQUEST();
            ioRecv.cbPciLength = 255;
 
            int pcbRecvLength = recvBuffer.Length;
            int cbSendLength = sendBuffer.Length;
            IntPtr SCARD_PCI_T1 = getPciT1();
            uint ret = NfcApi.SCardTransmit(hCard, SCARD_PCI_T1, sendBuffer, cbSendLength, ioRecv, recvBuffer, ref pcbRecvLength);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("カードへの送信に失敗しました。code = " + ret);
            }
            return pcbRecvLength; // 受信したバイト数(recvBufferに受け取ったバイト数)
        }
 
     }
}
