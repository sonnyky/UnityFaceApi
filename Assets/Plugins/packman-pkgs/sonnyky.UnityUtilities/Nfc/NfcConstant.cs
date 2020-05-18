using System;

namespace P_NfcPcSc
{
    class NfcConstant
    {
        public const uint SCARD_S_SUCCESS = 0;
        public const uint SCARD_E_NO_SERVICE = 0x8010001D;
        public const uint SCARD_E_TIMEOUT = 0x8010000A;

        public const uint SCARD_SCOPE_USER = 0;

        public const int SCARD_STATE_UNAWARE = 0x0000;
        public const int SCARD_STATE_PRESENT = 0x00000020;// This implies that there is a card
        public const int SCARD_SHARE_SHARED = 0x00000002; // - This application will allow others to share the reader
        public const int SCARD_PROTOCOL_T1 = 2;// - Use the T=1 protocol (value = 0x00000002)
        public const int SCARD_LEAVE_CARD = 0; // Don't do anything special on close

        public const int NFC_STATE_DISCONNECT = 0;
        public const int NFC_STATE_CONNECT = 1;

    }
}