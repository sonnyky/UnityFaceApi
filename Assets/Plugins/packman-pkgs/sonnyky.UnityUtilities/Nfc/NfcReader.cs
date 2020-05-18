using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace P_NfcPcSc
{
    public class NfcReader : MonoBehaviour
    {
        private IntPtr context;
        private List<string> readersList;
        private NfcApi.SCARD_READERSTATE[] readerStateArray;
        private int nfc_state = NfcConstant.NFC_STATE_DISCONNECT;
        private bool isBeginTouch = true;
        private bool isTouchingCard = false;

        private string m_MemberCardNumber = "";

        void Start()
        {
            initNfc();
        }
        void Update()
        {
            context = establishContext();
            readersList = getReaders(context, nfc_state == NfcConstant.NFC_STATE_DISCONNECT);
            readerStateArray = readerStateChange(context, readersList);
            setState();

            if (readerStateArray.Length != 0)
            {
                if ((readerStateArray[0].dwEventState & NfcConstant.SCARD_STATE_PRESENT) == NfcConstant.SCARD_STATE_PRESENT)
                {
                    if (isBeginTouch)
                    {
                        Debug.Log("タッチし始め");
                        readCard(context, readerStateArray[0].szReader);
                        SendCommand(context, readerStateArray[0].szReader);
                        isTouchingCard = true;
                        isBeginTouch = false;
                    }
                    //Debug.Log("カードタッチ中");
                    NfcApi.SCardReleaseContext(context);
                }
                else
                {
                    if (isTouchingCard)
                    {
                        Debug.Log("カードが離れた");
                        isBeginTouch = true;
                        isTouchingCard = false;
                    }
                    //Debug.Log("カードがタッチされていない");
                }
            }
            else
            {
                isBeginTouch = true;
            }
        }
        private void initNfc()
        {
            context = establishContext();
            readersList = getReaders(context, true);
            readerStateArray = readerStateChange(context, readersList);
            setState();
        }
        private void setState()
        {
            if (readerStateArray.Length != 0)
            {
                nfc_state = NfcConstant.NFC_STATE_CONNECT;
            }
            else
            {
                nfc_state = NfcConstant.NFC_STATE_DISCONNECT;
            }
        }

        private IntPtr establishContext()
        {
            IntPtr eContext = IntPtr.Zero;

            uint ret = NfcApi.SCardEstablishContext(NfcConstant.SCARD_SCOPE_USER, IntPtr.Zero, IntPtr.Zero, out eContext);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                switch (ret)
                {
                    case NfcConstant.SCARD_E_NO_SERVICE:
                        Debug.LogWarning("サービスが起動されていません。");
                        break;
                    default:
                        Debug.LogWarning("Smart Cardサービスに接続できません。code = " + ret);
                        break;
                }
                return IntPtr.Zero;
            }
            return eContext;
        }

        List<string> getReaders(IntPtr hContext, bool isInit)
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

            if (isInit)
            {
                Debug.Log("【接続】リーダー名: " + readerNameMultiString);
            }

            List<string> readersList = new List<string>();
            int nullindex = readerNameMultiString.IndexOf((char)0);   // 装置は１台のみ
            readersList.Add(readerNameMultiString.Substring(0, nullindex));
            return readersList;
        }

        NfcApi.SCARD_READERSTATE[] readerStateChange(IntPtr hContext, List<string> readerNameList)
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
                //Debug.Log("error...");
            }
            if (nfc_state == NfcConstant.NFC_STATE_DISCONNECT)
            {
                Debug.LogWarning("リーダーの状態の取得に失敗。code = " + ret);
            }
            return readerStateArray;
        }

        ReadResult readCard(IntPtr eContext, string readerName)
        {
            IntPtr hCard = connect(eContext, readerName);
            string readerSerialNumber = readReaderSerialNumber(hCard);
            string cardId = readCardId(hCard);
            Debug.Log(readerName + " (S/N " + readerSerialNumber + ") から、カードを読み取りました。" + cardId);
            disconnect(hCard);

            ReadResult.readerSerialNumber = readerSerialNumber;
            ReadResult.cardId = cardId;
            return null;

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
            long lResult;

            byte[] response = new byte[2048];
            byte[] commnadSelectFile = { 0xff, 0xA4, 0x00, 0x01, 0x02, 0x0b, 0x10 };
            byte[] commnadReadBinary = { 0xff, 0xb0, 0x00, 0x00, 0x00 };

            IntPtr SCARD_PCI_T1 = getPciT1();
            NfcApi.SCARD_IO_REQUEST ioRecv = new NfcApi.SCARD_IO_REQUEST();
            ioRecv.cbPciLength = 2048;
            IntPtr hCard = connect(hContext, readerName);

            dwResponseSize = response.Length;
            lResult = NfcApi.SCardTransmit(hCard, SCARD_PCI_T1, commnadSelectFile, commnadSelectFile.Length, ioRecv, response, ref dwResponseSize);

            if (lResult != NfcConstant.SCARD_S_SUCCESS)
            {
                //Debug.Log("SelectFile error");
                return;
            }
            dwResponseSize = response.Length;

            lResult = NfcApi.SCardTransmit(hCard, SCARD_PCI_T1, commnadReadBinary, commnadReadBinary.Length, ioRecv, response, ref dwResponseSize);

            if (lResult != NfcConstant.SCARD_S_SUCCESS)
            {
                Debug.Log("ReadBinary error");
                return;
            }
            parse_tag(response);
        }
        private void parse_tag(byte[] data)
        {
            Debug.Log("data string : " + BitConverter.ToString(data,0, 8));
         
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
                Debug.LogWarning("カードに接続できません。code = " + ret);
            }
            return hCard;
        }

        void disconnect(IntPtr hCard)
        {
            uint ret = NfcApi.SCardDisconnect(hCard, NfcConstant.SCARD_LEAVE_CARD);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                Debug.LogWarning("カードとの接続を切断できません。code = " + ret);
            }
        }

        int control(IntPtr hCard, int controlCode, byte[] sendBuffer, byte[] recvBuffer)
        {
            int bytesReturned = 0;
            uint ret = NfcApi.SCardControl(hCard, controlCode, sendBuffer, sendBuffer.Length, recvBuffer, recvBuffer.Length, ref bytesReturned);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                Debug.LogWarning("カードへの制御命令送信に失敗しました。code = " + ret);
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
                Debug.LogWarning("カードへの送信に失敗しました。code = " + ret);
            }
            return pcbRecvLength; // 受信したバイト数(recvBufferに受け取ったバイト数)
        }
    }
    /// <summary>
    /// 読み取った情報の結果を保持する
    /// </summary>
    class ReadResult
    {
        /// <summary>
        /// リーダーの製造番号
        /// </summary>
        public static string readerSerialNumber;
        /// <summary>
        /// カードのID
        /// </summary>
        public static string cardId;
        /// <summary>
        /// カードの残高
        /// </summary>
        public static int cardBalance;
    }
}